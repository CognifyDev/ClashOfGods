using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using AmongUs.GameOptions;
using COG.Config.Impl;
using COG.Rpc;
//using COG.UI.SidebarText;
using COG.Utils;
using COG.Utils.Coding;
using COG.Utils.WinAPI;
using Reactor.Utilities.Extensions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static COG.UI.CustomOption.CustomOption;
using Mode = COG.Utils.WinAPI.OpenFileDialogue.OpenFileMode;


namespace COG.UI.CustomOption;

// Code base from
// https://github.com/TheOtherRolesAU/TheOtherRoles/blob/main/TheOtherRoles/Modules/CustomOptions.cs
[DataContract]
[ShitCode]
public sealed class CustomOption
{
    public enum OptionType
    {
        Default = 0,
        Button = 1
    }

    [Serializable]
    public enum TabType
    {
        General = 0,
        Impostor = 1,
        Neutral = 2,
        Crewmate = 3,
        Addons = 4
    }

    internal static bool FirstOpen = true;

    public static readonly List<CustomOption?> Options = new();

    private static int _typeId;

    public readonly int CharacteristicCode;

    public readonly int DefaultSelection;

    public readonly int ID;

    public readonly bool IsHeader;

    public string Name;

    public readonly TabType Page;

    public readonly CustomOption? Parent;

    public readonly object[] Selections;

    public Action<OptionBehaviour>? OnClickIfButton;

    public OptionBehaviour? OptionBehaviour;

    public int Selection;

    public OptionType SpecialOptionType;

    // Option creation
    public CustomOption(TabType type, string name, object[] selections,
        object defaultValue, CustomOption? parent, bool isHeader, OptionType specialOptionType)
    {
        ID = _typeId;
        _typeId++;
        Name = parent == null ? name : Color.gray.ToColorString("→ ") + name;
        Selections = selections;
        var index = Array.IndexOf(selections, defaultValue);
        DefaultSelection = index >= 0 ? index : 0;
        Selection = DefaultSelection;
        Parent = parent;
        IsHeader = isHeader;
        Page = type;
        Selection = 0;
        SpecialOptionType = specialOptionType;
        Options.Add(this);

        CharacteristicCode = GetHashCode();
    }

    public static CustomOption? GetCustomOptionByCharacteristicCode(int characteristicCode)
    {
        return Options.FirstOrDefault(customOption =>
            customOption != null && customOption.CharacteristicCode == characteristicCode);
    }

    public static CustomOption Create(TabType type, string name, string[] selections,
        CustomOption? parent = null, bool isHeader = false, OptionType optionType = OptionType.Default)
    {
        return new CustomOption(type, name, selections, "", parent, isHeader, optionType);
    }

    public static CustomOption Create(TabType type, string name, float defaultValue, float min,
        float max, float step, CustomOption? parent = null, bool isHeader = false,
        OptionType optionType = OptionType.Default)
    {
        List<object> selections = new();
        for (var s = min; s <= max; s += step) selections.Add(s);
        return new CustomOption(type, name, selections.ToArray(), defaultValue, parent, isHeader, optionType);
    }

    public static CustomOption Create(TabType type, string name, bool defaultValue,
        CustomOption? parent = null, bool isHeader = false, OptionType optionType = OptionType.Default)
    {
        return new CustomOption(type, name,
            new object[] { LanguageConfig.Instance.Disable, LanguageConfig.Instance.Enable },
            defaultValue ? LanguageConfig.Instance.Enable : LanguageConfig.Instance.Disable, parent, isHeader,
            optionType);
    }

