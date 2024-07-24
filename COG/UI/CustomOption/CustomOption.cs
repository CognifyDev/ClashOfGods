using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using AmongUs.GameOptions;
using COG.Config.Impl;
using COG.Role;
using COG.Rpc;
using COG.UI.CustomOption.ValueRules;
using COG.UI.CustomOption.ValueRules.Impl;
using COG.Utils;
using COG.Utils.Coding;
using COG.Utils.WinAPI;
using UnityEngine;
using UnityEngine.Events;
using Mode = COG.Utils.WinAPI.OpenFileDialogue.OpenFileMode;

namespace COG.UI.CustomOption;

// Code base from
// https://github.com/TheOtherRolesAU/TheOtherRoles/blob/main/TheOtherRoles/Modules/CustomOptions.cs
[DataContract]
[ShitCode]
public sealed class CustomOption
{
    [Serializable]
    public enum TabType
    {
        General = 0,
        Impostor = 1,
        Neutral = 2,
        Crewmate = 3,
        Addons = 4
    }

    public static List<CustomOption?> Options { get; } = new();

    private static int _typeId;

    public int DefaultSelection => ValueRule.DefaultSelection;
    public int ID { get; }
    public bool IsHeader { get; }
    public Func<string> RealName { get; set; }
    public TabType Page { get; }
    public CustomOption? Parent { get; }
    public object[] Selections => ValueRule.Selections;
    public IValueRule ValueRule { get; }
    public OptionBehaviour? OptionBehaviour { get; set; }

    public int Selection;

    // Option creation
    public CustomOption(TabType type, Func<string> nameGetter, IValueRule rule, CustomOption? parent, bool isHeader)
    {
        ID = _typeId;
        _typeId++;
        RealName = nameGetter;
        ValueRule = rule;
        Selection = rule.DefaultSelection;
        Parent = parent;
        IsHeader = isHeader;
        Page = type;
        Selection = 0;
        Options.Add(this);
    }

    public static CustomOption Create(TabType type, Func<string> nameGetter, IValueRule rule,
        CustomOption? parent = null, bool isHeader = false)
    {
        return new CustomOption(type, nameGetter, rule, parent, isHeader);
    }

    public static void ShareConfigs(PlayerControl? target = null)
    {
        if (PlayerUtils.GetAllPlayers().Count <= 0 || !AmongUsClient.Instance.AmHost) return;

        // 当游戏选项更改的时候调用

        var localPlayer = PlayerControl.LocalPlayer;
        PlayerControl[]? targetArr = null;
        if (target) targetArr = new[] { target! };

        // 新建写入器
        var writer = RpcUtils.StartRpcImmediately(localPlayer, KnownRpc.ShareOptions, targetArr);

        var sb = new StringBuilder();

        foreach (var option in from option in Options
                               where option != null
                               where option.Selection != option.DefaultSelection
                               select option)
        {
            sb.Append(option.ID + "|" + option.Selection);
            sb.Append(',');
        }

        writer.Write(sb.ToString().RemoveLast()).Finish();

        // id|selection,id|selection
    }

    public static void LoadOptionFromPreset(string path)
    {
        try
        {
            if (!File.Exists(path)) return;
            using StreamReader reader = new(path, Encoding.UTF8);
            while (reader.ReadLine() is { } line)
            {
                var optionInfo = line.Split(" ");
                var optionID = optionInfo[0];
                var optionSelection = optionInfo[1];

                var option = Options.FirstOrDefault(o => o?.ID.ToString() == optionID);
                if (option == null) continue;
                option.UpdateSelection(int.Parse(optionSelection));
            }
        }
        catch (System.Exception e)
        {
            Main.Logger.LogError("Error loading options: " + e);
        }
    }

    public static void SaveCurrentOption(string path)
    {
        try
        {
            var realPath = path.EndsWith(".cog") ? path : path + ".cog";
            using StreamWriter writer = new(realPath, false, Encoding.UTF8);
            foreach (var option in Options.Where(o => o is not null)
                         .OrderBy(o => o!.ID))
                writer.WriteLine(option!.ID + " " + option.Selection);
        }
        catch (System.Exception e)
        {
            Main.Logger.LogError("Error saving options: " + e);
        }
    }

