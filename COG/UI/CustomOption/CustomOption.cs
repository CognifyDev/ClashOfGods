using System;
using System.Collections.Generic;
using System.Linq;
using COG.Config.Impl;
using COG.Listener;
using COG.Modules;
using COG.Utils;
using UnityEngine;
using static COG.UI.CustomOption.CustomOption;

namespace COG.UI.CustomOption;

// Code base from
// https://github.com/TheOtherRolesAU/TheOtherRoles/blob/main/TheOtherRoles/Modules/CustomOptions.cs
public class CustomOption
{
    internal static bool FirstOpen = true;  
  
    public enum CustomOptionType
    {
        General = 0,
        Impostor = 1,
        Neutral = 2,
        Crewmate = 3,
        Addons = 4
    }

    public static readonly List<CustomOption?> Options = new();

    public readonly int ID;
    public readonly string Name;
    public readonly System.Object[] Selections;

    public readonly int DefaultSelection;
    public int Selection;
    public OptionBehaviour OptionBehaviour;
    public readonly CustomOption? Parent;
    public readonly bool IsHeader;
    public readonly CustomOptionType Type;

    // Option creation

    public CustomOption(int id, CustomOptionType type, string name, System.Object[] selections, System.Object defaultValue, CustomOption? parent, bool isHeader)
    {
        ID = id;
        Name = parent == null ? name : ColorUtils.ToAmongUsColorString(Color.gray, "→ ") + name;
        Selections = selections;
        int index = Array.IndexOf(selections, defaultValue);
        DefaultSelection = index >= 0 ? index : 0;
        Parent = parent;
        IsHeader = isHeader;
        Type = type;
        Selection = 0;
        Options.Add(this);
    }

    public static CustomOption Create(int id, CustomOptionType type, string name, string[] selections, CustomOption? parent = null, bool isHeader = false)
    {
        return new CustomOption(id, type, name, selections, "", parent, isHeader);
    }

    public static CustomOption Create(int id, CustomOptionType type, string name, float defaultValue, float min, float max, float step, CustomOption? parent = null, bool isHeader = false)
    {
        List<object> selections = new();
        for (float s = min; s <= max; s += step) selections.Add(s);
        return new CustomOption(id, type, name, selections.ToArray(), defaultValue, parent, isHeader);
    }

    public static CustomOption Create(int id, CustomOptionType type, string name, bool defaultValue, CustomOption? parent = null, bool isHeader = false)
    {
        return new CustomOption(id, type, name, new object[] { LanguageConfig.Instance.Disable, LanguageConfig.Instance.Enable }, defaultValue ? LanguageConfig.Instance.Enable : LanguageConfig.Instance.Disable, parent, isHeader);
    }

