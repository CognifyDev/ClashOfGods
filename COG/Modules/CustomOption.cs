using BepInEx.Configuration;
using HarmonyLib;
using Hazel;
using Il2CppSystem.Web.Util;
using Reactor.Networking.Rpc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static COG.Modules.CustomOption;
using UnityEngine;
using System.IO;

namespace COG.Modules
{
    //https://github.com/TheOtherRolesAU/TheOtherRoles/blob/main/TheOtherRoles/Modules/CustomOptions.cs
    public class CustomOption
    {
        public enum CustomOptionType
        {
            General,
            Impostor,
            Neutral,
            Crewmate,
            Modifier,
        }

        public static List<CustomOption> options = new List<CustomOption>();
        public static int preset = 0;

        public int id;
        public string name;
        public System.Object[] selections;

        public int defaultSelection;
        public int selection;
        public OptionBehaviour optionBehaviour;
        public CustomOption parent;
        public bool isHeader;
        public CustomOptionType type;

        // Option creation

        public CustomOption(int id, CustomOptionType type, string name, System.Object[] selections, System.Object defaultValue, CustomOption parent, bool isHeader)
        {
            this.id = id;
            this.name = parent == null ? name : "- " + name;
            this.selections = selections;
            int index = Array.IndexOf(selections, defaultValue);
            this.defaultSelection = index >= 0 ? index : 0;
            this.parent = parent;
            this.isHeader = isHeader;
            this.type = type;
            selection = 0;
            options.Add(this);
        }

        public static CustomOption Create(int id, CustomOptionType type, string name, string[] selections, CustomOption parent = null, bool isHeader = false)
        {
            return new CustomOption(id, type, name, selections, "", parent, isHeader);
        }

        public static CustomOption Create(int id, CustomOptionType type, string name, float defaultValue, float min, float max, float step, CustomOption parent = null, bool isHeader = false)
        {
            List<object> selections = new();
            for (float s = min; s <= max; s += step) selections.Add(s);
            return new CustomOption(id, type, name, selections.ToArray(), defaultValue, parent, isHeader);
        }

        public static CustomOption Create(int id, CustomOptionType type, string name, bool defaultValue, CustomOption parent = null, bool isHeader = false)
        {
            return new CustomOption(id, type, name, new string[] { "Off", "On" }, defaultValue ? "On" : "Off", parent, isHeader);
        }

        public static void ShareOptionChange(uint optionId)
        {
            var option = options.FirstOrDefault(x => x.id == optionId);
            if (option == null) return;
            var writer = AmongUsClient.Instance!.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.ShareOptions, SendOption.Reliable, -1);
            writer.Write((byte)1);
            writer.WritePacked((uint)option.id);
            writer.WritePacked(Convert.ToUInt32(option.selection));
            AmongUsClient.Instance.FinishRpcImmediately(writer);
        }