    public static void SavePresetWithDialogue()
    {
        var file = OpenFileDialogue.Open(Mode.Save, "Preset File(*.cog)\0*.cog\0\0");
        if (file.FilePath is null or "") return;
        SaveCurrentOption(file.FilePath);
    }

    public static void LoadPresetWithDialogue()
    {
        var file = OpenFileDialogue.Open(Mode.Open, "Preset File(*.cog)\0*.cog\0\0");
        if (file.FilePath is null or "") return;
        LoadOptionFromPreset(file.FilePath);
    }

    public dynamic GetDynamicValue() => ValueRule.Selections[Selection];

    public bool GetBool()
    {
        if (ValueRule is BoolOptionValueRule rule)
            return rule.Selections[Selection];
        throw new NotSupportedException();
    }

    public float GetFloat()
    {
        if (ValueRule is FloatOptionValueRule rule)
            return rule.Selections[Selection];
        throw new NotSupportedException();
    }

    public int GetInt()
    {
        if (ValueRule is IntOptionValueRule rule)
            return rule.Selections[Selection];
        throw new NotSupportedException();
    }

    public string GetString()
    {
        if (ValueRule is StringOptionValueRule rule)
            return rule.Selections[Selection];
        throw new NotSupportedException();
    }

    public int GetQuantity()
    {
        return Selection + 1;
    }

    // Option changes
    public void UpdateSelection(int newSelection)
    {
        var selections = ValueRule.Selections;
        Selection = Mathf.Clamp((newSelection + selections.Length) % selections.Length, 0, selections.Length - 1);
        //if (OptionBehaviour != null && OptionBehaviour is StringOption stringOption)
        //{
        //    stringOption.oldValue = stringOption.Value = Selection;
        //    stringOption.ValueText.text = ValueRule.Selections[Selection].ToString();
        //}

        ShareOptionChange(newSelection);
    }

    public void UpdateSelection(object newValue)
    {
        if (newValue == null) return;
        var index = ValueRule.Selections.ToList().IndexOf(newValue);
        if (index != -1) UpdateSelection(index);
    }

    public void ShareOptionChange(int newSelection)
    {
        RpcUtils.StartRpcImmediately(PlayerControl.LocalPlayer, KnownRpc.UpdateOption)
            .WritePacked(ID)
            .WritePacked(newSelection)
            .Finish();
    }
}

[HarmonyPatch(typeof(GamePresetsTab))]
public static class PresetsButtonsPatch
{
    [HarmonyPatch(nameof(GamePresetsTab.Start))]
    [HarmonyPostfix]
    public static void PresetButtonHook(GamePresetsTab __instance)
    {
        var std = __instance.StandardPresetButton;
        var alter = __instance.SecondPresetButton;

        // Destroy selectable background
        DestroySelectableSprite(std.gameObject);
        DestroySelectableSprite(alter.gameObject);

        std.transform.localPosition = new(-1.5f, 0.2f, 0);
        alter.transform.localPosition = new(2.1f, 0.2f, 0);
        std.transform.localScale = alter.transform.localScale = new(1.1f, 1.1f, 1);

        void DestroySelectableSprite(GameObject go)
        {
            var trans = go.transform;
            trans.transform.FindChild("Active").FindChild("SelectionBackground").gameObject.Destroy();
            trans.transform.FindChild("Selected").FindChild("SelectionBackground").gameObject.Destroy();
        }

        // Set button text
        __instance.StandardRulesText.text = LanguageConfig.Instance.LoadPreset;
        __instance.AlternateRulesText.text = LanguageConfig.Instance.SavePreset;

        // Set button OnClick action
        std.OnClick = new();
        std.OnClick.AddListener((UnityAction)new Action(() =>
        {
            ResetActiveState(std.transform);
            CustomOption.LoadPresetWithDialogue();
        }));

        alter.OnClick = new();
        alter.OnClick.AddListener((UnityAction)new Action(() =>
        {
            ResetActiveState(alter.transform);
            CustomOption.SavePresetWithDialogue();
        }));

        __instance.PresetDescriptionText.gameObject.SetActive(false); // Hide preset introduction text
        __instance.transform.Find("DividerImage").gameObject.Destroy();

        void ResetActiveState(Transform transform)
        {
            transform.FindChild("Active").gameObject.SetActive(false);
            transform.FindChild("Inactive").gameObject.SetActive(true);
            __instance.ClickPresetButton(RulesPresets.Custom);
        }
    }