    public static void ShareOptionChange(uint optionId)
    {
        var option = Options.FirstOrDefault(x => x.ID == optionId);
        if (option == null) return;
        var writer = AmongUsClient.Instance!.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.ShareOptions, SendOption.Reliable);
        writer.Write((byte)1);
        writer.WritePacked((uint)option.ID);
        writer.WritePacked(Convert.ToUInt32(option.Selection));
        AmongUsClient.Instance.FinishRpcImmediately(writer);
    }

    public static void ShareOptionSelections()
    {
        if (PlayerControl.AllPlayerControls.Count <= 1 || AmongUsClient.Instance!.AmHost == false && PlayerControl.LocalPlayer == null) return;
        var optionsList = new List<CustomOption?>(Options);
        while (optionsList.Any())
        {
            byte amount = (byte)Math.Min(optionsList.Count, 200); // takes less than 3 bytes per option on average
            var writer = AmongUsClient.Instance!.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.ShareOptions, SendOption.Reliable);
            writer.Write(amount);
            for (int i = 0; i < amount; i++)
            {
                var option = optionsList[0];
                optionsList.RemoveAt(0);
                if (option != null)
                {
                    writer.WritePacked((uint)option.ID);
                    writer.WritePacked(Convert.ToUInt32(option.Selection));
                }
            }
            AmongUsClient.Instance.FinishRpcImmediately(writer);
        }
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

            ShareOptionChange((uint)ID);
        }
    }
    
    [HarmonyPatch(typeof(GameOptionsMenu), nameof(GameOptionsMenu.Start))]
    class GameOptionsMenuStartPatch
    {
        public static void Postfix(GameOptionsMenu __instance)
        {
            CreateClassicTabs(__instance);
        }

        private static void CreateClassicTabs(GameOptionsMenu __instance)
        {
            bool isReturn = SetNames(
                new Dictionary<string, string>
                {
                    ["COGSettings"] = LanguageConfig.Instance.GeneralSetting,
                    ["ImpostorSettings"] = LanguageConfig.Instance.ImpostorRolesSetting,
                    ["NeutralSettings"] = LanguageConfig.Instance.NeutralRolesSetting,
                    ["CrewmateSettings"] = LanguageConfig.Instance.CrewmateRolesSetting,
                    ["AddonsSettings"] = LanguageConfig.Instance.AddonsSetting
                });

            if (isReturn) return;

            // Setup COG tab
            var template = UnityEngine.Object.FindObjectsOfType<StringOption>().FirstOrDefault();
            if (template == null) return;
            var gameSettings = GameObject.Find("Game Settings");
            var gameSettingMenu = UnityEngine.Object.FindObjectsOfType<GameSettingMenu>().FirstOrDefault();

            var parent = gameSettings.transform.parent;
            var torSettings = UnityEngine.Object.Instantiate(gameSettings, parent);
            var torMenu = GetMenu(torSettings, "COGSettings");

            var impostorSettings = UnityEngine.Object.Instantiate(gameSettings, parent);
            var impostorMenu = GetMenu(impostorSettings, "ImpostorSettings");

            var neutralSettings = UnityEngine.Object.Instantiate(gameSettings, parent);
            var neutralMenu = GetMenu(neutralSettings, "NeutralSettings");

            var crewmateSettings = UnityEngine.Object.Instantiate(gameSettings, parent);
            var crewmateMenu = GetMenu(crewmateSettings, "CrewmateSettings");

            var addonsSettings = UnityEngine.Object.Instantiate(gameSettings, parent);
            var modifierMenu = GetMenu(addonsSettings, "AddonsSettings");

            var roleTab = GameObject.Find("RoleTab");
            var gameTab = GameObject.Find("GameTab");

            var cogTab = UnityEngine.Object.Instantiate(roleTab, roleTab.transform.parent);
            var cogTabHighlight = GetTabHighlight(cogTab, "COGTab", "COG.Resources.InDLL.Images.Setting.COG.png");

            var impostorTab = UnityEngine.Object.Instantiate(roleTab, cogTab.transform);
            var impostorTabHighlight = GetTabHighlight(impostorTab, "ImpostorTab", "COG.Resources.InDLL.Images.Setting.Imposter.png");

            var neutralTab = UnityEngine.Object.Instantiate(roleTab, impostorTab.transform);
            var neutralTabHighlight = GetTabHighlight(neutralTab, "NeutralTab", "COG.Resources.InDLL.Images.Setting.Neutral.png");

            var crewmateTab = UnityEngine.Object.Instantiate(roleTab, neutralTab.transform);
            var crewmateTabHighlight = GetTabHighlight(crewmateTab, "CrewmateTab", "COG.Resources.InDLL.Images.Setting.Crewmate.png");

            var modifierTab = UnityEngine.Object.Instantiate(roleTab, crewmateTab.transform);
            var modifierTabHighlight = GetTabHighlight(modifierTab, "ModifierTab", "COG.Resources.InDLL.Images.Setting.SubRole.png");

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
                    [torSettings.gameObject] = cogTabHighlight,
                    [impostorSettings.gameObject] = impostorTabHighlight,
                    [neutralSettings.gameObject] = neutralTabHighlight,
                    [crewmateSettings.gameObject] = crewmateTabHighlight,
                    [addonsSettings.gameObject] = modifierTabHighlight
                };
                for (int i = 0; i < tabs.Length; i++)
                {
                    var button = tabs[i].GetComponentInChildren<PassiveButton>();
                    if (button == null) continue;
                    int copiedIndex = i;
                    button.OnClick = new UnityEngine.UI.Button.ButtonClickedEvent();
                    button.OnClick.AddListener((Action)(() =>
                    {
                        if (settingsHighlightMap == null || copiedIndex == null) return;
                        SetListener(settingsHighlightMap, copiedIndex);
                    }));
                }
            }

            DestroyOptions(new List<List<OptionBehaviour>>{
                Enumerable.ToList(torMenu.GetComponentsInChildren<OptionBehaviour>()),
                Enumerable.ToList(impostorMenu.GetComponentsInChildren<OptionBehaviour>()),
                Enumerable.ToList(neutralMenu.GetComponentsInChildren<OptionBehaviour>()),
                Enumerable.ToList(crewmateMenu.GetComponentsInChildren<OptionBehaviour>()),
                Enumerable.ToList(modifierMenu.GetComponentsInChildren<OptionBehaviour>())
            });

            List<OptionBehaviour> torOptions = new List<OptionBehaviour>();
            List<OptionBehaviour> impostorOptions = new List<OptionBehaviour>();
            List<OptionBehaviour> neutralOptions = new List<OptionBehaviour>();
            List<OptionBehaviour> crewmateOptions = new List<OptionBehaviour>();
            List<OptionBehaviour> modifierOptions = new List<OptionBehaviour>();


            List<Transform> menus = new List<Transform> { torMenu.transform, impostorMenu.transform, neutralMenu.transform, crewmateMenu.transform, modifierMenu.transform };
            List<List<OptionBehaviour>> optionBehaviours = new List<List<OptionBehaviour>> { torOptions, impostorOptions, neutralOptions, crewmateOptions, modifierOptions };

            foreach (var option in Options)
            {
                if (option != null && (int)option.Type > 4) continue;
                if (option?.OptionBehaviour == null)
                {
                    if (option != null)
                    {
                        StringOption stringOption = UnityEngine.Object.Instantiate(template, menus[(int)option.Type]);
                        optionBehaviours[(int)option.Type].Add(stringOption);
                        stringOption.OnValueChanged = new Action<OptionBehaviour>(_ => { });
                        stringOption.TitleText.text = option.Name;
                        if (FirstOpen)
                        {
                            stringOption.Value = stringOption.oldValue = option.Selection = option.DefaultSelection;
                        }
                        else
                        {
                            stringOption.Value = stringOption.oldValue = option.Selection;
                        }

                        stringOption.ValueText.text = option.Selections[option.Selection].ToString();

                        option.OptionBehaviour = stringOption;
                    }
                }
                option?.OptionBehaviour.gameObject.SetActive(true);
            }

            SetOptions(
                new List<GameOptionsMenu> { torMenu, impostorMenu, neutralMenu, crewmateMenu, modifierMenu },
                new List<List<OptionBehaviour>> { torOptions, impostorOptions, neutralOptions, crewmateOptions, modifierOptions },
                new List<GameObject> { torSettings, impostorSettings, neutralSettings, crewmateSettings, addonsSettings }
            );

            AdaptTaskCount(__instance);
        }

        private static void SetListener(Dictionary<GameObject, SpriteRenderer> settingsHighlightMap, int index)
        {
            foreach (KeyValuePair<GameObject, SpriteRenderer> entry in settingsHighlightMap)
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
            {
                UnityEngine.Object.Destroy(option.gameObject);
            }
        }

        private static bool SetNames(Dictionary<string, string> gameObjectNameDisplayNameMap)
        {
            foreach (KeyValuePair<string, string> entry in gameObjectNameDisplayNameMap)
            {
                if (GameObject.Find(entry.Key) != null)
                { // Settings setup has already been performed, fixing the title of the tab and returning
                    GameObject.Find(entry.Key).transform.FindChild("GameGroup").FindChild("Text").GetComponent<TMPro.TextMeshPro>().SetText(entry.Value);
                    return true;
                }
            }

            return false;
        }

        private static GameOptionsMenu GetMenu(GameObject setting, string settingName)
        {
            var menu = setting.transform.FindChild("GameGroup").FindChild("SliderInner").GetComponent<GameOptionsMenu>();
            setting.name = settingName;

            return menu;
        }

        private static SpriteRenderer GetTabHighlight(GameObject tab, string tabName, string tabSpritePath)
        {
            var tabHighlight = tab.transform.FindChild("Hat Button").FindChild("Tab Background").GetComponent<SpriteRenderer>();
            tab.transform.FindChild("Hat Button").FindChild("Icon").GetComponent<SpriteRenderer>().sprite = Utils.ResourceUtils.LoadSprite(tabSpritePath, 100f);
            tab.name = "tabName";

            return tabHighlight;
        }
        private static SpriteRenderer GetTabHighlight(GameObject tab, string tabName, Sprite tabSprite)
        {
            var tabHighlight = tab.transform.FindChild("Hat Button").FindChild("Tab Background").GetComponent<SpriteRenderer>();
            tab.transform.FindChild("Hat Button").FindChild("Icon").GetComponent<SpriteRenderer>().sprite = tabSprite;
            tab.name = "tabName";

            return tabHighlight;
        }

        private static void SetOptions(List<GameOptionsMenu> menus, List<List<OptionBehaviour>> options, List<GameObject> settings)
        {
            if (!(menus.Count == options.Count && options.Count == settings.Count))
            {
                Main.Logger.LogError("List counts are not equal");
                return;
            }
            for (int i = 0; i < menus.Count; i++)
            {
                menus[i].Children = options[i].ToArray();
                settings[i].gameObject.SetActive(false);
            }
        }

        private static void AdaptTaskCount(GameOptionsMenu __instance)
        {
            // Adapt task count for main options
            var commonTasksOption = __instance.Children.FirstOrDefault(x => x.name == "NumCommonTasks")?.TryCast<NumberOption>();
            if (commonTasksOption != null) commonTasksOption.ValidRange = new FloatRange(0f, 4f);

            var shortTasksOption = __instance.Children.FirstOrDefault(x => x.name == "NumShortTasks")?.TryCast<NumberOption>();
            if (shortTasksOption != null) shortTasksOption.ValidRange = new FloatRange(0f, 23f);

            var longTasksOption = __instance.Children.FirstOrDefault(x => x.name == "NumLongTasks")?.TryCast<NumberOption>();
            if (longTasksOption != null) longTasksOption.ValidRange = new FloatRange(0f, 15f);
        }
    }
}
[HarmonyPatch(typeof(StringOption), nameof(StringOption.OnEnable))]
public class StringOptionEnablePatch
{
    public static bool Prefix(StringOption __instance)
    {
        CustomOption? option = Options.FirstOrDefault(option => option.OptionBehaviour == __instance);
        if (option == null) return true;

        __instance.OnValueChanged = new Action<OptionBehaviour>(_ => { });
        __instance.TitleText.text = option.Name;
        
        if (FirstOpen)
        {
            __instance.Value = __instance.oldValue = option.Selection = option.DefaultSelection;
        }
        else
        {
            __instance.Value = __instance.oldValue = option.Selection;
        }
        
        __instance.ValueText.text = option.Selections[option.Selection].ToString();

        return false;
    }
}

