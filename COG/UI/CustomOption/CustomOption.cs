using AmongUs.GameOptions;
using COG.Config.Impl;
using COG.Role;
using COG.Rpc;
using COG.UI.CustomOption.ValueRules;
using COG.UI.CustomOption.ValueRules.Impl;
using COG.Utils;
using COG.Utils.Coding;
using COG.Utils.WinAPI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
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

    // Option creation
    private CustomOption(TabType type, Func<string> nameGetter, IValueRule rule, CustomOption? parent, bool isHeader)
    {
        Id = _typeId;
        _typeId++;
        Name = nameGetter;
        ValueRule = rule;
        _selection = rule.DefaultSelection;
        Parent = parent;
        IsHeader = isHeader;
        Page = type;
        Selection = 0;
        Options.Add(this);
    }

    public static List<CustomOption?> Options { get; } = new();

    public int DefaultSelection => ValueRule.DefaultSelection;
    public int Id { get; }
    public bool IsHeader { get; }
    public Func<string> Name { get; set; }
    public TabType Page { get; }
    public CustomOption? Parent { get; }
    public object[] Selections => ValueRule.Selections;
    public IValueRule ValueRule { get; }
    public OptionBehaviour? OptionBehaviour { get; set; }
    public BaseGameSetting? VanillaData { get; set; }
    public int Selection
    {
        get => _selection;
        set
        {
            _selection = value;
            NotifySettingChange();
        }
    }

    private int _selection;
    private static int _typeId;

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
            sb.Append(option.Id + "|" + option.Selection);
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

                var option = Options.FirstOrDefault(o => o?.Id.ToString() == optionID);
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
                         .OrderBy(o => o!.Id))
                writer.WriteLine(option!.Id + " " + option.Selection);
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

    public dynamic GetDynamicValue()
    {
        return ValueRule.Selections[Selection];
    }

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
        Selection = newSelection;
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
            .WritePacked(Id)
            .WritePacked(newSelection)
            .Finish();
    }

    public void NotifySettingChange()
    {
        var role = CustomRoleManager.GetManager().GetRoles().FirstOrDefault(r => r.AllOptions.Contains(this));
        if (role == null) return;

        //RoleOptionSetting? setting;
        if (OptionBehaviour is RoleOptionSetting setting)
        {
            var roleName = setting.Role.TeamType switch
            {
                RoleTeamTypes.Crewmate => role.GetColorName(),
                RoleTeamTypes.Impostor => role.Name.Color(Palette.ImpostorRed),
                _ => role.Name.Color(Color.grey)
            };
            var item = TranslationController.Instance.GetString(StringNames.LobbyChangeSettingNotificationRole,
                $"<font=\"Barlow-Black SDF\" material=\"Barlow-Black Outline\">{roleName}</font>",
                "<font=\"Barlow-Black SDF\" material=\"Barlow-Black Outline\">" + setting.roleMaxCount.ToString() + "</font>",
                "<font=\"Barlow-Black SDF\" material=\"Barlow-Black Outline\">" + setting.roleChance.ToString() + "%"
            );
            HudManager.Instance.Notifier.SettingsChangeMessageLogic((StringNames)int.MaxValue - role.Id, item, true);
        }
        else
        {
            var roleName = role.CampType switch
            {
                CampType.Crewmate => role.GetColorName(),
                CampType.Impostor => role.Name.Color(Palette.ImpostorRed),
                _ => role.Name.Color(Color.grey)
            };

            string valueText = "";
            dynamic option;

            if ((option = OptionBehaviour!.GetComponent<StringOption>()) == true) // It's strange that using (OptionBehaviour is TargetType) expression is useless
                valueText = GetString();
            else if ((option = OptionBehaviour!.GetComponent<ToggleOption>()) == true) // It's also strange that it will throw exception without (== true) expression 
                valueText = TranslationController.Instance.GetString(GetBool() ? StringNames.SettingsOn : StringNames.SettingsOff);
            else
                valueText = OptionBehaviour!.Data.GetValueString(OptionBehaviour.GetInt());
                
            var item = TranslationController.Instance.GetString(StringNames.LobbyChangeSettingNotification,
                $"<font=\"Barlow-Black SDF\" material=\"Barlow-Black Outline\">{roleName}: {Name()} </font>",
                "<font=\"Barlow-Black SDF\" material=\"Barlow-Black Outline\">" + valueText + "</font>"
            );
            HudManager.Instance.Notifier.SettingsChangeMessageLogic((StringNames)int.MaxValue - Id, item, true);
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

        std.transform.localPosition = new Vector3(-1.5f, 0.2f, 0);
        alter.transform.localPosition = new Vector3(2.1f, 0.2f, 0);
        std.transform.localScale = alter.transform.localScale = new Vector3(1.1f, 1.1f, 1);

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
        std.OnClick = new Button.ButtonClickedEvent();
        std.OnClick.AddListener((UnityAction)new Action(() =>
        {
            ResetActiveState(std);
            CustomOption.LoadPresetWithDialogue();
        }));

        alter.OnClick = new Button.ButtonClickedEvent();
        alter.OnClick.AddListener((UnityAction)new Action(() =>
        {
            ResetActiveState(alter);
            CustomOption.SavePresetWithDialogue();
        }));

        __instance.PresetDescriptionText.gameObject.SetActive(false); // Hide preset introduction text
        __instance.transform.Find("DividerImage").gameObject.Destroy();

        void ResetActiveState(PassiveButton button)
        {
            button.SelectButton(false);
            __instance.ClickPresetButton(RulesPresets.Custom);
        }
    }

    [HarmonyPatch(nameof(GamePresetsTab.ClickPresetButton))]
    [HarmonyPrefix]
    public static void OnButtonClickAlwaysCustomPreset(ref RulesPresets preset)
    {
        preset = RulesPresets.Custom;
    }
}