    [HarmonyPatch(nameof(GamePresetsTab.ClickPresetButton))]
    [HarmonyPrefix]
    public static void OnButtonClickAlwaysCustomPreset(ref RulesPresets preset) => preset = RulesPresets.Custom;
}

[HarmonyPatch(typeof(RolesSettingsMenu), nameof(RolesSettingsMenu.Start))]
public static class RoleOptionPatch
{
    public static void Postfix(RolesSettingsMenu __instance)
    {
        var chanceTab = __instance.transform.Find("Scroller").Find("SliderInner").Find("ChancesTab");
        chanceTab.GetAllChildren().Where(t => t.name != "CategoryHeaderMasked").ForEach(t => t.gameObject.SetActive(false));

        __instance.transform.FindChild("HeaderButtons").GetComponentsInChildren<RoleSettingsTabButton>().ForEach(btn => btn.Destroy());

        int i = 0;
        foreach (var team in Enum.GetValues<CampType>())
            SetUpCustomRoleTab(__instance, chanceTab, team, i++);
        
    }

    public static void SetUpCustomRoleTab(RolesSettingsMenu menu, Transform chanceTabTemplate, CampType camp, int index)
    {
        var initialHeaderPos = new Vector3(4.986f, 0.662f, -2f);
        var sliderInner = chanceTabTemplate.parent;
        var tab = Object.Instantiate(chanceTabTemplate, sliderInner);

        tab.localPosition = chanceTabTemplate.localPosition;
        tab.name = camp + "Tab";
        SetUpTabButton(menu, tab.gameObject, index);

        var header = Object.Instantiate(menu.categoryHeaderEditRoleOrigin, tab);
        var layer = RolesSettingsMenu.MASK_LAYER;
        header.transform.localPosition = initialHeaderPos;
        header.SetHeader(StringNames.None, layer);
        header.Title.text = camp switch
        {
            CampType.Crewmate => LanguageConfig.Instance.CrewmateCamp,
            CampType.Impostor => LanguageConfig.Instance.ImpostorCamp,
            CampType.Neutral => LanguageConfig.Instance.NeutralCamp,
            _ => "Setting"
        };
        header.Background.color = camp switch
        {
            CampType.Crewmate => Palette.CrewmateRoleHeaderBlue,
            CampType.Impostor => Palette.ImpostorRoleHeaderRed,
            _ => Color.grey
        };
        header.blankLabel.color = camp switch
        {
            CampType.Crewmate => Palette.CrewmateRoleHeaderVeryDarkBlue,
            CampType.Impostor => Palette.ImpostorRoleHeaderVeryDarkRed,
            _ => Color.grey
        };
        header.countLabel.color = header.chanceLabel.color = camp switch
        {
            CampType.Crewmate => Palette.CrewmateRoleHeaderDarkBlue,
            CampType.Impostor => Palette.ImpostorRoleHeaderDarkRed,
            _ => Color.grey
        };
        var initialX = RolesSettingsMenu.X_START_CHANCE;
        var initialY = 0.14f;
        var offsetY = RolesSettingsMenu.Y_OFFSET;
        var vanillaType = camp switch
        {
            CampType.Crewmate => RoleTeamTypes.Crewmate,
            CampType.Impostor => RoleTeamTypes.Impostor,
            CampType.Neutral => (RoleTeamTypes)99,
            _ or CampType.Unknown => (RoleTeamTypes)100
        };

        int i = 0;
        foreach (var role in CustomRoleManager.GetManager().GetRoles().Where(r => r.CampType == camp && !r.IsBaseRole))
        {
            var chanceSetting = Object.Instantiate(menu.roleOptionSettingOrigin, tab);
            var numberOption = role.RoleNumberOption;
            if (numberOption == null) continue;
            numberOption.OptionBehaviour = chanceSetting;
            chanceSetting.SetRole(GameUtils.GetGameOptions().RoleOptions,
                new()
                {
                    StringName = StringNames.None,
                    TeamType = vanillaType,
                    Role = (RoleTypes)role.Id + 100
                }, layer);
            chanceSetting.transform.localPosition = new(initialX, initialY + offsetY * i, -2f);
            chanceSetting.titleText.text = role.Name;
            chanceSetting.labelSprite.color = camp switch
            {
                CampType.Crewmate => Palette.CrewmateRoleBlue,
                CampType.Impostor => Palette.ImpostorRoleRed,
                _ => Color.grey
            };
            chanceSetting.OnValueChanged = new Action<OptionBehaviour>(ob =>
            {
                var setting = ob.Cast<RoleOptionSetting>();
                var numberOption = role.RoleNumberOption!;
                var playerCount = setting.RoleMaxCount;
                numberOption.UpdateSelection(newValue: playerCount);
                role.MainRoleOption!.UpdateSelection(playerCount != 0);
                setting.UpdateValuesAndText(null);
                HudManager.Instance.Notifier.AddRoleSettingsChangeMessage(StringNames.Roles, playerCount, 100, vanillaType);
            });
            i++;
        }
    }