[HarmonyPatch(typeof(StringOption))]
public class StringOptionIncreasePatch
{
    [HarmonyPatch(nameof(StringOption.Increase))]
    [HarmonyPrefix]
    public static bool IncreasePatch(StringOption __instance)
    {
        var option = Options.FirstOrDefault(option => option.OptionBehaviour == __instance);
        if (option == null) return true;

        option.UpdateSelection(option.Selection + 1);
        return false;
    }

    [HarmonyPatch(nameof(StringOption.Decrease))]
    [HarmonyPrefix]
    public static bool DecreasePatch(StringOption __instance)
    {
        var option = Options.FirstOrDefault(option => option.OptionBehaviour == __instance);
        if (option == null) return true;

        option.UpdateSelection(option.Selection - 1);
        return false;
    }
    [HarmonyPatch(nameof(StringOption.OnEnable))]
    public static bool OnEnablePatch(StringOption __instance)
    {
        CustomOption? option = Options.FirstOrDefault(option => option.OptionBehaviour == __instance);
        if (option == null) return true;

        __instance.OnValueChanged = new Action<OptionBehaviour>(_ => { });
        __instance.TitleText.text = option.Name;
        __instance.Value = __instance.oldValue = option.Selection;
        __instance.ValueText.text = option.Selections[option.Selection].ToString();

        return false;
    }
}
    
