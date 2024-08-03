using COG.Config.Impl;
using COG.Role;
using COG.UI.CustomOption;
using COG.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace COG.Patch;

[HarmonyPatch(typeof(RolesSettingsMenu))]
public static class RoleOptionPatch
{
    public static List<GameObject> Tabs { get; } = new();

    public static CustomRole? CurrentAdvancedTabFor { get; set; }

    public static PassiveButton? AllButton { get; set; }

    public static GameObject? CurrentTab { get; set; }
    public static PassiveButton? CurrentButton { get; set; }

    public static float ScrollerLocationPercent { get; set; }

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
        chanceTab.GetAllChildren().Where(t => t.name != "CategoryHeaderMasked")
            .ForEach(t => t.gameObject.SetActive(false));

        var headers = __instance.tabParent;
        headers.GetComponentsInChildren<RoleSettingsTabButton>().ForEach(btn => btn.gameObject.Destroy());

        (AllButton = headers.FindChild("AllButton").GetComponent<PassiveButton>()).OnClick.AddListener(
            (UnityAction)(() =>
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
        __instance.roleScreenshot.sprite =
            ResourceUtils.LoadSprite("COG.Resources.InDLL.Images.Settings.General.png", 28);
        __instance.AdvancedRolesSettings.transform.FindChild("Imagebackground").GetComponent<SpriteRenderer>().color =
            new Color(1, 1, 1, 1);

        var options = __instance.advancedSettingChildren;
        foreach (var option in options)
        {
            var customOption = CustomOption.Options.Where(o => o != null)
                .FirstOrDefault(o => o!.VanillaData == option.Data);
            if (customOption == null) return;

            customOption.OptionBehaviour = option;
            option.OnValueChanged = new Action<OptionBehaviour>(_ => { });
        }
    }

    public static void SetUpCustomRoleTab(RolesSettingsMenu menu, Transform chanceTabTemplate, CampType camp, int index)
    {
        Main.Logger.LogInfo($"Creating tab for team {camp}...");

        var initialHeaderPos = new Vector3(4.986f, 0.662f, -2f);
        var sliderInner = chanceTabTemplate.parent;
        var tab = Object.Instantiate(chanceTabTemplate, sliderInner);
        Tabs.Add(tab.gameObject);
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

        if (camp is CampType.Neutral or CampType.Unknown)
            header.Title.color = Color.white;

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

        var i = 0;
        foreach (var role in CustomRoleManager.GetManager().GetTypeCampRoles(camp)
                     .Where(r => !r.IsBaseRole && r.ShowInOptions))
        {
            if ((camp == CampType.Unknown && !role.IsSubRole) || !role.ShowInOptions) continue;
            var roleSetting = Object.Instantiate(menu.roleOptionSettingOrigin, tab);
            var numberOption = role.RoleNumberOption!;
            var chanceOption = role.RoleChanceOption!;
            numberOption.OptionBehaviour = chanceOption.OptionBehaviour = roleSetting;
            roleSetting.SetRole(GameUtils.GetGameOptions().RoleOptions, role.VanillaRole, layer);
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
            passive.Colliders = collider.ToSingleElementArray();
            passive.OnMouseOut = new UnityEvent();
            passive.OnMouseOver = new UnityEvent();
            passive.OnClick = new Button.ButtonClickedEvent();

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
                    catch
                    {
                        // ignored
                    }
                }));
            }

            roleSetting.OnValueChanged = new Action<OptionBehaviour>(ob =>
            {
                var setting = ob.Cast<RoleOptionSetting>();
                var numberOption = role.RoleNumberOption!;
                var chanceOption = role.RoleChanceOption!;
                var playerCount = setting.roleMaxCount;
                var roleChance = setting.roleChance;
                numberOption.UpdateSelection(newValue: playerCount);
                chanceOption.UpdateSelection(newValue: roleChance);
                setting.UpdateValuesAndText(null);
            });
            roleSetting.ControllerSelectable.Add(passive);
            i++;
        }
    }

    public static PassiveButton SetUpTabButton(RolesSettingsMenu menu, GameObject tab, int index, string imageName,
        CampType camp)
    {
        Main.Logger.LogInfo($"Setting up tab button for {tab.name} ({index})");

        var headerParent = menu.transform.FindChild("HeaderButtons");
        var offset = RolesSettingsMenu.X_OFFSET;
        var xStart = RolesSettingsMenu.X_START;
        var yStart = RolesSettingsMenu.TAB_Y_START;
        var button = Object.Instantiate(menu.roleSettingsTabButtonOrigin, headerParent).GetComponent<PassiveButton>();

        button.transform.localPosition = new Vector3(xStart + index * offset, yStart, -2);
        button.DestroyComponent<RoleSettingsTabButton>();
        button.OnClick = new Button.ButtonClickedEvent();
        button.OnClick.AddListener((UnityAction)new Action(() =>
        {
            var elements = tab.GetComponentsInChildren<UiElement>();
            ControllerManager.Instance.OpenOverlayMenu(tab.name, menu.BackButton, elements.FirstOrDefault(),
                elements.ToList().ToIl2CppList());
            ChangeCustomTab(menu, tab, button, camp);
            // idk if code below is useful, but keeping it is not a bad idea
            menu.ControllerSelectable.Clear();
            menu.ControllerSelectable = elements.ToList().ToIl2CppList();
            ControllerManager.Instance.CurrentUiState.SelectableUiElements = menu.ControllerSelectable;
            ControllerManager.Instance.SetDefaultSelection(menu.ControllerSelectable.ToArray()[0]);
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
    }

    public static void ChangeCustomTab(RolesSettingsMenu menu, GameObject newTab, PassiveButton toSelect, CampType camp)
    {
        menu.AdvancedRolesSettings.SetActive(false);
        CurrentAdvancedTabFor = null;

        CloseAllTab(menu);
        OpenTab(newTab, toSelect);
        var scroller = menu.scrollBar;
        scroller.CalculateAndSetYBounds(
            CustomRoleManager.GetManager().GetTypeCampRoles(camp).Where(r => !r.IsBaseRole && r.ShowInOptions).ToList()
                .Count + 2, 1f, 6f, 0.43f);
        if (menu.currentTabButton != toSelect)
        {
            menu.currentTabButton = toSelect;
            scroller.ScrollToTop();
        }
        else
        {
            scroller.ScrollPercentY(ScrollerLocationPercent);
        }
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