[HarmonyPatch(typeof(RolesSettingsMenu))]
public static class RoleOptionPatch
{
    [HarmonyPatch(nameof(RolesSettingsMenu.Start))]
    [HarmonyPostfix]
    public static void OnMenuInitialization(RolesSettingsMenu __instance)
    {
        Main.Logger.LogInfo("======== Start to initialize custom role options... ========");

        // Fix button is unselected when open at the first time
        Object.FindObjectOfType<GameSettingMenu>()?.transform.FindChild("LeftPanel")?.FindChild("RoleSettingsButton")
            ?.GetComponent<PassiveButton>()?.SelectButton(true);
        __instance.AllButton.SelectButton(true);
        CurrentAdvancedTabFor = null;

        var chanceTab = __instance.scrollBar.Inner.Find("ChancesTab");
        chanceTab.GetAllChildren().Where(t => t.name != "CategoryHeaderMasked").ForEach(t => t.gameObject.SetActive(false));

        var headers = __instance.tabParent;
        headers.GetComponentsInChildren<RoleSettingsTabButton>().ForEach(btn => btn.gameObject.Destroy());

        (AllButton = headers.FindChild("AllButton").GetComponent<PassiveButton>()).OnClick.AddListener((UnityAction)(() =>
        {
            Tabs.Where(go => go).ForEach(go => go.SetActive(false));
            if (AllButton) SetButtonActive(CurrentButton!, false, true);
            if (CurrentTab) CurrentTab!.SetActive(false);
        }));

        Main.Logger.LogInfo("Creating tabs...");

        var i = 0;
        foreach (var team in Enum.GetValues<CampType>())
            SetUpCustomRoleTab(__instance, chanceTab, team, i++);

        chanceTab.GetComponentInChildren<CategoryHeaderMasked>().gameObject.Destroy();
    }

    [HarmonyPatch(nameof(RolesSettingsMenu.ChangeTab))]
    [HarmonyPostfix]
    public static void OnTabChanged(RolesSettingsMenu __instance)
    {
        Main.Logger.LogInfo($"{nameof(CurrentAdvancedTabFor)}: {CurrentAdvancedTabFor?.GetType().Name ?? "(null)"}");
        if (CurrentAdvancedTabFor == null) return;

        if (CurrentAdvancedTabFor.CampType == CampType.Neutral)
        {
            __instance.roleHeaderSprite.color = Color.grey;
            __instance.roleHeaderText.color = Color.white;
        }
        __instance.roleHeaderText.text = CurrentAdvancedTabFor.Name;
        __instance.roleDescriptionText.text = CurrentAdvancedTabFor.LongDescription;
        __instance.roleScreenshot.sprite = ResourceUtils.LoadSprite("COG.Resources.InDLL.Images.Settings.General.png", 40);
        __instance.AdvancedRolesSettings.transform.FindChild("Imagebackground").GetComponent<SpriteRenderer>().color = new(1, 1, 1, 1);

        var options = __instance.advancedSettingChildren;
        foreach (var option in options)
        {
            var customOption = CustomOption.Options.Where(o => o != null).FirstOrDefault(o => o!.VanillaData == option.Data);
            if (customOption == null) return;

            customOption.OptionBehaviour = option;
            option.OnValueChanged = new Action<OptionBehaviour>((_) => { });
        }
    }