public abstract class SyncSettingPatch
{

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.RpcSyncSettings)), HarmonyPostfix]
    public static void SyncSetting()
    {
        ShareOptionSelections();
    }

    [HarmonyPatch(typeof(PlayerPhysics), nameof(PlayerPhysics.CoSpawnPlayer)), HarmonyPostfix]
    public static void SyncOnSpawnPlayer()
    {
        if (PlayerControl.LocalPlayer != null && AmongUsClient.Instance.AmHost)
        {
            ShareOptionSelections();
        }
    }
}


[HarmonyPatch(typeof(GameOptionsMenu), nameof(GameOptionsMenu.Update))]
class GameOptionsMenuUpdatePatch
{
    private static float timer = 1f;
    private static float timerForBugFix = 1f;
    public static void Postfix(GameOptionsMenu __instance)
    {
        // Return Menu Update if in normal among us settings 
        var gameSettingMenu = UnityEngine.Object.FindObjectsOfType<GameSettingMenu>().FirstOrDefault();
        if (gameSettingMenu != null && (gameSettingMenu.RegularGameSettings.active || gameSettingMenu.RolesSettings.gameObject.active)) return;

        __instance.GetComponentInParent<Scroller>().ContentYBounds.max = -0.5F + __instance.Children.Length * 0.55F;
        timer += Time.deltaTime;
        timer += Time.deltaTime;
        if (timer < 0.1f) return;

        timer = 0f;

        if (timerForBugFix < 3.0f) FirstOpen = false;

        float offset = 2.75f;
        foreach (CustomOption? option in Options)
        {
            if (GameObject.Find("COGSettings") && option.Type != CustomOptionType.General)
                continue;
            if (GameObject.Find("ImpostorSettings") && option.Type != CustomOptionType.Impostor)
                continue;
            if (GameObject.Find("NeutralSettings") && option.Type != CustomOptionType.Neutral)
                continue;
            if (GameObject.Find("CrewmateSettings") && option.Type != CustomOptionType.Crewmate)
                continue;
            if (GameObject.Find("AddonsSettings") && option.Type != CustomOptionType.Addons)
                continue;
            if (option?.OptionBehaviour != null && option.OptionBehaviour.gameObject != null)
            {
                bool enabled = true;
                var parent = option.Parent;
                while (enabled)
                {
                    if (parent != null)
                    {
                        enabled = parent.Selection != 0;
                        parent = parent.Parent;
                    } else break;
                }
                option.OptionBehaviour.gameObject.SetActive(enabled);
                if (enabled)
                {
                    offset -= option.IsHeader ? 0.75f : 0.5f;
                    var transform = option.OptionBehaviour.transform;
                    var localPosition = transform.localPosition;
                    localPosition = new Vector3(localPosition.x, offset, localPosition.z);
                    transform.localPosition = localPosition;
                }
            }
        }
    }
}


