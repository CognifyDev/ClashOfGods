using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using COG.Asset.Dependens;
using COG.Command;
using COG.Command.Impl;
using COG.Config;
using COG.Config.Impl;
using COG.Constant;
using COG.Game.CustomWinner;
using COG.Game.CustomWinner.Winnable;
using COG.Listener;
using COG.Listener.Impl;
using COG.Plugin;
using COG.Plugin.Python;
using COG.Role;
using COG.Role.Impl;
using COG.Role.Impl.Crewmate;
using COG.Role.Impl.Impostor;
using COG.Role.Impl.Neutral;
using COG.Role.Impl.SubRole;
using TMPro;
using UnityEngine;
using static COG.Utils.GameObjectUtils;
using static COG.Utils.ResourceUtils;

namespace COG.UI.Load;

[HarmonyPatch(typeof(SplashManager), nameof(SplashManager.Update))]
public static class SplashManagerPatch
{
    private static readonly Sprite CognifyDevLogoSprite = LoadSprite(ResourceConstant.TeamLogoSprite, 90f)!;

    public static TextMeshPro LoadText = null!;
    public static bool LoadedCognifyDevLogo;
    private static bool _loadedCOG;
    public static bool CognifyDevLogoActive;
    public static string LoadingText
    {
        set => LoadText.text = value;
    }

    public static IEnumerator CoLoadCognifyDevLogo(SplashManager __instance)
    {
        CognifyDevLogoActive = true;

        var logo = CreateObject<SpriteRenderer>("CognifyDevLogo", null!, new Vector3(0, 0.5f, -4.9f));
        logo.sprite = CognifyDevLogoSprite;
        logo.color = Color.clear;
        logo.transform.localScale = Vector3.one;

        LoadText = Object.Instantiate(__instance.errorPopup.InfoText, null);
        LoadText.transform.localPosition = new Vector3(0f, -0.28f, -10f);
        LoadText.fontStyle = FontStyles.Bold;
        LoadText.text = "Loading";
        LoadText.color = new Color(1, 1, 1, 0);
        LoadText.fontSize = 0.6f;
        LoadText.alignment = TextAlignmentOptions.Center;

        var duration = 1.5f;
        var elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            var t = Mathf.SmoothStep(0f, 1f, elapsed / duration);

            logo.color = Color.Lerp(Color.clear, Color.white, t);
            LoadText.color = Color.Lerp(new Color(1, 1, 1, 0), new Color(1, 1, 1, 0.3f), t);
            logo.transform.localScale = Vector3.Lerp(Vector3.one * 0.9f, Vector3.one, t);

            yield return null;
        }

        logo.color = Color.white;
        LoadText.color = new Color(1, 1, 1, 0.3f);

        yield return __instance.StartCoroutine(CoLoadMod_ExecuteSteps(__instance).WrapToIl2Cpp());

        yield return __instance.StartCoroutine(CoLoadMod_CompletionAnimation(__instance).WrapToIl2Cpp());

        yield return new WaitForSeconds(1f);

        elapsed = 0f;
        var startLogoColor = logo.color;
        var startTextColor = LoadText.color;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            var t = Mathf.SmoothStep(0f, 1f, elapsed / duration);

            logo.color = Color.Lerp(startLogoColor, Color.clear, t);
            LoadText.color = Color.Lerp(startTextColor, new Color(1, 1, 1, 0), t);

