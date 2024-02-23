using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using AmongUs.GameOptions;
using COG.Config.Impl;
using COG.Constant;
using COG.Rpc;
using COG.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static COG.UI.CustomOption.CustomOption;
using Object = UnityEngine.Object;
using Mode = COG.Utils.WinAPI.OpenFileDialogue.OpenFileMode;
using COG.UI.SidebarText;
using COG.Utils.WinAPI;

namespace COG.UI.CustomOption;

// Code base from
// https://github.com/TheOtherRolesAU/TheOtherRoles/blob/main/TheOtherRoles/Modules/CustomOptions.cs
[DataContract]
public sealed class CustomOption
{
    [Serializable]
    public enum CustomOptionType
    {
        General = 0,
        Impostor = 1,
        Neutral = 2,
        Crewmate = 3,
        Addons = 4
    }

    internal static bool FirstOpen = true;

    public static readonly List<CustomOption?> Options = new();

    public int Selection;

    public OptionBehaviour? OptionBehaviour;

    public readonly int DefaultSelection;

    public readonly int ID;

    public readonly bool IsHeader;

    public readonly string Name;

    public readonly CustomOption? Parent;

    public readonly object[] Selections;

    public readonly CustomOptionType Type;

    public readonly int CharacteristicCode;

    public bool Ignore;

    private static int _typeId;

    public static CustomOption? GetCustomOptionByCharacteristicCode(int characteristicCode)
    {
        return Options.FirstOrDefault(customOption =>
            customOption != null && customOption.CharacteristicCode == characteristicCode);
    }

    // Option creation
    public CustomOption(bool ignore, CustomOptionType type, string name, object[] selections,
        object defaultValue, CustomOption? parent, bool isHeader)
    {
        Ignore = ignore;
        ID = _typeId;
        _typeId++;
        Name = parent == null ? name : ColorUtils.ToColorString(Color.gray, "→ ") + name;
        Selections = selections;
        var index = Array.IndexOf(selections, defaultValue);
        DefaultSelection = index >= 0 ? index : 0;
        Selection = DefaultSelection;
        Parent = parent;
        IsHeader = isHeader;
        Type = type;
        Selection = 0;
        Options.Add(this);

        CharacteristicCode = GetHashCode();
    }

    public static CustomOption Create(bool ignore, CustomOptionType type, string name, string[] selections,
        CustomOption? parent = null, bool isHeader = false)
    {
        return new CustomOption(ignore, type, name, selections, "", parent, isHeader);
    }

    public static CustomOption? Create(bool ignore, CustomOptionType type, string name, float defaultValue, float min,
        float max, float step, CustomOption? parent = null, bool isHeader = false)
    {
        List<object> selections = new();
        for (var s = min; s <= max; s += step) selections.Add(s);
        return new CustomOption(ignore, type, name, selections.ToArray(), defaultValue, parent, isHeader);
    }

