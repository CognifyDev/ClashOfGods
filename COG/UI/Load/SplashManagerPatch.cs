using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using COG.Asset.Dependence;
using COG.Command;
using COG.Command.Impl;
using COG.Config;
using COG.Config.Impl;
using COG.Constant;
using COG.Cosmetics;
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
using COG.Utils;
using TMPro;
using UnityEngine;
using static COG.Utils.GameObjectUtils;
using static COG.Utils.ResourceUtils;

namespace COG.UI.Load;

[HarmonyPatch(typeof(SplashManager), nameof(SplashManager.Update))]
public static class SplashManagerPatch
{
    
    // 懒加载 —— 在资源下载完成后才加载，避免在程序集资源不可用时提前失败
    private static Sprite? _cognifyDevLogoSprite;

    public static TextMeshPro LoadText = null!;
    public static bool LoadedCognifyDevLogo;
    public static bool CognifyDevLogoActive;

    public static string LoadingText
    {
        set => LoadText.text = value;
    }

    public static bool Prefix(SplashManager __instance)
    {
        if (__instance.doneLoadingRefdata &&
            !__instance.startedSceneLoad &&
            Time.time - __instance.startTime > 4.2f &&
            !LoadedCognifyDevLogo)
        {
            LoadedCognifyDevLogo = true;
            __instance.StartCoroutine(CoLoadCognifyDevLogo(__instance).WrapToIl2Cpp());
        }
        return false;
    }

    public static IEnumerator CoLoadCognifyDevLogo(SplashManager __instance)
    {
        CognifyDevLogoActive = true;
        
        _cognifyDevLogoSprite ??= LoadSpriteFromResources(ResourceConstant.TeamLogoSprite, 90f);
        
        var logo = CreateObject<SpriteRenderer>("CognifyDevLogo", null!, new Vector3(0, 0.5f, -4.9f));
        logo.sprite = _cognifyDevLogoSprite;
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
        yield return __instance.StartCoroutine(CoLoadMod_StepDownloadDependencies().WrapToIl2Cpp());
        
        yield return __instance.StartCoroutine(CoLoadMod_StepDownloadImages().WrapToIl2Cpp());

        yield return __instance.StartCoroutine(CoLoadMod_StepListeners().WrapToIl2Cpp());

        yield return __instance.StartCoroutine(CoLoadMod_StepDatas().WrapToIl2Cpp());
        
        yield return __instance.StartCoroutine(CoLoadMod_StepCosmetics().WrapToIl2Cpp());

        yield return __instance.StartCoroutine(CoLoadMod_StepCompleted().WrapToIl2Cpp());
    }
    
    private static IEnumerator CoLoadMod_StepDownloadDependencies()
    {
        yield return DependenceDownloader.DownloadYaml();
        yield return ChangeLoadingText(LanguageConfig.Instance.LoadingDependencies, 0.3f);
		yield return DependenceDownloader.DownloadCommonDependence();
		yield return DependenceDownloader.DownloadPluginSystemDependence();
		yield return new WaitForSeconds(0.3f);
    }