        public static void ShareOptionSelections()
        {
            if (PlayerControl.AllPlayerControls.Count <= 1 || AmongUsClient.Instance!.AmHost == false && PlayerControl.LocalPlayer == null) return;
            var optionsList = new List<CustomOption>(CustomOption.options);
            while (optionsList.Any())
            {
                byte amount = (byte)Math.Min(optionsList.Count, 200); // takes less than 3 bytes per option on average
                var writer = AmongUsClient.Instance!.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.ShareOptions, SendOption.Reliable, -1);
                writer.Write(amount);
                for (int i = 0; i < amount; i++)
                {
                    var option = optionsList[0];
                    optionsList.RemoveAt(0);
                    writer.WritePacked((uint)option.id);
                    writer.WritePacked(Convert.ToUInt32(option.selection));
                }
                AmongUsClient.Instance.FinishRpcImmediately(writer);
            }
        }

        public int getSelection()
        {
            return selection;
        }

        public bool getBool()
        {
            return selection > 0;
        }

        public float getFloat()
        {
            return (float)selections[selection];
        }

        public int getQuantity()
        {
            return selection + 1;
        }

        // Option changes

        public void updateSelection(int newSelection)
        {
            selection = Mathf.Clamp((newSelection + selections.Length) % selections.Length, 0, selections.Length - 1);
            if (optionBehaviour != null && optionBehaviour is StringOption stringOption)
            {
                stringOption.oldValue = stringOption.Value = selection;
                stringOption.ValueText.text = selections[selection].ToString();

                ShareOptionChange((uint)id);
            }
        }



        [HarmonyPatch(typeof(GameOptionsMenu), nameof(GameOptionsMenu.Start))]
        class GameOptionsMenuStartPatch
        {
            public static void Postfix(GameOptionsMenu __instance)
            {
                createClassicTabs(__instance);
            }

            private static void createClassicTabs(GameOptionsMenu __instance)
            {
                bool isReturn = setNames(
                    new Dictionary<string, string>()
                    {
                        ["COGSettings"] = "Clash Of Gods Settings",
                        ["ImpostorSettings"] = "Impostor Roles Settings",
                        ["NeutralSettings"] = "Neutral Roles Settings",
                        ["CrewmateSettings"] = "Crewmate Roles Settings",
                        ["ModifierSettings"] = "Modifier Settings",
                    });

                if (isReturn) return;

                // Setup COG tab
                var template = UnityEngine.Object.FindObjectsOfType<StringOption>().FirstOrDefault();
                if (template == null) return;
                var gameSettings = GameObject.Find("Game Settings");
                var gameSettingMenu = UnityEngine.Object.FindObjectsOfType<GameSettingMenu>().FirstOrDefault();

                var torSettings = UnityEngine.Object.Instantiate(gameSettings, gameSettings.transform.parent);
                var torMenu = getMenu(torSettings, "COGSettings");

                var impostorSettings = UnityEngine.Object.Instantiate(gameSettings, gameSettings.transform.parent);
                var impostorMenu = getMenu(impostorSettings, "ImpostorSettings");

                var neutralSettings = UnityEngine.Object.Instantiate(gameSettings, gameSettings.transform.parent);
                var neutralMenu = getMenu(neutralSettings, "NeutralSettings");

                var crewmateSettings = UnityEngine.Object.Instantiate(gameSettings, gameSettings.transform.parent);
                var crewmateMenu = getMenu(crewmateSettings, "CrewmateSettings");

                var modifierSettings = UnityEngine.Object.Instantiate(gameSettings, gameSettings.transform.parent);
                var modifierMenu = getMenu(modifierSettings, "ModifierSettings");

                var roleTab = GameObject.Find("RoleTab");
                var gameTab = GameObject.Find("GameTab");

                var cogTab = UnityEngine.Object.Instantiate(roleTab, roleTab.transform.parent);
                var cogTabHighlight = getTabHighlight(cogTab, "TheOtherRolesTab", "TheOtherRoles.Resources.TabIcon.png");

                var impostorTab = UnityEngine.Object.Instantiate(roleTab, cogTab.transform);
                var impostorTabHighlight = getTabHighlight(impostorTab, "ImpostorTab", "TheOtherRoles.Resources.TabIconImpostor.png");

                var neutralTab = UnityEngine.Object.Instantiate(roleTab, impostorTab.transform);
                var neutralTabHighlight = getTabHighlight(neutralTab, "NeutralTab", "TheOtherRoles.Resources.TabIconNeutral.png");

                var crewmateTab = UnityEngine.Object.Instantiate(roleTab, neutralTab.transform);
                var crewmateTabHighlight = getTabHighlight(crewmateTab, "CrewmateTab", "TheOtherRoles.Resources.TabIconCrewmate.png");

                var modifierTab = UnityEngine.Object.Instantiate(roleTab, crewmateTab.transform);
                var modifierTabHighlight = getTabHighlight(modifierTab, "ModifierTab", "TheOtherRoles.Resources.TabIconModifier.png");

                // Position of Tab Icons
                gameTab.transform.position += Vector3.left * 3f;
                roleTab.transform.position += Vector3.left * 3f;
                cogTab.transform.position += Vector3.left * 2f;
                impostorTab.transform.localPosition = Vector3.right * 1f;
                neutralTab.transform.localPosition = Vector3.right * 1f;
                crewmateTab.transform.localPosition = Vector3.right * 1f;
                modifierTab.transform.localPosition = Vector3.right * 1f;

                var tabs = new GameObject[] { gameTab, roleTab, cogTab, impostorTab, neutralTab, crewmateTab, modifierTab };
                var settingsHighlightMap = new Dictionary<GameObject, SpriteRenderer>
                {
                    [gameSettingMenu.RegularGameSettings] = gameSettingMenu.GameSettingsHightlight,
                    [gameSettingMenu.RolesSettings.gameObject] = gameSettingMenu.RolesSettingsHightlight,
                    [torSettings.gameObject] = cogTabHighlight,
                    [impostorSettings.gameObject] = impostorTabHighlight,
                    [neutralSettings.gameObject] = neutralTabHighlight,
                    [crewmateSettings.gameObject] = crewmateTabHighlight,
                    [modifierSettings.gameObject] = modifierTabHighlight
                };
                for (int i = 0; i < tabs.Length; i++)
                {
                    var button = tabs[i].GetComponentInChildren<PassiveButton>();
                    if (button == null) continue;
                    int copiedIndex = i;
                    button.OnClick = new UnityEngine.UI.Button.ButtonClickedEvent();
                    button.OnClick.AddListener((Action)(() =>
                    {
                        setListener(settingsHighlightMap, copiedIndex);
                    }));
                }

                destroyOptions(new List<List<OptionBehaviour>>{
                torMenu.GetComponentsInChildren<OptionBehaviour>().ToList(),
                impostorMenu.GetComponentsInChildren<OptionBehaviour>().ToList(),
                neutralMenu.GetComponentsInChildren<OptionBehaviour>().ToList(),
                crewmateMenu.GetComponentsInChildren<OptionBehaviour>().ToList(),
                modifierMenu.GetComponentsInChildren<OptionBehaviour>().ToList()
            });

                List<OptionBehaviour> torOptions = new List<OptionBehaviour>();
                List<OptionBehaviour> impostorOptions = new List<OptionBehaviour>();
                List<OptionBehaviour> neutralOptions = new List<OptionBehaviour>();
                List<OptionBehaviour> crewmateOptions = new List<OptionBehaviour>();
                List<OptionBehaviour> modifierOptions = new List<OptionBehaviour>();


                List<Transform> menus = new List<Transform>() { torMenu.transform, impostorMenu.transform, neutralMenu.transform, crewmateMenu.transform, modifierMenu.transform };
                List<List<OptionBehaviour>> optionBehaviours = new List<List<OptionBehaviour>>() { torOptions, impostorOptions, neutralOptions, crewmateOptions, modifierOptions };

                for (int i = 0; i < CustomOption.options.Count; i++)
                {
                    CustomOption option = CustomOption.options[i];
                    if ((int)option.type > 4) continue;
                    if (option.optionBehaviour == null)
                    {
                        StringOption stringOption = UnityEngine.Object.Instantiate(template, menus[(int)option.type]);
                        optionBehaviours[(int)option.type].Add(stringOption);
                        stringOption.OnValueChanged = new Action<OptionBehaviour>((o) => { });
                        stringOption.TitleText.text = option.name;
                        stringOption.Value = stringOption.oldValue = option.selection;
                        stringOption.ValueText.text = option.selections[option.selection].ToString();

                        option.optionBehaviour = stringOption;
                    }
                    option.optionBehaviour.gameObject.SetActive(true);
                }

                setOptions(
                    new List<GameOptionsMenu> { torMenu, impostorMenu, neutralMenu, crewmateMenu, modifierMenu },
                    new List<List<OptionBehaviour>> { torOptions, impostorOptions, neutralOptions, crewmateOptions, modifierOptions },
                    new List<GameObject> { torSettings, impostorSettings, neutralSettings, crewmateSettings, modifierSettings }
                );

                adaptTaskCount(__instance);
            }




            private static void setListener(Dictionary<GameObject, SpriteRenderer> settingsHighlightMap, int index)
            {
                foreach (KeyValuePair<GameObject, SpriteRenderer> entry in settingsHighlightMap)
                {
                    entry.Key.SetActive(false);
                    entry.Value.enabled = false;
                }
                settingsHighlightMap.ElementAt(index).Key.SetActive(true);
                settingsHighlightMap.ElementAt(index).Value.enabled = true;
            }

            private static void destroyOptions(List<List<OptionBehaviour>> optionBehavioursList)
            {
                foreach (List<OptionBehaviour> optionBehaviours in optionBehavioursList)
                {
                    foreach (OptionBehaviour option in optionBehaviours)
                        UnityEngine.Object.Destroy(option.gameObject);
                }
            }

            private static bool setNames(Dictionary<string, string> gameObjectNameDisplayNameMap)
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

            private static GameOptionsMenu getMenu(GameObject setting, string settingName)
            {
                var menu = setting.transform.FindChild("GameGroup").FindChild("SliderInner").GetComponent<GameOptionsMenu>();
                setting.name = settingName;

                return menu;
            }

            private static SpriteRenderer getTabHighlight(GameObject tab, string tabName, string tabSpritePath)
            {
                var tabHighlight = tab.transform.FindChild("Hat Button").FindChild("Tab Background").GetComponent<SpriteRenderer>();
                tab.transform.FindChild("Hat Button").FindChild("Icon").GetComponent<SpriteRenderer>().sprite = Utils.ResourceUtils.LoadSprite(tabSpritePath, 100f);
                tab.name = "tabName";

                return tabHighlight;
            }
            private static SpriteRenderer getTabHighlight(GameObject tab, string tabName, Sprite tabSprite)
            {
                var tabHighlight = tab.transform.FindChild("Hat Button").FindChild("Tab Background").GetComponent<SpriteRenderer>();
                tab.transform.FindChild("Hat Button").FindChild("Icon").GetComponent<SpriteRenderer>().sprite = tabSprite;
                tab.name = "tabName";

                return tabHighlight;
            }

            private static void setOptions(List<GameOptionsMenu> menus, List<List<OptionBehaviour>> options, List<GameObject> settings)
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

            private static void adaptTaskCount(GameOptionsMenu __instance)
            {
                // Adapt task count for main options
                var commonTasksOption = __instance.Children.FirstOrDefault(x => x.name == "NumCommonTasks").TryCast<NumberOption>();
                if (commonTasksOption != null) commonTasksOption.ValidRange = new FloatRange(0f, 4f);

                var shortTasksOption = __instance.Children.FirstOrDefault(x => x.name == "NumShortTasks").TryCast<NumberOption>();
                if (shortTasksOption != null) shortTasksOption.ValidRange = new FloatRange(0f, 23f);

                var longTasksOption = __instance.Children.FirstOrDefault(x => x.name == "NumLongTasks").TryCast<NumberOption>();
                if (longTasksOption != null) longTasksOption.ValidRange = new FloatRange(0f, 15f);
            }
        }
    }
    [HarmonyPatch(typeof(StringOption), nameof(StringOption.OnEnable))]
    public class StringOptionEnablePatch
    {
        public static bool Prefix(StringOption __instance)
        {
            CustomOption option = CustomOption.options.FirstOrDefault(option => option.optionBehaviour == __instance);
            if (option == null) return true;

            __instance.OnValueChanged = new Action<OptionBehaviour>((o) => { });
            __instance.TitleText.text = option.name;
            __instance.Value = __instance.oldValue = option.selection;
            __instance.ValueText.text = option.selections[option.selection].ToString();

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
            CustomOption option = CustomOption.options.FirstOrDefault(option => option.optionBehaviour == __instance);
            if (option == null) return true;

            option.updateSelection(option.selection + 1);
            return false;
        }

        [HarmonyPatch(nameof(StringOption.Decrease))]
        [HarmonyPrefix]
        public static bool DecreasePatch(StringOption __instance)
        {
            CustomOption option = CustomOption.options.FirstOrDefault(option => option.optionBehaviour == __instance);
            if (option == null) return true;

            option.updateSelection(option.selection + 1);
            return false;
        }
        [HarmonyPatch(nameof(StringOption.OnEnable))]
        public static bool OnEnablePatch(StringOption __instance)
        {
            CustomOption option = CustomOption.options.FirstOrDefault(option => option.optionBehaviour == __instance);
            if (option == null) return true;

            __instance.OnValueChanged = new Action<OptionBehaviour>((o) => { });
            __instance.TitleText.text = option.name;
            __instance.Value = __instance.oldValue = option.selection;
            __instance.ValueText.text = option.selections[option.selection].ToString();

            return false;
        }
    }
    
    public class SyncSettingPatch
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
        public static void Postfix(GameOptionsMenu __instance)
        {
            // Return Menu Update if in normal among us settings 
            var gameSettingMenu = UnityEngine.Object.FindObjectsOfType<GameSettingMenu>().FirstOrDefault();
            if (gameSettingMenu.RegularGameSettings.active || gameSettingMenu.RolesSettings.gameObject.active) return;

            __instance.GetComponentInParent<Scroller>().ContentYBounds.max = -0.5F + __instance.Children.Length * 0.55F;
            timer += Time.deltaTime;
            if (timer < 0.1f) return;
            timer = 0f;

            float offset = 2.75f;
            foreach (CustomOption option in CustomOption.options)
            {
                if (GameObject.Find("COGSettings") && option.type != CustomOptionType.General)
                    continue;
                if (GameObject.Find("ImpostorSettings") && option.type != CustomOptionType.Impostor)
                    continue;
                if (GameObject.Find("NeutralSettings") && option.type != CustomOptionType.Neutral)
                    continue;
                if (GameObject.Find("CrewmateSettings") && option.type != CustomOptionType.Crewmate)
                    continue;
                if (GameObject.Find("ModifierSettings") && option.type != CustomOptionType.Modifier)
                    continue;
                if (option?.optionBehaviour != null && option.optionBehaviour.gameObject != null)
                {
                    bool enabled = true;
                    var parent = option.parent;
                    while (parent != null && enabled)
                    {
                        enabled = parent.selection != 0;
                        parent = parent.parent;
                    }
                    option.optionBehaviour.gameObject.SetActive(enabled);
                    if (enabled)
                    {
                        offset -= option.isHeader ? 0.75f : 0.5f;
                        option.optionBehaviour.transform.localPosition = new Vector3(option.optionBehaviour.transform.localPosition.x, offset, option.optionBehaviour.transform.localPosition.z);
                    }
                }
            }
        }
    }


    [HarmonyPatch]
    class HudStringPatch
    {
        public static int PageCount = 6, Page = 0;


        [HarmonyPatch(typeof(IGameOptionsExtensions), nameof(IGameOptionsExtensions.ToHudString))]
        private static void Postfix(ref string __result)
        {
            if (GameOptionsManager.Instance.currentGameOptions.GameMode == AmongUs.GameOptions.GameModes.HideNSeek) return; // Allow Vanilla Hide N Seek
            string text = "";
            switch (Page)
            {
                case 0:
                    text = "第一页：原版设置\n\n" + __result;
                    break;
                case 1:
                    text = "第二页：模组设置\n" + HudStringPatch.GetOptByType(CustomOptionType.General);
                    break;
                case 2:
                    text = "Page 3: Impostor Role Settings \n" + HudStringPatch.GetOptByType(CustomOptionType.Impostor);
                    break;
                case 3:
                    text = "Page 4: Neutral Role Settings \n" + HudStringPatch.GetOptByType(CustomOptionType.Neutral);
                    break;
                case 4:
                    text = "Page 5: Crewmate Role Settings \n" + HudStringPatch.GetOptByType(CustomOptionType.Crewmate);
                    break;
                case 5:
                    text = "Page 6: Modifier Settings \n" + HudStringPatch.GetOptByType(CustomOptionType.Modifier);
                    break;
            }
            text += $"Press TAB for next page...{Page + 1}/{PageCount}";
            __result = text;
        }
        public static string GetOptByType(CustomOptionType type)
        {
            string txt = "";
            List<CustomOption> opt = new();
            foreach (var option in options)
            {
                if (option.type == type)
                {
                    opt.Add(option);
                }
            }
            foreach (var option in opt)
            {
                txt += option.name + ": " + option.selections[option.selection].ToString() + "\n";
            }
            return txt;
        }
    }

    [HarmonyPatch(typeof(KeyboardJoystick), nameof(KeyboardJoystick.Update))]
    public static class GameOptionsNextPagePatch
    {
        public static void Postfix(KeyboardJoystick __instance)
        {
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                HudStringPatch.Page = (HudStringPatch.Page + 1) % 7;
            }
            if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1))
            {
                HudStringPatch.Page = 0;
            }
            if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2))
            {
                HudStringPatch.Page = 1;
            }
            if (Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Keypad3))
            {
                HudStringPatch.Page = 2;
            }
            if (Input.GetKeyDown(KeyCode.Alpha4) || Input.GetKeyDown(KeyCode.Keypad4))
            {
                HudStringPatch.Page = 3;
            }
            if (Input.GetKeyDown(KeyCode.Alpha5) || Input.GetKeyDown(KeyCode.Keypad5))
            {
                HudStringPatch.Page = 4;
            }
            if (Input.GetKeyDown(KeyCode.Alpha6) || Input.GetKeyDown(KeyCode.Keypad6))
            {
                HudStringPatch.Page = 5;
            }
            if (Input.GetKeyDown(KeyCode.Alpha7) || Input.GetKeyDown(KeyCode.Keypad7))
            {
                HudStringPatch.Page = 6;
            }
        }
    }
}