    public static GameObject? CurrentTab { get; set; }

    public static void SetUpTabButton(RolesSettingsMenu menu, GameObject tab, int index)
    {
        var headerParent = menu.transform.FindChild("HeaderButtons");
        var offset = RolesSettingsMenu.X_OFFSET;
        var xStart = RolesSettingsMenu.X_START;
        var yStart = RolesSettingsMenu.TAB_Y_START;
        var button = Object.Instantiate(menu.roleSettingsTabButtonOrigin, headerParent).GetComponent<PassiveButton>();
        
        button.transform.localPosition = new(xStart + index * offset, yStart, -2);
        button.DestroyComponent<RoleSettingsTabButton>();
        button.OnClick = new();
        button.OnClick.AddListener((UnityAction)(() =>
        {
            CurrentTab?.SetActive(false);
            CurrentTab = tab;
            tab.SetActive(true);
        }));
    }
}

[HarmonyPatch(typeof(RoleOptionSetting))]
public static class RoleOptionSettingPatch
{
    [HarmonyPatch(nameof(RoleOptionSetting.UpdateValuesAndText))]
    [HarmonyPrefix]
    public static bool UpdateValuePatch(RoleOptionSetting __instance)
    {
        var option = CustomOption.Options.FirstOrDefault(o => o.OptionBehaviour == __instance);
        if (option == null) return true;
        __instance.roleMaxCount = option.GetInt();
        __instance.roleChance = 100;
        __instance.countText.text = __instance.roleMaxCount.ToString();
        __instance.chanceText.text = __instance.roleChance.ToString();
        return false;
    }
}

[HarmonyPatch(typeof(GameOptionsMenu), nameof(GameOptionsMenu.Initialize))]
public static class GameOptionsMenuPatch
{
    public static void Postfix(GameOptionsMenu __instance)
    {
        //GameUtils.SendGameMessage("新模组菜单正在开发中，请使用 /option help 命令了解详细信息。");
    }
}

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.RpcSyncSettings))]
public static class SyncVanillaSettingsPatch
{
    public static void Postfix() => CustomOption.ShareConfigs();
}

#if false


[HarmonyPatch(typeof(GameOptionsMenu), nameof(GameOptionsMenu.Update))]
internal class GameOptionsMenuUpdatePatch
{
    private const float TimerForBugFix = 1f;
    private static float _timer = 1f;

    public static void Postfix(GameOptionsMenu __instance)
    {
        // Return Menu Update if in normal among us settings 
        var gameSettingMenu = Object.FindObjectsOfType<GameSettingMenu>().FirstOrDefault();
        if (gameSettingMenu != null && (gameSettingMenu.RegularGameSettings.active ||
                                        gameSettingMenu.RolesSettings.gameObject.active)) return;

        __instance.GetComponentInParent<Scroller>().ContentYBounds.max = -0.5F + __instance.Children.Length * 0.55F;

        _timer += Time.deltaTime;
        if (_timer < 0.1f) return;

        _timer = 0f;

        if (TimerForBugFix < 3.0f) FirstOpen = false;

        var offset = 2.75f;
        var objType = new Dictionary<string, TabType>
        {
            { "GeneralSettings", TabType.General },
            { "ImpostorSettings", TabType.Impostor },
            { "NeutralSettings", TabType.Neutral },
            { "CrewmateSettings", TabType.Crewmate },
            { "AddonsSettings", TabType.Addons }
        };

        foreach (var option in Options.Where(o => o != null))
        {
            if (objType.ToList().Any(kvp => GameObject.Find(kvp.Key) && option!.Page != kvp.Value)) continue;
            if (!(option != null && option.OptionBehaviour && option.OptionBehaviour != null &&
                  option.OptionBehaviour!.gameObject)) return;
            var enabled = true;
            var parent = option!.Parent;

            while (enabled && parent != null)
            {
                enabled = parent!.Selection != 0;
                parent = parent.Parent;
            }

            option.OptionBehaviour!.gameObject.SetActive(enabled);
            if (enabled)
            {
                offset -= option.IsHeader ? 0.75f : 0.5f;
                var transform = option.OptionBehaviour.transform;
                var localPosition = transform.localPosition;
                localPosition = new Vector3(localPosition.x, offset, localPosition.z);
                transform.localPosition = localPosition;
            }
        }

        //每帧更新按钮选项名称与按下按钮操作
        foreach (var option in Options.Where(o => o != null && o.SpecialOptionType == OptionType.Button))
        {
            var toggle = (ToggleOption)option!.OptionBehaviour!;
            toggle.TitleText.text = option.Name;
            toggle.OnValueChanged = option.OnClickIfButton ?? new Action<OptionBehaviour>(_ => { });
        }
    }
}

