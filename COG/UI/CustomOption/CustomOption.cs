using AmongUs.GameOptions;
using COG.Config.Impl;
using COG.Rpc;
using COG.Utils;
using COG.Utils.Coding;
using COG.Utils.WinAPI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
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

    // Option creation
    public CustomOption(TabType type, string name, object[] selections,
        object defaultValue, CustomOption? parent, bool isHeader)
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
        Options.Add(this);

        CharacteristicCode = GetHashCode();
    }

    public static CustomOption? GetCustomOptionByCharacteristicCode(int characteristicCode)
    {
        return Options.FirstOrDefault(customOption =>
            customOption != null && customOption.CharacteristicCode == characteristicCode);
    }

    public static CustomOption Create(TabType type, string name, string[] selections,
        CustomOption? parent = null, bool isHeader = false)
    {
        return new CustomOption(type, name, selections, "", parent, isHeader);
    }

    public static CustomOption Create(TabType type, string name, float defaultValue, float min,
        float max, float step, CustomOption? parent = null, bool isHeader = false)
    {
        List<object> selections = new();
        for (var s = min; s <= max; s += step) selections.Add(s);
        return new CustomOption(type, name, selections.ToArray(), defaultValue, parent, isHeader);
    }

    public static CustomOption Create(TabType type, string name, bool defaultValue,
        CustomOption? parent = null, bool isHeader = false)
    {
        return new CustomOption(type, name,
            new object[] { LanguageConfig.Instance.Disable, LanguageConfig.Instance.Enable },
            defaultValue ? LanguageConfig.Instance.Enable : LanguageConfig.Instance.Disable, parent, isHeader);
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

    public static void SaveOptionWithDialogue()
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
        }

        ShareOptionChange(newSelection);
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
            CustomOption.SaveOptionWithDialogue();
        }));

        __instance.PresetDescriptionText.gameObject.SetActive(false); // Hide preset introduction text

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

    }
}

[HarmonyPatch(typeof(GameOptionsMenu), nameof(GameOptionsMenu.Initialize))]
public static class GameOptionsMenuPatch
{
    public static void Postfix(GameOptionsMenu __instance)
    {
        GameUtils.SendGameMessage("新模组菜单正在开发中，请使用 /option help 命令了解详细信息。");
    }
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