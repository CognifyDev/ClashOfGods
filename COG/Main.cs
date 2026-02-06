global using HarmonyLib;
global using Hazel;
global using GitInfo = ThisAssembly.Git;
global using Object = UnityEngine.Object;
using System;
using System.IO;
using System.Linq;
using System.Text;
using BepInEx;
using BepInEx.Unity.IL2CPP;
using COG.Command;
using COG.Config;
using COG.Config.Impl;
using COG.Constant;
using COG.Game.CustomWinner;
using COG.Listener;
using COG.Listener.Impl;
using COG.Patch;
using COG.Role;
using COG.UI.ClientOption;
using COG.UI.ClientOption.Impl;
using COG.Utils;
using COG.Utils.Version;
using Reactor;
using Reactor.Networking;
using Reactor.Networking.Attributes;
using UnityEngine.SceneManagement;
using System.Reflection;
using COG.UI.Load;

#if WINDOWS
using System.Windows.Forms;
#endif

namespace COG;

[BepInAutoPlugin(PluginGuid, PluginName)]
[BepInProcess("Among Us.exe")]
[BepInDependency(ReactorPlugin.Id)]
[ReactorModFlags(ModFlags.RequireOnAllClients)]
public partial class Main : BasePlugin
{
    public const string PluginName = "Clash Of Gods";
    public const string PluginGuid = "top.cog.clashofgods";
    public const string DisplayName = "ClashOfGods";

    public static StackTraceLogger Logger { get; private set; } = null!;
    public static VersionInfo VersionInfo { get; private set; } = null!;
    public static string PluginVersion { get; private set; } = null!;
    public static DateTime CommitTime => DateTime.Parse(GitInfo.CommitDate);
    public static Assembly Assembly { get; } = typeof(Main).Assembly;

    private Harmony Harmony { get; } = new(PluginGuid);

    public static Main Instance { get; private set; } = null!;

    /// <summary>
    ///     插件的启动方法
    /// </summary>
    public override void Load()
    {
        Instance = this;

        PluginVersion = ProjectUtils.GetProjectVersion() ?? "Unknown";
        VersionInfo = PluginVersion.Equals("Unknown")
            ? VersionInfo.Empty
            : VersionInfo.Parse(PluginVersion);

        System.Console.OutputEncoding = Encoding.UTF8;

        Logger = new StackTraceLogger($"   {DisplayName}");
        Logger.LogInfo("Loading...");
        Logger.LogInfo("Mod Version => " + PluginVersion);
        Logger.LogInfo($"GitInfo: {GitInfo.Branch} ({GitInfo.Commit} at {GitInfo.CommitDate})");

        var longVersionInfo = StringUtils.EncodeToBase64($"{VersionInfo}{GitInfo.Branch}{GitInfo.Sha}");
        var storagedInfo = "";

        try
        {
            storagedInfo = File.ReadAllText(@$".\{ConfigBase.DataDirectoryName}\VersionInfo.dat");
        }
        catch
        {
            // ignored
        }

        ConfigBase.AutoReplace = true;

        if (!storagedInfo.IsNullOrEmptyOrWhiteSpace())
        {
            if (storagedInfo != longVersionInfo)
                Logger.LogWarning(
                    "Current mod version doesnt equal to version of last mod running on this machine. Schedule to replace config files...");
            else
                ConfigBase.AutoReplace = false;
        }


        if (!Directory.Exists(ConfigBase.DataDirectoryName))
        {
            Directory.CreateDirectory(ConfigBase.DataDirectoryName);
        }
        
        File.WriteAllText(@$".\{ConfigBase.DataDirectoryName}\VersionInfo.dat", longVersionInfo);

        /*
        ModUpdater.FetchUpdate();
        Logger.LogInfo(
            $"Latest Version => {(Equals(ModUpdater.LatestVersion, VersionInfo.Empty) ? "Unknown" : ModUpdater.LatestVersion!.ToString())}");
        */

        // Register mod options
        ClientOptionManager.GetManager().RegisterClientOptions(new IClientOption[]
        {
            new ToggleClientOption("main.unload-mod.name",
                false,
                _ =>
                {
                    DestroyableSingleton<OptionsMenuBehaviour>.Instance.Close();
                    if (GameStates.InRealGame || GameStates.InLobby)
                    {
                        GameUtils.Popup?.Show(LanguageConfig.Instance.UnloadModInGameErrorMsg);
                        return false;
                    }

                    Unload();
                    GameUtils.Popup?.Show(LanguageConfig.Instance.UnloadModSuccessfulMessage);
                    return false;
                }),
#if WINDOWS
            new ToggleClientOption("main.load-custom-lang",
                false,
                _ =>
                {
                    if (GameStates.InRealGame || GameStates.InLobby)
                    {
                        GameUtils.Popup?.Show(
                            "You're trying to load the custom language in the game.\nIt may occur some unexpected glitches.\nPlease leave to reload.");
                        return false;
                    }

                    var openFileDialog = new OpenFileDialog();
                    openFileDialog.DefaultExt = "yml";
                    openFileDialog.InitialDirectory =
                        @$"{Directory.GetCurrentDirectory()}\{ConfigBase.DataDirectoryName}";
                    if (openFileDialog.ShowDialog() != DialogResult.OK)
                        return false;

                    LanguageConfig.LoadLanguageConfig(openFileDialog.FileName);
                    DestroyableSingleton<OptionsMenuBehaviour>.Instance.Close();
                    SceneManager.LoadScene(Constants.MAIN_MENU_SCENE);
                    return false;
                }),
#endif
            new ToggleClientOption("hotkey.name",
                false,
                _ =>
                {
                    ClientOptionManager.GetManager().GetOptions()
                        .ForEach(o => o.Component!.gameObject.SetActive(false));
                    ClientOptionListener.HotkeyButtons.ForEach(o => o.SetActive(true));
                    return false;
                }),
            new SliderClientOption("main.max-chat-bubble-adjustment",
                20,
                20,
                100,
                newValue =>
                {
                    var value = ChatBubblePoolPatch.MaxBubbleCount = (int)newValue;
                    ChatController? controller = null;
                    if (controller == Object.FindObjectOfType<ChatController>())
                    {
                        if (controller == null)
                        {
                            return value;
                        }
                        var pool = controller.chatBubblePool;
                        var current = pool.activeChildren.Count + pool.inactiveChildren.Count;
                        var toClone = value - current;

                        for (var i = 0; i < toClone; i++)
                            pool.CreateOneInactive(pool.Prefab);
                    }

                    return value;
                },
                (value, origin) => origin + $": {(int)value}")
        });

        Harmony.PatchAll();

        GlobalCustomOptionConstant.Init();
    }

    public override bool Unload()
    {
        if (SettingsConfig.Instance.EnablePluginSystem)
        {
            SplashManagerPatch.PluginManager.DisableAllPlugins();
        }

        // 卸载插件时候，卸载一切东西
        CommandManager.GetManager().GetCommands().Clear();
        ClientOptionManager.GetManager().GetOptions().Clear();
        CustomRoleManager.GetManager().GetRoles().Clear();
        ListenerManager.GetManager().UnregisterHandlers();
        EndGameResult.CachedWinners?.Clear();
        CustomWinnerManager.GetManager().Reset();
        Harmony.UnpatchAll();
        MainMenuPatch.Buttons.Where(b => b).ToList().ForEach(b => b.gameObject.TryDestroy());
        MainMenuPatch.CustomBanner!.TryDestroy();
        return false;
    }
}