[HarmonyPatch]
internal class HudStringPatch
{
    [HarmonyPatch(typeof(IGameOptionsExtensions), nameof(IGameOptionsExtensions.ToHudString))]
    private static void Postfix(ref string __result)
    {
        if (GameOptionsManager.Instance.currentGameOptions.GameMode == GameModes.HideNSeek)
            return; // Allow Vanilla Hide N Seek

        var pages = SidebarTextManager.GetManager().GetSidebarTexts().Count;

        if (pages <= 0) return;

        var sidebars = SidebarTextManager.GetManager().GetSidebarTexts();
        if (GameOptionsNextPagePatch.TypePage > sidebars.Count || GameOptionsNextPagePatch.TypePage == 0)
            GameOptionsNextPagePatch.TypePage = 1;

        if (sidebars.Count <= 0) return;

        var sidebar = sidebars[GameOptionsNextPagePatch.TypePage - 1];
        var text = sidebar.Title + Environment.NewLine;

        sidebar.ForResult(ref __result);
        foreach (var sidebarObject in sidebar.Objects) text += sidebarObject + Environment.NewLine;
        text += LanguageConfig.Instance.MessageForNextPage.CustomFormat(GameOptionsNextPagePatch.TypePage, pages);
        __result = text;
    }

    public static string GetOptByType(TabType type)
    {
        var txt = "";
        List<CustomOption> opt = new();
        foreach (var option in Options)
            if (option != null && option.Page == type && option.SpecialOptionType != OptionType.Button)
                opt.Add(option);
        foreach (var option in opt)
            // 临时解决方案
            try
            {
                txt += option.Name + ": " + option.Selections[option.Selection] + Environment.NewLine;
            }
            catch (IndexOutOfRangeException)
            {
            }

        return txt;
        /*
         * FIXME
         * 房主设置更新，然后如果将已经选择打开的职业关闭，那么成员客户端就会抛这个错误
         * [Error  :Il2CppInterop] During invoking native->managed trampoline
         * Exception: System.IndexOutOfRangeException: Index was outside the bounds of the array.
         * at COG.UI.CustomOption.HudStringPatch.GetOptByType(CustomOptionType type) in D:\RiderProjects\ClashOfGods\COG\UI\CustomOption\CustomOption.cs:line 652
         * at COG.UI.SidebarText.Impl.CrewmateSettings.ForResult(String& result) in D:\RiderProjects\ClashOfGods\COG\UI\SidebarText\Impl\CrewmateSettings.cs:line 15
         * at COG.Listener.Impl.OptionListener.OnIGameOptionsExtensionsDisplay(String& result) in D:\RiderProjects\ClashOfGods\COG\Listener\Impl\OptionListener.cs:line 30
         * at COG.UI.CustomOption.HudStringPatch.Postfix(String& __result) in D:\RiderProjects\ClashOfGods\COG\UI\CustomOption\CustomOption.cs:line 641
         * at DMD<IGameOptionsExtensions::ToHudString>(IGameOptions gameOptions, Int32 numPlayers)
         * at (il2cpp -> managed) ToHudString(IntPtr , Int32 , Il2CppMethodInfo* )
         */
    }
}

#endif