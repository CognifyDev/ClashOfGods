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
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using YamlDotNet.Core.Tokens;
using static UnityEngine.RemoteConfigSettingsHelper;
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
    private CustomOption(TabType type, Func<string> nameGetter, IValueRule rule, CustomOption? parent, bool isHeader)
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

        NotifySettingChange();
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

    public void NotifySettingChange()
    {
        if (OptionBehaviour is RoleOptionSetting setting)
        {
            var role = CustomRoleManager.GetManager().GetRoles().FirstOrDefault(r => r.RoleOptions.Contains(this));
            if (role == null) return;

            var color = (int)setting.Role.TeamType switch
            {
                (int)CampType.Crewmate => Palette.CrewmateSettingChangeText,
                (int)CampType.Impostor => Palette.ImpostorRed,
                _ => Color.grey
            };
            var item = TranslationController.Instance.GetString(StringNames.LobbyChangeSettingNotificationRole,
                $"<font=\"Barlow-Black SDF\" material=\"Barlow-Black Outline\">{role.Name.Color(color)}</font>",
                "<font=\"Barlow-Black SDF\" material=\"Barlow-Black Outline\">" + setting.roleMaxCount.ToString() + "</font>",
                "<font=\"Barlow-Black SDF\" material=\"Barlow-Black Outline\">" + setting.roleChance.ToString() + "%"
            );
            HudManager.Instance.Notifier.SettingsChangeMessageLogic((StringNames)int.MaxValue, item, true);
        }
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
        Main.Logger.LogInfo("======== Start to initialize custom role options... ========");

        Object.FindObjectOfType<GameSettingMenu>()?.transform.FindChild("LeftPanel")?.FindChild("RoleSettingsButton")?.GetComponent<PassiveButton>()?.SelectButton(true); // Fix button is unselected when open at the first time

        var chanceTab = __instance.transform.Find("Scroller").Find("SliderInner").Find("ChancesTab");
        chanceTab.GetAllChildren().Where(t => t.name != "CategoryHeaderMasked").ForEach(t => t.gameObject.SetActive(false));

        var headers = __instance.transform.FindChild("HeaderButtons");
        headers.GetComponentsInChildren<RoleSettingsTabButton>().ForEach(btn => btn.gameObject.Destroy());
        
        (AllButton = headers.FindChild("AllButton").GetComponent<PassiveButton>()).OnClick.AddListener((UnityAction)(() =>
        {
            Tabs.Where(go => go).ForEach(go => go.SetActive(false));
            if (AllButton) SetButtonActive(CurrentButton!, false, true);
            if (CurrentTab) CurrentTab!.SetActive(false);
        }));

        Main.Logger.LogInfo("Creating tabs...");

        int i = 0;
        foreach (var team in Enum.GetValues<CampType>())
            SetUpCustomRoleTab(__instance, chanceTab, team, i++);

        chanceTab.GetComponentInChildren<CategoryHeaderMasked>().gameObject.Destroy();
    }

    public static List<GameObject> Tabs => _tabs.Where(tab => tab).ToList();

    private static readonly List<GameObject> _tabs = new();

    public static PassiveButton? AllButton { get; set; }

    public static void SetUpCustomRoleTab(RolesSettingsMenu menu, Transform chanceTabTemplate, CampType camp, int index)
    {
        Main.Logger.LogInfo($"Creating tab for team {camp}...");

        var initialHeaderPos = new Vector3(4.986f, 0.662f, -2f);
        var sliderInner = chanceTabTemplate.parent;
        var tab = Object.Instantiate(chanceTabTemplate, sliderInner);
        Tabs.Add(tab.gameObject);

        tab.gameObject.SetActive(false);
        tab.localPosition = chanceTabTemplate.localPosition;
        var trueName = camp != CampType.Unknown ? camp.ToString() : "Addon";
        tab.name = trueName + "Tab";
        SetUpTabButton(menu, tab.gameObject, index, trueName);

        var header = Object.Instantiate(menu.categoryHeaderEditRoleOrigin, tab);
        var layer = RolesSettingsMenu.MASK_LAYER;
        header.transform.localPosition = initialHeaderPos;
        header.SetHeader(StringNames.None, layer);
        header.Title.text = camp switch
        {
            CampType.Crewmate => LanguageConfig.Instance.CrewmateCamp,
            CampType.Impostor => LanguageConfig.Instance.ImpostorCamp,
            CampType.Neutral => LanguageConfig.Instance.NeutralCamp,
            _ => "Addon"
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

        Main.Logger.LogInfo("Role header has created. Now set up role buttons...");

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
        foreach (var role in CustomRoleManager.GetManager().GetTypeCampRoles(camp).Where(r => !r.IsBaseRole))
        {
            if ((camp == CampType.Unknown && !role.IsSubRole) || !role.ShowInOptions) continue;
            var roleSetting = Object.Instantiate(menu.roleOptionSettingOrigin, tab);
            var numberOption = role.RoleNumberOption!;
            var chanceOption = role.RoleChanceOption!;
            numberOption.OptionBehaviour = chanceOption.OptionBehaviour = roleSetting;
            roleSetting.SetRole(GameUtils.GetGameOptions().RoleOptions,
                new()
                {
                    StringName = StringNames.None,
                    TeamType = vanillaType,
                    Role = (RoleTypes)role.Id + 100
                }, layer);
            roleSetting.transform.localPosition = new(initialX, initialY + offsetY * i, -2f);
            roleSetting.titleText.text = role.Name;
            roleSetting.labelSprite.color = camp switch
            {
                CampType.Crewmate => Palette.CrewmateRoleBlue,
                CampType.Impostor => Palette.ImpostorRoleRed,
                _ => Color.grey
            };
            roleSetting.OnValueChanged = new Action<OptionBehaviour>(ob =>
            {
                var setting = ob.Cast<RoleOptionSetting>();
                var numberOption = role.RoleNumberOption!;
                var chanceOption = role.RoleChanceOption!;
                var playerCount = setting.roleMaxCount;
                var roleChance = setting.roleChance;

                Main.Logger.LogInfo($"{role.GetType().Name} Num: {playerCount}p, {roleChance}%");

                numberOption.UpdateSelection(newValue: playerCount);
                chanceOption.UpdateSelection(newValue: roleChance);
                setting.UpdateValuesAndText(null);
            });

            Main.Logger.LogInfo($"Role option has set up for {role.GetType().Name}.");

            i++;
        }
    }

    public static GameObject? CurrentTab { get; set; }
    public static PassiveButton? CurrentButton { get; set; }

    public static void SetUpTabButton(RolesSettingsMenu menu, GameObject tab, int index, string imageName)
    {
        Main.Logger.LogInfo($"Setting up tab button for {tab.name} ({index})");

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
            ChangeCustomTab(menu, tab, button);
        }));

        Main.Logger.LogInfo("Button action has registeristed. Start to set button icon...");

        var renderer = button.transform.FindChild("RoleIcon").GetComponent<SpriteRenderer>();
        const string settingImagePath = "COG.Resources.InDLL.Images.Settings";

        renderer.sprite = ResourceUtils.LoadSprite(settingImagePath + "." + imageName + ".png", 35f);
    }

    static void SetButtonActive(PassiveButton obj, bool active, bool clickedAllButton = false)
    {
        if (!obj) return;
        obj.SelectButton(active);
        if (!AllButton) return;
        AllButton!.SelectButton(clickedAllButton);
        
        Main.Logger.LogInfo($"Is {obj.name} active: {active} ({clickedAllButton})");
    }

    public static void ChangeCustomTab(RolesSettingsMenu menu, GameObject newTab, PassiveButton toSelect)
    {
        CloseAllTab(menu);
        OpenTab(newTab, toSelect);
    }

    static void CloseAllTab(RolesSettingsMenu menuInstance)
    {
        menuInstance.RoleChancesSettings.SetActive(false);
        if (CurrentTab) CurrentTab!.SetActive(false); /* Don't use CurrentTab?.SetActive(false) directly because a destroyed object won't be null immediately and unity has overwritten == operator but use ? operator won't use the logic of == operator */
        if (CurrentButton) CurrentButton!.SelectButton(false);
    }

    static void OpenTab(GameObject tabToOpen, PassiveButton button)
    {
        CurrentButton = button;
        CurrentTab = tabToOpen;
        SetButtonActive(button, true);
        tabToOpen.SetActive(true);
    }
    
    [HarmonyPatch(nameof(RolesSettingsMenu.CloseMenu))]
    [HarmonyPrefix]
    public static void OnMenuClose()
    {
        SetButtonActive(CurrentButton!, false, true);
        if (CurrentTab) CurrentTab!.SetActive(false);
    }
}

[HarmonyPatch(typeof(RoleOptionSetting))]
public static class RoleOptionSettingPatch
{
    [HarmonyPatch(nameof(RoleOptionSetting.UpdateValuesAndText))]
    [HarmonyPrefix]
    public static bool UpdateValuePatch(RoleOptionSetting __instance)
    {
        var role = CustomRoleManager.GetManager().GetRoles().FirstOrDefault(r => r.RoleOptions.Any(o => o.OptionBehaviour == __instance));
        if (role == null) return true;

        var playerCount = role.RoleNumberOption!;
        var chance = role.RoleChanceOption!;

        __instance.roleMaxCount = playerCount.GetInt();
        __instance.roleChance = chance.GetInt();
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