    public static CustomOption Create(bool ignore, CustomOptionType type, string name, bool defaultValue,
        CustomOption? parent = null, bool isHeader = false)
    {
        return new CustomOption(ignore, type, name,
            new object[] { LanguageConfig.Instance.Disable, LanguageConfig.Instance.Enable },
            defaultValue ? LanguageConfig.Instance.Enable : LanguageConfig.Instance.Disable, parent, isHeader);
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
                 where !option.Ignore
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
            foreach (var option in Options.Where(o => o is { Ignore: false }).OrderBy(o => o!.ID))
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

    [HarmonyPatch(typeof(GameOptionsMenu), nameof(GameOptionsMenu.Start))]
    private class GameOptionsMenuStartPatch
    {
        public static void Postfix(GameOptionsMenu __instance)
        {
            CreateClassicTabs(__instance);
        }

        private static void CreateClassicTabs(GameOptionsMenu instance)
        {
            var allTypes = Enum.GetValues<CustomOptionType>();
            var typeNameDictionary = new Dictionary<CustomOptionType, string>();
            allTypes.ToList().ForEach(t => typeNameDictionary[t] = t.ToString() + "Settings");
            var isReturn = SetNames(
                new Dictionary<string, string>
                {
                    ["GeneralSettings"] = LanguageConfig.Instance.GeneralSetting,
                    ["ImpostorSettings"] = LanguageConfig.Instance.ImpostorRolesSetting,
                    ["NeutralSettings"] = LanguageConfig.Instance.NeutralRolesSetting,
                    ["CrewmateSettings"] = LanguageConfig.Instance.CrewmateRolesSetting,
                    ["AddonsSettings"] = LanguageConfig.Instance.AddonsSetting
                });

            if (isReturn) return;

            // Setup COG tab
            var template = Object.FindObjectsOfType<StringOption>().FirstOrDefault();
            if (template == null) return;

            var gameSettings = GameObject.Find("Main Camera/PlayerOptionsMenu(Clone)").transform
                .FindChild("Game Settings");
            var gameSettingMenu = Object.FindObjectsOfType<GameSettingMenu>().FirstOrDefault();

            var settingsMenu = new Dictionary<CustomOptionType, (GameObject?, GameOptionsMenu?)>();
            foreach (var (type, name) in typeNameDictionary)
            {
                var setting = Object.Instantiate(gameSettings, gameSettings.transform.parent);
                var menu = GetMenu(setting.gameObject, name);
                settingsMenu[type] = (setting.gameObject, menu);
            }

            var roleTab = GameObject.Find("RoleTab");
            var gameTab = GameObject.Find("GameTab");

            var cogTab = Object.Instantiate(roleTab, roleTab.transform.parent);
            var cogTabHighlight = GetTabHighlight(cogTab, "COGTab", "COG.Resources.InDLL.Images.Setting.COG.png");

            var impostorTab = Object.Instantiate(roleTab, cogTab.transform);
            var impostorTabHighlight = GetTabHighlight(impostorTab, "ImpostorTab",
                "COG.Resources.InDLL.Images.Setting.Imposter.png");

            var neutralTab = Object.Instantiate(roleTab, impostorTab.transform);
            var neutralTabHighlight =
                GetTabHighlight(neutralTab, "NeutralTab", "COG.Resources.InDLL.Images.Setting.Neutral.png");

            var crewmateTab = Object.Instantiate(roleTab, neutralTab.transform);
            var crewmateTabHighlight = GetTabHighlight(crewmateTab, "CrewmateTab",
                "COG.Resources.InDLL.Images.Setting.Crewmate.png");

            var modifierTab = Object.Instantiate(roleTab, crewmateTab.transform);
            var modifierTabHighlight = GetTabHighlight(modifierTab, "ModifierTab",
                "COG.Resources.InDLL.Images.Setting.SubRole.png");

            // Position of Tab Icons
            gameTab.transform.position += Vector3.left * 3f;
            roleTab.transform.position += Vector3.left * 3f;
            cogTab.transform.position += Vector3.left * 2f;
            impostorTab.transform.localPosition = Vector3.right * 1f;
            neutralTab.transform.localPosition = Vector3.right * 1f;
            crewmateTab.transform.localPosition = Vector3.right * 1f;
            modifierTab.transform.localPosition = Vector3.right * 1f;

            var tabs = new[] { gameTab, roleTab, cogTab, impostorTab, neutralTab, crewmateTab, modifierTab };
            if (gameSettingMenu != null)
            {
                var settingsHighlightMap = new Dictionary<GameObject, SpriteRenderer>
                {
                    [gameSettingMenu.RegularGameSettings] = gameSettingMenu.GameSettingsHightlight,
                    [gameSettingMenu.RolesSettings.gameObject] = gameSettingMenu.RolesSettingsHightlight,
                    [settingsMenu[CustomOptionType.General].Item1!.gameObject] = cogTabHighlight,
                    [settingsMenu[CustomOptionType.Impostor].Item1!.gameObject] = impostorTabHighlight,
                    [settingsMenu[CustomOptionType.Neutral].Item1!.gameObject] = neutralTabHighlight,
                    [settingsMenu[CustomOptionType.Crewmate].Item1!.gameObject] = crewmateTabHighlight,
                    [settingsMenu[CustomOptionType.Addons].Item1!.gameObject] = modifierTabHighlight
                };

                var a = 0;
                foreach (var tab in tabs)
                {
                    var button = tab.GetComponentInChildren<PassiveButton>();
                    if (button == null) continue;
                    var copiedIndex = a;
                    button.OnClick = new Button.ButtonClickedEvent();
                    button.OnClick.AddListener((Action)(() =>
                    {
                        if (settingsHighlightMap == null!) return;
                        SetListener(settingsHighlightMap, copiedIndex);
                    }));
                    a++;
                }
            }

            var typeOptions = settingsMenu.ToDictionary(kvp => kvp.Key,
                kvp => kvp.Value.Item2!.GetComponentsInChildren<OptionBehaviour>().ToList());

            DestroyOptions(typeOptions.Select(kvp => kvp.Value).ToList());

            var menus = settingsMenu.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Item2!.transform);

            foreach (var option in Options.Where(option => option == null || (int)option.Type <= 4))
            {
                if (option?.OptionBehaviour == null && option != null)
                {
                    if (!option.Ignore)
                    {
                        var stringOption = Object.Instantiate(template, menus[option.Type]);
                        typeOptions[option.Type].Add(stringOption);
                        stringOption.OnValueChanged = new Action<OptionBehaviour>(_ => { });
                        stringOption.TitleText.text = stringOption.name = option.Name;
                        if (FirstOpen)
                            stringOption.Value = stringOption.oldValue = option.Selection = option.DefaultSelection;
                        else
                            stringOption.Value = stringOption.oldValue = option.Selection;

                        stringOption.ValueText.text = option.Selections[option.Selection].ToString();

                        option.OptionBehaviour = stringOption;
                    }
                    else // 对预设用选项处理
                    {
                        var templateToggle = GameObject.Find("ResetToDefault")?.GetComponent<ToggleOption>();
                        if (!templateToggle) return;

                        var strOpt = Object.Instantiate(templateToggle, menus[option.Type]);
                        strOpt!.transform.Find("CheckBox")?.gameObject.SetActive(false);
                        strOpt.TitleText.transform.localPosition = Vector3.zero;
                        strOpt.name = option.Name;

                        option.OptionBehaviour = strOpt;
                    }
                }

                if (option?.OptionBehaviour != null) option.OptionBehaviour.gameObject.SetActive(true);
            }

            SetOptions(
                settingsMenu.Select(kvp => kvp.Value.Item2!).ToList(),
                typeOptions.Select(kvp => kvp.Value).ToList(),
                settingsMenu.Select(kvp => kvp.Value.Item1!).ToList()
            );
        }

        private static void SetListener(Dictionary<GameObject, SpriteRenderer> settingsHighlightMap, int index)
        {
            foreach (var entry in settingsHighlightMap)
            {
                if (entry.Key == null || entry.Value == null) continue;
                entry.Key.SetActive(false);
                entry.Value.enabled = false;
            }

            settingsHighlightMap.ElementAt(index).Key.SetActive(true);
            settingsHighlightMap.ElementAt(index).Value.enabled = true;
        }

        private static void DestroyOptions(List<List<OptionBehaviour>> optionBehavioursList)
        {
            foreach (var option in optionBehavioursList.SelectMany(optionBehaviours => optionBehaviours))
                Object.Destroy(option.gameObject);
        }

        private static bool SetNames(Dictionary<string, string> gameObjectNameDisplayNameMap)
        {
            foreach (var entry in gameObjectNameDisplayNameMap)
                if (GameObject.Find(entry.Key) != null)
                {
                    // Settings setup has already been performed, fixing the title of the tab and returning
                    GameObject.Find(entry.Key).transform.FindChild("GameGroup").FindChild("Text")
                        .GetComponent<TextMeshPro>().SetText(entry.Value);
                    return true;
                }

            return false;
        }

        private static GameOptionsMenu GetMenu(GameObject setting, string settingName)
        {
            var menu = setting.transform.FindChild("GameGroup").FindChild("SliderInner")
                .GetComponent<GameOptionsMenu>();
            setting.name = settingName;

            return menu;
        }

        private static SpriteRenderer GetTabHighlight(GameObject tab, string tabName, string tabSpritePath)
        {
            var tabHighlight = tab.transform.FindChild("Hat Button").FindChild("Tab Background")
                .GetComponent<SpriteRenderer>();
            tab.transform.FindChild("Hat Button").FindChild("Icon").GetComponent<SpriteRenderer>().sprite =
                ResourceUtils.LoadSprite(tabSpritePath, 100f);
            tab.name = "tabName";

            return tabHighlight;
        }

        private static SpriteRenderer GetTabHighlight(GameObject tab, string tabName, Sprite tabSprite)
        {
            var tabHighlight = tab.transform.FindChild("Hat Button").FindChild("Tab Background")
                .GetComponent<SpriteRenderer>();
            tab.transform.FindChild("Hat Button").FindChild("Icon").GetComponent<SpriteRenderer>().sprite = tabSprite;
            tab.name = "tabName";

            return tabHighlight;
        }

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
            option?.OptionBehaviour == __instance && !option.Ignore);
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
    private static float _timer = 1f;
    private const float TimerForBugFix = 1f;

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
        var objType = new Dictionary<string, CustomOptionType>
        {
            { "GeneralSettings", CustomOptionType.General },
            { "ImpostorSettings", CustomOptionType.Impostor },
            { "NeutralSettings", CustomOptionType.Neutral },
            { "CrewmateSettings", CustomOptionType.Crewmate },
            { "AddonsSettings", CustomOptionType.Addons }
        };

        foreach (var option in Options.Where(o => o != null))
        {
            if (objType.ToList().Any(kvp => GameObject.Find(kvp.Key) && option!.Type != kvp.Value)) continue;
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


        //每帧更新预设选项名称与按下按钮操作
        var load = (ToggleOption)GlobalCustomOptionConstant.LoadPreset.OptionBehaviour!;
        var save = (ToggleOption)GlobalCustomOptionConstant.SavePreset.OptionBehaviour!;

        load!.TitleText.text = GlobalCustomOptionConstant.LoadPreset.Name;
        save!.TitleText.text = GlobalCustomOptionConstant.SavePreset.Name;

        load.OnValueChanged = new Action<OptionBehaviour>((_) => OpenPresetWithDialogue());
        save.OnValueChanged = new Action<OptionBehaviour>((_) => SaveOptionWithDialogue());
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

    public static string GetOptByType(CustomOptionType type)
    {
        var txt = "";
        List<CustomOption> opt = new();
        foreach (var option in Options)
            if (option != null && option.Type == type && !option.Ignore)
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
}