    public static void ShareConfigs(PlayerControl target)
    {
        if (PlayerUtils.GetAllPlayers().Count <= 0 || !AmongUsClient.Instance.AmHost) return;

        // 当游戏选项更改的时候调用

        var localPlayer = PlayerControl.LocalPlayer;

        // 新建写入器
        var writer = AmongUsClient.Instance.StartRpcImmediately(localPlayer.NetId, (byte)KnownRpc.ShareOptions,
            SendOption.Reliable, target.GetClientID());

        var sb = new StringBuilder();

        foreach (var option in from option in Options
                               where option != null
                               where option.SpecialOptionType != OptionType.Button
                               where option.Selection != option.DefaultSelection
                               select option)
        {
            sb.Append(option.ID + "|" + option.Selection);
            sb.Append(',');
        }

        writer.Write(sb.ToString().RemoveLast());

        // id|selection,id|selection

        // OK 现在进行一个结束
        AmongUsClient.Instance.FinishRpcImmediately(writer);
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
            foreach (var option in Options.Where(o => o is { SpecialOptionType: OptionType.Default })
                         .OrderBy(o => o!.ID))
                writer.WriteLine(option!.ID + " " + option.Selection);
        }
        catch (System.Exception e)
        {
            Main.Logger.LogError("Error saving options: " + e);
        }
    }

    public static void SaveOptionWithDialogue()
    {
        var file = OpenFileDialogue.Open(Mode.Save, "Preset File(*.cog)\0*.cog\0\0");
        if (file.FilePath is null or "") return;
        SaveCurrentOption(file.FilePath);
    }

    public static void OpenPresetWithDialogue()
    {
        var file = OpenFileDialogue.Open(Mode.Open, "Preset File(*.cog)\0*.cog\0\0");
        if (file.FilePath is null or "") return;
        LoadOptionFromPreset(file.FilePath);
    }

    public int GetSelection()
    {
        return Selection;
    }

    public bool GetBool()
    {
        return Selection > 0;
    }

    public float GetFloat()
    {
        return (float)Selections[Selection];
    }

    public int GetQuantity()
    {
        return Selection + 1;
    }

    // Option changes
    public void UpdateSelection(int newSelection)
    {
        Selection = Mathf.Clamp((newSelection + Selections.Length) % Selections.Length, 0, Selections.Length - 1);
        if (OptionBehaviour != null && OptionBehaviour is StringOption stringOption)
        {
            stringOption.oldValue = stringOption.Value = Selection;
            stringOption.ValueText.text = Selections[Selection].ToString();

            ShareOptionChange(newSelection);
        }
    }

    public void ShareOptionChange(int newSelection)
    {
        var writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId,
            (byte)KnownRpc.UpdateOption, SendOption.Reliable);
        writer.Write(ID + "|" + newSelection);
        AmongUsClient.Instance.FinishRpcImmediately(writer);
    }
    [HarmonyPatch(typeof(RolesSettingsMenu), nameof(RolesSettingsMenu.Start))]
    private static class GameOptionPatch
    {
        public static void Postfix(RolesSettingsMenu __instance)
        {
            //__instance.RoleChancesSettings

        }
    }
}
#if false
    [HarmonyPatch(typeof(GameOptionsMenu), nameof(GameOptionsMenu.Start))]
    private class GameOptionsMenuStartPatch
    {
        public static void Postfix()
        {
            CreateClassicTabs();
        }

        private static void CreateClassicTabs()
        {
            var allTypes = Enum.GetValues<TabType>();
            var typeNameDictionary = new Dictionary<TabType, string>();
            allTypes.ToList().ForEach(t => typeNameDictionary[t] = t + "Settings");
            var shouldReturn = SetTabDisplayName(
                new Dictionary<string, string>
                {
                    ["GeneralSettings"] = LanguageConfig.Instance.GeneralSetting,
                    ["ImpostorSettings"] = LanguageConfig.Instance.ImpostorRolesSetting,
                    ["NeutralSettings"] = LanguageConfig.Instance.NeutralRolesSetting,
                    ["CrewmateSettings"] = LanguageConfig.Instance.CrewmateRolesSetting,
                    ["AddonsSettings"] = LanguageConfig.Instance.AddonsSetting
                });

            if (shouldReturn) return;

            Main.Logger.LogInfo("Begin to init CustomOption");

            // Setup COG tab
            var template = Object.FindObjectsOfType<StringOption>().FirstOrDefault();
            if (template == null) return;

            var gameSettings = GameObject.Find("Main Camera/PlayerOptionsMenu(Clone)").transform
                .FindChild("Game Settings");
            var gameSettingMenu = Object.FindObjectsOfType<GameSettingMenu>().FirstOrDefault();

            var settingsMenu = new Dictionary<TabType, (GameObject?, GameOptionsMenu?)>();

            foreach (var (type, name) in typeNameDictionary)
            {
                var setting = Object.Instantiate(gameSettings, gameSettings.transform.parent);
                var menu = GetOptionMenuComponent(setting.gameObject, name);
                settingsMenu[type] = (setting.gameObject, menu);
            }

            GameObject? GetSetting(TabType tab)
            {
                if (settingsMenu!.TryGetValue(tab, out var pair))
                    return pair.Item1;
                return null;
            }

            var roleTab = GameObject.Find("RoleTab");
            var gameTab = GameObject.Find("GameTab");

            var typeTabHighlights = new Dictionary<TabType, (GameObject, SpriteRenderer)>();

            var lastTab = roleTab.transform.parent;
            foreach (var type in allTypes)
            {
                var tab = Object.Instantiate(roleTab, lastTab);
                var highlight = SetHighlightSprite(tab, $"{type}Tab", $"COG.Resources.InDLL.Images.Setting.{type}.png");

                typeTabHighlights[type] = (tab, highlight);
                lastTab = tab.transform;
            }

            (GameObject?, SpriteRenderer?) GetTabHighlightPair(TabType tab)
            {
                if (typeTabHighlights!.TryGetValue(tab, out var pair))
                    return pair;
                return (null, null);
            }

            GameObject? GetTab(TabType tab)
            {
                return GetTabHighlightPair(tab).Item1;
            }

            SpriteRenderer? GetHighlight(TabType tab)
            {
                return GetTabHighlightPair(tab).Item2;
            }

            // Position of Tab Icons
            gameTab.transform.position += Vector3.left * 3f;
            roleTab.transform.position += Vector3.left * 3f;

            GetTab(TabType.General)!.transform.position += Vector3.left * 2f;

            allTypes.Where(t => t != TabType.General).ForEach(t =>
                GetTab(t)!.transform.localPosition = Vector3.right * 1f);

            var tabs = new List<GameObject> { gameTab, roleTab };
            allTypes.ForEach(t => tabs.Add(GetTab(t)!));

            if (gameSettingMenu != null)
            {
                var settingsHighlightMap = new Dictionary<GameObject, SpriteRenderer>
                {
                    [gameSettingMenu.RegularGameSettings] = gameSettingMenu.GameSettingsHightlight,
                    [gameSettingMenu.RolesSettings.gameObject] = gameSettingMenu.RolesSettingsHightlight
                };

                foreach (var t in allTypes)
                    settingsHighlightMap.Add(GetSetting(t)!, GetHighlight(t)!);

                var a = 0;
                foreach (var tab in tabs)
                {
                    var index = a; // 如在下方方法调用直接用a会导致问题（值类型的锅）
                    var button = tab.GetComponentInChildren<PassiveButton>();
                    if (!button) continue;
                    button.OnClick = new Button.ButtonClickedEvent();
                    button.OnClick.AddListener((Action)(() =>
                    {
                        if (settingsHighlightMap == null!) return;
                        SetTabActive(settingsHighlightMap, index);
                    }));
                    a++;
                }
            }

            var typeOptions = settingsMenu.ToDictionary(kvp => kvp.Key,
                kvp => kvp.Value.Item2!.GetComponentsInChildren<OptionBehaviour>().ToList());

            // Destroy vanilla options in new tab
            DestroyOptions(typeOptions.Select(kvp => kvp.Value).ToList());

            Main.Logger.LogInfo("Finished tab initialization");

            var menus = settingsMenu.ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value.Item2!.transform);

            foreach (var option in Options.Where(option => option == null || (int)option.Page <= 4))
            {
                if (option?.OptionBehaviour == null && option != null)
                    switch (option.SpecialOptionType)
                    {
                        case OptionType.Default:
                        {
                            var stringOption = Object.Instantiate(template, menus[option.Page]);
                            typeOptions[option.Page].Add(stringOption);
                            stringOption.OnValueChanged = new Action<OptionBehaviour>(_ => { });
                            stringOption.TitleText.text = stringOption.name = option.Name;
                            if (FirstOpen)
                                stringOption.Value = stringOption.oldValue = option.Selection = option.DefaultSelection;
                            else
                                stringOption.Value = stringOption.oldValue = option.Selection;

                            stringOption.ValueText.text = option.Selections[option.Selection].ToString();

                            option.OptionBehaviour = stringOption;
                        }
                            break;
                        case OptionType.Button:
                        {
                            var templateToggle = GameObject.Find("ResetToDefault")?.GetComponent<ToggleOption>();
                            if (!templateToggle) return;

                            var strOpt = Object.Instantiate(templateToggle, menus[option.Page]);
                            strOpt!.transform.Find("CheckBox")?.gameObject.SetActive(false);
                            strOpt.TitleText.transform.localPosition = Vector3.zero;
                            strOpt.name = option.Name;

                            option.OptionBehaviour = strOpt;
                        }
                            break;
                    }

                if (option?.OptionBehaviour != null) option.OptionBehaviour.gameObject.SetActive(true);
            }

            Main.Logger.LogInfo("Finished option item initialization");

            SetOptions(
                settingsMenu.Select(kvp => kvp.Value.Item2!).ToList(),
                typeOptions.Select(kvp => kvp.Value).ToList(),
                settingsMenu.Select(kvp => kvp.Value.Item1!).ToList()
            );

            Main.Logger.LogInfo("Set up options");
        }

        /// <summary>
        ///     Open the tab.
        /// </summary>
        /// <param name="settingsHighlightMap">A dictionary that contains setting to open and the highlight of the setting</param>
        /// <param name="index">The index of the highlight in
        ///     <param name="settingsHighlightMap"></param>
        private static void SetTabActive(Dictionary<GameObject, SpriteRenderer> settingsHighlightMap, int index)
        {
            var i = 0;
            foreach (var (setting, highlight) in settingsHighlightMap)
            {
                setting.SetActive(i == index);
                highlight.enabled = i == index;
                i++;
            }

            Main.Logger.LogInfo("Opened tab: " + index);
        }

        /// <summary>
        ///     Destroy <see cref="OptionBehaviour" /> in the list.
        /// </summary>
        /// <param name="optionBehavioursList"></param>
        private static void DestroyOptions(List<List<OptionBehaviour>> optionBehavioursList)
        {
            foreach (var option in optionBehavioursList.SelectMany(optionBehaviours => optionBehaviours))
                Object.Destroy(option.gameObject);
        }

        /// <summary>
        ///     Set the title text of current tab.
        /// </summary>
        /// <param name="displayNameMap">A dictionary that contains the name and the title of the tabs.</param>
        /// <returns></returns>
        private static bool SetTabDisplayName(Dictionary<string, string> displayNameMap)
        {
            foreach (var entry in displayNameMap)
            {
                // 寻找当前打开的设置页（未被打开则是null），再设置页面标题
                GameObject obj;
                if (obj = GameObject.Find(entry.Key))
                {
                    obj.transform.FindChild("GameGroup").FindChild("Text")
                        .GetComponent<TextMeshPro>().SetText(entry.Value);
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        ///     Get <see cref="GameOptionsMenu" /> component of the setting.
        /// </summary>
        /// <param name="setting"></param>
        /// <param name="settingName">Name to set</param>
        /// <returns></returns>
        private static GameOptionsMenu GetOptionMenuComponent(GameObject setting, string settingName)
        {
            var menu = setting.transform.FindChild("GameGroup").FindChild("SliderInner")
                .GetComponent<GameOptionsMenu>();
            setting.name = settingName;

            return menu;
        }

        /// <summary>
        ///     Set the icon of the tab.
        /// </summary>
        /// <param name="tab"></param>
        /// <param name="tabName">Name to set</param>
        /// <param name="tabSprite">The location of the image</param>
        /// <returns>The highlight of the icon</returns>
        private static SpriteRenderer SetHighlightSprite(GameObject tab, string tabName, string tabSpritePath)
        {
            return SetHighlightSprite(tab, tabName, ResourceUtils.LoadSprite(tabSpritePath, 100f)!);
        }

        /// <summary>
        ///     Set the icon of the tab.
        /// </summary>
        /// <param name="tab"></param>
        /// <param name="tabName">Name to set</param>
        /// <param name="tabSprite">Icon to set</param>
        /// <returns>The highlight of the icon</returns>
        private static SpriteRenderer SetHighlightSprite(GameObject tab, string tabName, Sprite tabSprite)
        {
            var tabHighlight = tab.transform.FindChild("Hat Button").FindChild("Tab Background")
                .GetComponent<SpriteRenderer>();
            tab.transform.FindChild("Hat Button").FindChild("Icon").GetComponent<SpriteRenderer>().sprite = tabSprite;
            tab.name = tabName;

            return tabHighlight;
        }

        /// <summary>
        ///     Set up options.
        /// </summary>
        /// <param name="menus"></param>
        /// <param name="options"></param>
        /// <param name="settings"></param>
        private static void SetOptions(List<GameOptionsMenu> menus, List<List<OptionBehaviour>> options,
            List<GameObject> settings)
        {
            if (!(menus.Count == options.Count && options.Count == settings.Count))
            {
                Main.Logger.LogError("List counts are not equal");
                return;
            }

            for (var i = 0; i < menus.Count; i++)
            {
                menus[i].Children = options[i].ToArray();
                settings[i].gameObject.SetActive(false);
            }
        }
    }

    [HarmonyPatch(typeof(RolesSettingsMenu), nameof(RolesSettingsMenu.Start))]
    private class RoleSettingsMenuPatch
    {
        public const string TitleObjectName = "Text";

        public static void Postfix(RolesSettingsMenu __instance)
        {
            __instance.transform.FindChild("Right Panel").gameObject.SetActive(false);

            void SetChildrenInactiveBut(Transform transform, string name)
            {
                transform.GetAllChildren()
                    .Where(t => t.name != name).ForEach(t => t.gameObject.SetActive(false));
            }

            SetChildrenInactiveBut(__instance.transform.FindChild("Left Panel"), __instance.RoleChancesSettings.name);
            SetChildrenInactiveBut(__instance.RoleChancesSettings.transform, TitleObjectName);

            __instance.RoleChancesSettings.transform.FindChild(TitleObjectName).GetComponent<TextTranslatorTMP>()
                .Destroy();
            var titleText = __instance.RoleChancesSettings.transform.FindChild(TitleObjectName)
                .GetComponent<TextMeshPro>();
            titleText.alignment = TextAlignmentOptions.Center;
            titleText.transform.localPosition = new Vector3(2.5f, 0, 0);
            titleText.text = LanguageConfig.Instance.VanillaRoleDisabled;
        }
    }
}

[HarmonyPatch(typeof(StringOption))]
public class StringOptionPatch
{
    [HarmonyPatch(nameof(StringOption.Increase))]
    [HarmonyPrefix]
    public static bool IncreasePatch(StringOption __instance)
    {
        var option = Options.FirstOrDefault(option => option?.OptionBehaviour == __instance);
        if (option == null) return true;

        option.UpdateSelection(option.Selection + 1);
        return false;
    }

    [HarmonyPatch(nameof(StringOption.Decrease))]
    [HarmonyPrefix]
    public static bool DecreasePatch(StringOption __instance)
    {
        var option = Options.FirstOrDefault(option => option?.OptionBehaviour == __instance);
        if (option == null) return true;

        option.UpdateSelection(option.Selection - 1);
        return false;
    }

    [HarmonyPatch(nameof(StringOption.OnEnable))]
    [HarmonyPrefix]
    public static bool OnEnablePatch(StringOption __instance)
    {
        var option = Options.FirstOrDefault(option =>
            option?.OptionBehaviour == __instance && option.SpecialOptionType != OptionType.Button);
        if (option == null) return true;

        __instance.OnValueChanged = new Action<OptionBehaviour>(_ => { });
        __instance.TitleText.text = option.Name;

        //if (FirstOpen)
        //    __instance.Value = __instance.oldValue = option.Selection = option.DefaultSelection;
        //else
        __instance.Value = __instance.oldValue = option.Selection;

        __instance.ValueText.text = option.Selections[option.Selection].ToString();

        return false;
    }
}

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

[HarmonyPatch(typeof(KeyboardJoystick), nameof(KeyboardJoystick.Update))]
public static class GameOptionsNextPagePatch
{
    internal static int TypePage = 1;

    public static void Postfix(KeyboardJoystick __instance)
    {
        if (HudManager.Instance.Chat.freeChatField.textArea.hasFocus) return; // 忽略正在聊天时的按键操作
        if (Input.GetKeyDown(KeyCode.Tab)) TypePage += 1;

        // from Alpha1 to Alpha9
        for (var num = (int)KeyCode.Alpha1; num <= (int)KeyCode.Alpha9; num++)
        {
            var page = num - ((int)KeyCode.Alpha1 - 1); // 指代的page
            var keycode = (KeyCode)num;
            if (Input.GetKeyDown(keycode)) TypePage = page;
        }

        // from Keypad1 to Keypad9
        for (var num = (int)KeyCode.Keypad1; num <= (int)KeyCode.Keypad9; num++)
        {
            var page = num - ((int)KeyCode.Keypad1 - 1);
            var keycode = (KeyCode)num;
            if (Input.GetKeyDown(keycode)) TypePage = page;
        }
    }

#endif