    public static List<GameObject> Tabs => _tabs.Where(tab => tab).ToList();

    public static CustomRole? CurrentAdvancedTabFor { get; set; }

    private static readonly List<GameObject> _tabs = new();

    public static PassiveButton? AllButton { get; set; }

    public static void SetUpCustomRoleTab(RolesSettingsMenu menu, Transform chanceTabTemplate, CampType camp, int index)
    {
        Main.Logger.LogInfo($"Creating tab for team {camp}...");

        var initialHeaderPos = new Vector3(4.986f, 0.662f, -2f);
        var sliderInner = chanceTabTemplate.parent;
        var tab = Object.Instantiate(chanceTabTemplate, sliderInner);
        _tabs.Add(tab.gameObject);
        tab.GetAllChildren().Where(t => t.name != "CategoryHeaderMasked").ForEach(o => o.gameObject.Destroy());

        tab.gameObject.SetActive(false);
        tab.localPosition = chanceTabTemplate.localPosition;
        var trueName = camp != CampType.Unknown ? camp.ToString() : "Addon";
        tab.name = trueName + "Tab";
        var button = SetUpTabButton(menu, tab.gameObject, index, trueName, camp);

        var header = Object.Instantiate(menu.categoryHeaderEditRoleOrigin, tab);
        var layer = RolesSettingsMenu.MASK_LAYER;
        header.transform.localPosition = initialHeaderPos;
        header.SetHeader(StringNames.None, layer);
        header.Title.text = camp switch
        {
            CampType.Crewmate => LanguageConfig.Instance.CrewmateCamp,
            CampType.Impostor => LanguageConfig.Instance.ImpostorCamp,
            CampType.Neutral => LanguageConfig.Instance.NeutralCamp,
            _ => LanguageConfig.Instance.AddonName
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
        foreach (var role in CustomRoleManager.GetManager().GetTypeCampRoles(camp).Where(r => !r.IsBaseRole && r.ShowInOptions))
        {
            if ((camp == CampType.Unknown && !role.IsSubRole) || !role.ShowInOptions) continue;
            var roleSetting = Object.Instantiate(menu.roleOptionSettingOrigin, tab);
            var numberOption = role.RoleNumberOption!;
            var chanceOption = role.RoleChanceOption!;
            numberOption.OptionBehaviour = chanceOption.OptionBehaviour = roleSetting;
            roleSetting.SetRole(GameUtils.GetGameOptions().RoleOptions,
                new RoleBehaviour
                {
                    StringName = StringNames.None,
                    TeamType = vanillaType,
                    Role = (RoleTypes)role.Id + 100
                }, layer);
            roleSetting.transform.localPosition = new Vector3(initialX, initialY + offsetY * i, -2f);
            roleSetting.titleText.text = role.Name;
            var label = roleSetting.labelSprite;
            var color = label.color = camp switch
            {
                CampType.Crewmate => Palette.CrewmateRoleBlue,
                CampType.Impostor => Palette.ImpostorRoleRed,
                _ => Color.grey
            };
            var collider = label.gameObject.AddComponent<BoxCollider2D>();
            collider.offset = Vector2.zero;
            collider.size = label.size;

            var passive = label.gameObject.AddComponent<PassiveButton>();
            passive.Colliders = ((Collider2D)collider).ToSingleElementArray().ToIl2CppArray();
            passive.OnMouseOut = new();
            passive.OnMouseOver = new();
            passive.OnClick = new();

            if (role.RoleOptions.Count != 0)
            {
                passive.OnMouseOut.AddListener((UnityAction)new Action(() => label.color = color));
                passive.OnMouseOver.AddListener((UnityAction)new Action(() =>
                {
                    Color.RGBToHSV(color, out var h, out var s, out var v);
                    label.color = Color.HSVToRGB(h, s, v / 2);
                }));
                passive.AddOnClickListeners(new Action(() =>
                {
                    CloseAllTab(menu);
                    CurrentAdvancedTabFor = role;
                    var scroller = menu.scrollBar;
                    ScrollerLocationPercent = scroller.GetScrollPercY();
                    scroller.ScrollToTop();
                    try
                    {
                        menu.ChangeTab(role.VanillaCategory, button);
                    }
                    catch { } // Ignored
                }));
            }

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
            roleSetting.ControllerSelectable.Add(passive);

            Main.Logger.LogInfo($"Role option has set up for {role.GetType().Name}.");

            i++;
        }
    }

    public static GameObject? CurrentTab { get; set; }
    public static PassiveButton? CurrentButton { get; set; }

    public static PassiveButton SetUpTabButton(RolesSettingsMenu menu, GameObject tab, int index, string imageName, CampType camp)
    {
        Main.Logger.LogInfo($"Setting up tab button for {tab.name} ({index})");

        var headerParent = menu.transform.FindChild("HeaderButtons");
        var offset = RolesSettingsMenu.X_OFFSET;
        var xStart = RolesSettingsMenu.X_START;
        var yStart = RolesSettingsMenu.TAB_Y_START;
        var button = Object.Instantiate(menu.roleSettingsTabButtonOrigin, headerParent).GetComponent<PassiveButton>();

        button.transform.localPosition = new(xStart + index * offset, yStart, -2);
        button.DestroyComponent<RoleSettingsTabButton>();
        button.OnClick = new Button.ButtonClickedEvent();
        button.OnClick.AddListener((UnityAction)new Action(() =>
        {
            var elements = tab.GetComponentsInChildren<UiElement>();
            ControllerManager.Instance.OpenOverlayMenu(tab.name, menu.BackButton, elements.FirstOrDefault(), elements.ToList().ToIl2CppList());
            ChangeCustomTab(menu, tab, button, camp);
            menu.ControllerSelectable.Clear();
            menu.ControllerSelectable = elements.ToList().ToIl2CppList();
            ControllerManager.Instance.CurrentUiState.SelectableUiElements = menu.ControllerSelectable;
            ControllerManager.Instance.SetDefaultSelection(menu.ControllerSelectable.ToArray()[0], null);
        }));

        Main.Logger.LogInfo("Button action has registered. Start to set button icon...");

        var renderer = button.transform.FindChild("RoleIcon").GetComponent<SpriteRenderer>();
        const string settingImagePath = "COG.Resources.InDLL.Images.Settings";

        renderer.sprite = ResourceUtils.LoadSprite(settingImagePath + "." + imageName + ".png", 35f);
        return button;
    }

    private static void SetButtonActive(PassiveButton obj, bool active, bool clickedAllButton = false)
    {
        if (!obj) return;
        obj.SelectButton(active);
        if (!AllButton) return;
        AllButton!.SelectButton(clickedAllButton);

        Main.Logger.LogInfo($"Is {obj.name} active: {active} ({clickedAllButton})");
    }

    static float ScrollerLocationPercent { get; set; } = 0f;

    public static void ChangeCustomTab(RolesSettingsMenu menu, GameObject newTab, PassiveButton toSelect, CampType camp)
    {
        menu.AdvancedRolesSettings.SetActive(false);
        CurrentAdvancedTabFor = null;
        Main.Logger.LogInfo($"{nameof(CurrentAdvancedTabFor)}: {CurrentAdvancedTabFor?.GetType().Name ?? "(null)"} (It should be null now)");

        CloseAllTab(menu);
        OpenTab(newTab, toSelect);
        var scroller = menu.scrollBar;
        scroller.CalculateAndSetYBounds(CustomRoleManager.GetManager().GetTypeCampRoles(camp).Where(r => !r.IsBaseRole && r.ShowInOptions).ToList().Count + 2, 1f, 6f, 0.43f);
        if (menu.currentTabButton != toSelect)
        {
            menu.currentTabButton = toSelect;
            scroller.ScrollToTop();
        }
        else
            scroller.ScrollPercentY(ScrollerLocationPercent);
    }

    private static void CloseAllTab(RolesSettingsMenu menuInstance)
    {
        menuInstance.RoleChancesSettings.SetActive(false);
        if (CurrentTab)
            CurrentTab!.SetActive(
                false); /* Don't use CurrentTab?.SetActive(false) directly because a destroyed object won't be null immediately and unity has overwritten == operator but use ? operator won't use the logic of == operator */
        if (CurrentButton) CurrentButton!.SelectButton(false);
    }

    private static void OpenTab(GameObject tabToOpen, PassiveButton button)
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
        var role = CustomRoleManager.GetManager().GetRoles().FirstOrDefault(r => r.AllOptions.Any(o => o.OptionBehaviour == __instance));
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

[HarmonyPatch]
public static class OptionBehaviourPatch
{
    static bool TryGetCustomOption(OptionBehaviour option, out CustomOption custom)
    {
        custom = CustomOption.Options.FirstOrDefault(o => o?.OptionBehaviour == option)!;
        return custom != null;
    }

    [HarmonyPatch(typeof(NumberOption), nameof(NumberOption.UpdateValue))]
    [HarmonyPrefix]
    static bool NumberOptionValueUpdatePatch(NumberOption __instance)
    {
        if (!TryGetCustomOption(__instance, out var customOption)) return true;
        var rule = customOption.ValueRule;
        customOption.UpdateSelection(newValue: rule is FloatOptionValueRule ? __instance.GetFloat() : __instance.GetInt());
        return false;
    }

    [HarmonyPatch(typeof(StringOption), nameof(StringOption.UpdateValue))]
    [HarmonyPrefix]
    static bool StringOptionValueUpdatePatch(StringOption __instance)
    {
        if (!TryGetCustomOption(__instance, out var customOption)) return true;
        customOption.UpdateSelection(__instance.GetInt());
        return false;
    }

    [HarmonyPatch(typeof(ToggleOption), nameof(ToggleOption.UpdateValue))]
    [HarmonyPrefix]
    static bool ToggleOptionValueUpdatePatch(ToggleOption __instance)
    {
        if (!TryGetCustomOption(__instance, out var customOption)) return true;
        customOption.UpdateSelection(__instance.GetBool());
        return false;
    }

    const string InitializeName = nameof(OptionBehaviour.Initialize);

    [HarmonyPatch(typeof(NumberOption), InitializeName)]
    [HarmonyPatch(typeof(StringOption), InitializeName)]
    [HarmonyPatch(typeof(ToggleOption), InitializeName)]
    [HarmonyPostfix]
    static void OptionNamePatch(OptionBehaviour __instance)
    {
        if (!TryGetCustomOption(__instance, out var customOption)) return;
        var titleText = __instance.transform.FindChild("Title Text").GetComponent<TextMeshPro>();
        titleText.text = customOption.Name();
        if (__instance is ToggleOption toggle)
            toggle.CheckMark.enabled = customOption.GetBool();
        else if (__instance is NumberOption number)
        {
            number.oldValue = float.MinValue;
            number.Value = (float)customOption.GetDynamicValue();
        }
        else if (__instance is StringOption option)
            option.Value = customOption.Selection;
    }

    [HarmonyPatch(typeof(StringOption), nameof(StringOption.FixedUpdate))]
    [HarmonyPrefix]
    static bool StringOptionValueTextPatch(StringOption __instance)
    {
        if (!TryGetCustomOption(__instance, out var customOption)) return true;
        if (__instance.oldValue != __instance.Value)
        {
            __instance.oldValue = __instance.Value;
            __instance.ValueText.text = customOption.GetString();
        }
        return false;
    }
}

[HarmonyPatch(typeof(GameSettingMenu), nameof(GameSettingMenu.Close))]
public static class SettingMenuClosePatch
{
    public static void Postfix() => ControllerManager.Instance.CurrentUiState.MenuName = "";
}

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.RpcSyncSettings))]
public static class SyncVanillaSettingsPatch
{
    public static void Postfix()
    {
        CustomOption.ShareConfigs();
    }
}