            yield return null;
        }

        if (logo != null) Object.Destroy(logo.gameObject);
        if (LoadText != null) Object.Destroy(LoadText.gameObject);

        __instance.sceneChanger.AllowFinishLoadingScene();
        __instance.startedSceneLoad = true;

        CognifyDevLogoActive = false;
    }

    private static IEnumerator CoLoadMod_ExecuteSteps(SplashManager __instance)
    {
        // 下载依赖dll
        yield return __instance.StartCoroutine(CoLoadMod_StepDownloadDependencies().WrapToIl2Cpp());

        // 加载监听器
        yield return __instance.StartCoroutine(CoLoadMod_StepListeners().WrapToIl2Cpp());

        // 加载DATA
        yield return __instance.StartCoroutine(CoLoadMod_StepDatas().WrapToIl2Cpp());

        // 加载完成
        yield return __instance.StartCoroutine(CoLoadMod_StepCompleted().WrapToIl2Cpp());
    }

    private static IEnumerator CoLoadMod_StepDownloadDependencies()
    {
        yield return DependensDownloader.DownloadYaml();

        yield return ChangeLoadingText(LanguageConfig.Instance.LoadingDependencies, 0.3f);

        yield return DependensDownloader.DownloadCommonDependens();

        yield return new WaitForSeconds(0.3f);
    }

    private static IEnumerator CoLoadMod_StepListeners()
    {
        yield return ChangeLoadingText(LanguageConfig.Instance.LoadingListeners, 0.3f);

        try
        {
            ListenerManager.GetManager().RegisterListeners([
            new CommandListener(),
            new PlayerListener(),
            new CustomButtonListener(),
            new GameListener(),
            new ClientOptionListener(),
            new RpcListener(),
            new TaskAdderListener(),
            new VersionShowerListener(),
            new VanillaBugFixListener(),
            new CustomGameEndLogicListener(),
            new LobbyListener(),
            new RoleAssignmentListener(),
            new IntroListener()
            ]);
        }
        catch (System.Exception e)
        {
            Main.Logger.LogError($"Error while loading listeners: " + e);
        }

        yield return new WaitForSeconds(0.3f);
    }

    public static IPluginManager PluginManager { get; private set; } = null!;

    private static IEnumerator CoLoadMod_StepDatas()
    {
        yield return ChangeLoadingText(LanguageConfig.Instance.LoadingDatas, 0.3f);

        try
        {
            // 快捷键
            _ = ButtonHotkeyConfig.Instance;

            // 指令
            CommandManager.GetManager().RegisterCommands([
            new RpcCommand(),
            new OptionCommand(),
            new DebugCommand()
            ]);

            // 预设配置
            _ = SettingsConfig.Instance;

            // 职业
            CustomRoleManager.GetManager().RegisterRoles([
                     // Unknown
                     new Unknown(),
                    // Crewmate
                    new Crewmate(),
                    new Bait(),
                    new Sheriff(),
                    new Vigilante(),
                    new SoulHunter(),
                    new Technician(),
                    new Inspector(),
                    new Doorman(),
                    new Chief(),
                    new Enchanter(),
                    new Witch(),
                    new Seer(),
                    // Impostor
                    new Impostor(),
                    new Cleaner(),
                    new Stabber(),
                    new Reaper(),
                    new Troublemaker(),
                    new Nightmare(),
                    new Spy(),
                    // Neutral
                    new Jester(),
                    new Reporter(),
                    new DeathBringer(),
                    // Sub-roles
                    new Guesser(),
                    new SpeedBooster()
                 ]);

            // 胜利
            CustomWinnerManager.GetManager().RegisterCustomWinnables([
            new CrewmatesCustomWinner(),
            new ImpostorsCustomWinner(),
            new LastPlayerCustomWinner()
            ]);

            // 插件
            if (SettingsConfig.Instance.EnablePluginSystem)
            {
                var pluginDir = Path.Combine(ConfigBase.DataDirectoryName, "plugins");
                if (!Directory.Exists(pluginDir))
                {
                    Directory.CreateDirectory(pluginDir);
                }

                Main.Logger.LogInfo("Initializing Plugin System...");

                PluginManager = new PythonPluginManager(pluginDir);
                try
                {
                    PluginManager.LoadAllPlugins();
                }
                catch (System.Exception ex)
                {
                    Main.Logger.LogError($"Critical error loading plugins: {ex}");
                }
            }
        }
        catch (System.Exception e)
        {
            Main.Logger.LogError("Error while loading data: " + e);
        }

        yield return new WaitForSeconds(0.3f);
    }

    private static IEnumerator CoLoadMod_StepCompleted()
    {
        yield return new WaitForSeconds(0.5f);
        yield return ChangeLoadingText(LanguageConfig.Instance.LoadingCompeleted, 0.3f);
        yield return new WaitForSeconds(0.5f);
    }

    private static IEnumerator CoLoadMod_CompletionAnimation(SplashManager __instance)
    {
        float elapsed = 0f;
        var startColor = LoadText.color;
        var targetColor = Color.green.AlphaMultiplied(0.6f);

        while (elapsed < 0.5f)
        {
            elapsed += Time.deltaTime;
            LoadText.color = Color.Lerp(startColor, targetColor, elapsed / 0.5f);
            yield return null;
        }

        for (var i = 0; i < 2; i++)
        {
            elapsed = 0f;
            while (elapsed < 0.3f)
            {
                elapsed += Time.deltaTime;
                var alpha = 0.6f + Mathf.Sin(elapsed * 20f) * 0.2f;
                LoadText.color = Color.green.AlphaMultiplied(alpha);
                yield return null;
            }
        }
    }

    public static IEnumerator ChangeLoadingText(string newText, float duration)
    {
        if (LoadText.text == LanguageConfig.Instance.Loading)
        {
            LoadText.text = newText;
            yield break;
        }

        var elapsed = 0f;

        while (elapsed < duration / 2)
        {
            elapsed += Time.deltaTime;
            var t = elapsed / (duration / 2);
            LoadText.color = Color.Lerp(new Color(1, 1, 1, 0.3f), new Color(1, 1, 1, 0), t);
            yield return null;
        }

        LoadText.text = newText;

        elapsed = 0f;
        while (elapsed < duration / 2)
        {
            elapsed += Time.deltaTime;
            var t = elapsed / (duration / 2);
            LoadText.color = Color.Lerp(new Color(1, 1, 1, 0), new Color(1, 1, 1, 0.3f), t);
            yield return null;
        }
    }

    public static bool Prefix(SplashManager __instance)
    {
        if (__instance.doneLoadingRefdata && !__instance.startedSceneLoad && Time.time - __instance.startTime > 4.2f && !LoadedCognifyDevLogo)
        {
            LoadedCognifyDevLogo = true;
            __instance.StartCoroutine(CoLoadCognifyDevLogo(__instance).WrapToIl2Cpp());
            return false;
        }

        return false;
    }
}