[HarmonyPatch]
class HudStringPatch
{
    [HarmonyPatch(typeof(IGameOptionsExtensions), nameof(IGameOptionsExtensions.ToHudString))]
    private static void Postfix(ref string __result)
    {
        if (GameOptionsManager.Instance.currentGameOptions.GameMode == AmongUs.GameOptions.GameModes.HideNSeek) return; // Allow Vanilla Hide N Seek

        foreach (var listener in ListenerManager.GetManager().GetListeners())
        {
            listener.OnIGameOptionsExtensionsDisplay(ref __result);
        }
    }
    
    public static string GetOptByType(CustomOptionType type)
    {
        string txt = "";
        List<CustomOption> opt = new();
        foreach (var option in Options)
        {
            if (option != null && option.Type == type)
            {
                opt.Add(option);
            }
        }
        foreach (var option in opt)
        {
            txt += option.Name + ": " + option.Selections[option.Selection] + Environment.NewLine;
        }
        return txt;
    }
}

[HarmonyPatch(typeof(KeyboardJoystick), nameof(KeyboardJoystick.Update))]
public static class GameOptionsNextPagePatch
{
    public static void Postfix(KeyboardJoystick __instance)
    {
        foreach (var listener in ListenerManager.GetManager().GetListeners())
        {
            listener.OnKeyboardJoystickUpdate(__instance);
        }
    }
}