    private static IEnumerator CoLoadMod_StepDownloadImages()
    {
        yield return ChangeLoadingText(LanguageConfig.Instance.LoadingVerify, 0.5f);
        
        byte[] fileListBytes = null!;
        yield return Task.Run(async () =>
        {
            fileListBytes = await ResourceUtils.DownloadFileAsync(ResourceUtils.FileListURL);
        }).WaitAsCoroutine();

        if (fileListBytes == null || fileListBytes.Length == 0)
        {
            Main.Logger.LogError("Failed to download resource list. Skipping resource verification.");
            yield break;
        }

        string fileList = Encoding.UTF8.GetString(fileListBytes);
        string[] fileEntries = fileList.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
        int totalFiles = fileEntries.Length;
        int processedFiles = 0;

        foreach (var entry in fileEntries)
        {
            if (!entry.Contains(',')) continue;

            var split = entry.Split(',');
            if (split.Length < 2) continue;

            string relativeFilePath = split[0].Trim();
            string expectedSHA1 = split[1].Trim();

            string localCachePath = Path.Combine(ResourceUtils.CacheDataDir, relativeFilePath);
            localCachePath = localCachePath.Replace("/", "\\");

            bool needsDownload = false;
            string? currentSHA1 = null;

            if (File.Exists(localCachePath))
            {
                try
                {
                    currentSHA1 = ResourceUtils.GetFileSHA1(localCachePath);
                }
                catch (System.Exception e)
                {
                    Main.Logger.LogError($"Failed to compute SHA1 for {localCachePath}: {e}");
                    needsDownload = true;
                }
                if (currentSHA1 != null && !currentSHA1.Equals(expectedSHA1, StringComparison.OrdinalIgnoreCase))
                {
                    Main.Logger.LogInfo($"File outdated: {relativeFilePath}. Expected:{expectedSHA1}, Got:{currentSHA1}");
                    needsDownload = true;
                }
            }
            else
            {
                needsDownload = true;
            }

            if (needsDownload)
            {
                processedFiles++;
                LoadingText = $"{LanguageConfig.Instance.LoadingResources} ({processedFiles}/{totalFiles})";

                string remoteFileUrl = $"{ResourceUtils.TargetURL}{relativeFilePath}";
                Main.Logger.LogInfo($"Downloading: {relativeFilePath}");

                byte[] fileData = null!;

                yield return Task.Run(async () =>
                {
                    fileData = await ResourceUtils.DownloadFileAsync(remoteFileUrl);
                }).WaitAsCoroutine();

                if (fileData != null && fileData.Length > 0)
                {
                    // 保存文件并同步更新内存缓存
                    ResourceUtils.SaveToCache(relativeFilePath, fileData);
                    Main.Logger.LogInfo($"Cached: {relativeFilePath} -> {localCachePath}");

                    try
                    {
                        string verifySHA1 = ResourceUtils.GetFileSHA1(localCachePath);
                        if (!verifySHA1.Equals(expectedSHA1, StringComparison.OrdinalIgnoreCase))
                        {
                            Main.Logger.LogError($"SHA1 mismatch after writing {localCachePath}. Expected:{expectedSHA1}, Got:{verifySHA1}");
                        }
                    }
                    catch (System.Exception e)
                    {
                        Main.Logger.LogError($"Failed to verify downloaded file {localCachePath}: {e}");
                    }
                }
            }
            else
            {
                processedFiles++;
            }
        }
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
            Main.Logger.LogError($"Error while loading listeners: {e}");
        }
        yield return new WaitForSeconds(0.3f);
    }

    public static IPluginManager PluginManager { get; private set; } = null!;

    private static IEnumerator CoLoadMod_StepDatas()
    {
        yield return ChangeLoadingText(LanguageConfig.Instance.LoadingDatas, 0.3f);
        try
        {
            //热键
            _ = ButtonHotkeyConfig.Instance;
            //指令
            CommandManager.GetManager().RegisterCommands([
                new RpcCommand(), new OptionCommand(), new DebugCommand()
            ]);
            //预设配置
            _ = SettingsConfig.Instance;
            //职业
            CustomRoleManager.GetManager().RegisterRoles([
                new Unknown(),
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
                new Impostor(),
                new Cleaner(),
                new Stabber(),
                new Reaper(),
                new Troublemaker(),
                new Nightmare(),
                new Spy(),
                new Jester(),
                new Reporter(),
                new DeathBringer(),
                new Guesser(),
                new SpeedBooster()
            ]);
            //结算
            CustomWinnerManager.GetManager().RegisterCustomWinnables([
                new CrewmatesCustomWinner(),
                new ImpostorsCustomWinner(),
                new LastPlayerCustomWinner()
            ]);
            //插件
            if (SettingsConfig.Instance.EnablePluginSystem)
            {
                var pluginDir = Path.Combine(ConfigBase.DataDirectoryName, "plugins");
                if (!Directory.Exists(pluginDir)) Directory.CreateDirectory(pluginDir);
                Main.Logger.LogInfo("Initializing Plugin System...");
                PluginManager = new PythonPluginManager(pluginDir);
                try { PluginManager.LoadAllPlugins(); }
                catch (System.Exception ex) { Main.Logger.LogError($"Critical error loading plugins: {ex}"); }
            }
        }
        catch (System.Exception e)
        {
            Main.Logger.LogError($"Error while loading data: {e}");
        }
        yield return new WaitForSeconds(0.3f);
    }

    private static IEnumerator CoLoadMod_StepCosmetics()
    {
        yield return ChangeLoadingText(LanguageConfig.Instance.LoadingCosmetics, 0.3f);

        try
        {
            CosmeticPaths.EnsureDirectoriesExist();
            CosmeticsManager.Instance.LoadCosmetics();
        }
        catch (System.Exception e)
        {
            Main.Logger.LogError($"Error loading cosmetics: {e}");
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
}
