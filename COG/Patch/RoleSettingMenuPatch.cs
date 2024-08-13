using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using COG.Config.Impl;
using COG.Constant;
using COG.Role;
using COG.UI.CustomOption;
using COG.Utils;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace COG.Patch;

[HarmonyPatch(typeof(RolesSettingsMenu))]
public static class RoleOptionPatch
{
    public static CustomRole? CurrentAdvancedTabFor { get; set; }

    public static GameObject? CurrentTab { get; set; }
    public static PassiveButton? CurrentButton { get; set; }

    public static float ScrollerLocationPercent { get; set; }
    public static Dictionary<CampType, (GameObject, PassiveButton)> CampTabs { get; } = new();

    [HarmonyPatch(nameof(RolesSettingsMenu.Start))]
    [HarmonyPostfix]
    private static void OnMenuInitialization(RolesSettingsMenu __instance)
    {
        Main.Logger.LogDebug("======== Start to initialize custom role options... ========");

        // Fix button is unselected when open at the first time
        Object.FindObjectOfType<GameSettingMenu>()?.RoleSettingsButton.SelectButton(true);
        __instance.AllButton.SelectButton(true);
        CurrentAdvancedTabFor = null;

        var chanceTab = __instance.scrollBar.Inner.Find("ChancesTab");
        chanceTab.GetAllChildren().Where(t => t.name != "CategoryHeaderMasked")
            .ForEach(t => t.gameObject.SetActive(false));

        var headers = __instance.tabParent;
        headers.GetComponentsInChildren<RoleSettingsTabButton>().ForEach(btn => btn.gameObject.Destroy());

        __instance.AllButton.gameObject.SetActive(false);
        
        Main.Logger.LogDebug("Creating tabs...");
        
        var i = 0;
        foreach (var team in Enum.GetValues<CampType>())
            SetUpCustomRoleTab(__instance, chanceTab, team, i++);
        
        chanceTab.GetComponentInChildren<CategoryHeaderMasked>().gameObject.Destroy();
    }

    [HarmonyPatch(nameof(RolesSettingsMenu.ChangeTab))]
    [HarmonyPostfix]
    public static void OnTabChanged(RolesSettingsMenu __instance)
    {
        Main.Logger.LogDebug($"{nameof(CurrentAdvancedTabFor)}: {CurrentAdvancedTabFor?.GetType().Name ?? "(null)"}");
        if (CurrentAdvancedTabFor == null) return;

        if (CurrentAdvancedTabFor.CampType == CampType.Neutral)
        {
            __instance.roleHeaderSprite.color = Color.grey;
            __instance.roleHeaderText.color = Color.white;
        }

        __instance.roleHeaderText.text = CurrentAdvancedTabFor.Name;
        __instance.roleDescriptionText.text = CurrentAdvancedTabFor.GetLongDescription();
        var rolePreview = ResourceUtils.LoadSprite(
            $"COG.Resources.InDLL.Images.RolePreviews.{CurrentAdvancedTabFor.GetType().Name}.png", 
            300);
        __instance.roleScreenshot.sprite = rolePreview == null ?
            ResourceUtils.LoadSprite(ResourcesConstant.DefaultRolePreview, 185) : rolePreview;
        __instance.AdvancedRolesSettings.transform.FindChild("Imagebackground").GetComponent<SpriteRenderer>().color =
            new Color(1, 1, 1, 1);

        var options = __instance.advancedSettingChildren;
        foreach (var option in options)
        {
            var customOption = CustomOption.Options.Where(o => o != null)
                .FirstOrDefault(o => o.VanillaData == option.Data);
            if (customOption == null) return;

            customOption.OptionBehaviour = option;
            option.OnValueChanged = new Action<OptionBehaviour>(_ => { });
        }
    }

    [HarmonyPatch(nameof(RolesSettingsMenu.OpenChancesTab))]
    [HarmonyPrefix]
    public static bool OnChanceTabOpened(RolesSettingsMenu __instance)
    {
        IEnumerator CoOpenDefaultTab()
        {
            yield return null;
            var defaultCamp = CampType.Crewmate;
            var (defaultTab, defaultButton) = CampTabs[defaultCamp];
            ChangeCustomTab(__instance, defaultTab, defaultButton, defaultCamp);
        }

        __instance.StartCoroutine(CoOpenDefaultTab().WrapToIl2Cpp());
        return false;
    }


    private static void SetUpCustomRoleTab(RolesSettingsMenu menu, Transform chanceTabTemplate, CampType camp, int index)
    {
        Main.Logger.LogDebug($"Creating tab for team {camp}...");

        var headerXStart = RolesSettingsMenu.X_START_ROLE_HEADER;
        var yStart = RolesSettingsMenu.Y_START;
        var initialHeaderPos = new Vector3(headerXStart, yStart, -2f);
        var sliderInner = chanceTabTemplate.parent;
        var tab = Object.Instantiate(chanceTabTemplate, sliderInner);
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

        if (camp is CampType.Unknown or CampType.Neutral)
            header.Title.color = Color.white;

        Main.Logger.LogDebug("Role header has created. Now set up role buttons...");

        var initialX = RolesSettingsMenu.X_START_CHANCE;
        const float initialY = 0.14f;
        var offsetY = RolesSettingsMenu.Y_OFFSET;

        var i = 0;
        foreach (var role in CustomRoleManager.GetManager().GetTypeCampRoles(camp)
                     .Where(r => r is { IsBaseRole: false, ShowInOptions: true }))
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

            if (!AmongUsClient.Instance.AmHost) roleSetting.SetAsPlayer();

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
                        // Ignored
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
        var headerParent = menu.tabParent;
        Main.Logger.LogDebug($"Setting up tab button for {tab.name} ({index})");
        
        var offset = RolesSettingsMenu.X_OFFSET;
        var xStart = menu.AllButton.transform.localPosition.x;
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

        Main.Logger.LogDebug("Button action has registered. Start to set button icon...");

        var renderer = button.transform.FindChild("RoleIcon").GetComponent<SpriteRenderer>();
        const string settingImagePath = "COG.Resources.InDLL.Images.Settings";

        renderer.sprite = ResourceUtils.LoadSprite(settingImagePath + "." + imageName + ".png", 35f);
        CampTabs.TryAdd(camp, (tab, button));

        return button;
    }

    private static void SetButtonActive(PassiveButton obj, bool active)
    {
        if (!obj) return;
        obj.SelectButton(active);
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
        if (tabToOpen != null) 
            tabToOpen.SetActive(true);
    }

    [HarmonyPatch(nameof(RolesSettingsMenu.CloseMenu))]
    [HarmonyPrefix]
    public static void OnMenuClose()
    {
        SetButtonActive(CurrentButton!, false);
        if (CurrentTab) CurrentTab!.SetActive(false);
    }
}