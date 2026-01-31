using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using COG.Asset.Dependens;
using COG.Command;
using COG.Command.Impl;
using COG.Config.Impl;
using COG.Game.CustomWinner;
using COG.Game.CustomWinner.Winnable;
using COG.Listener;
using COG.Listener.Impl;
using COG.Plugin;
using COG.Plugin.JavaScript;
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
    private static readonly Sprite LogoSprite = LoadSprite("COG-BG.png", 150f)!;
    private static readonly Sprite BgSprite = LoadSprite("COG-LOADBG.png")!;
    private static readonly Sprite CognifyDevLogoSprite = LoadSprite("TeamLogo.png", 120f)!;

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

        var logo = CreateObject<SpriteRenderer>("CognifyDevLogo", null!, new Vector3(0, 0.5f, -5f));
        logo.sprite = CognifyDevLogoSprite;
        logo.color = Color.clear;
        logo.transform.localScale = Vector3.one;

        var logoText = Object.Instantiate(__instance.errorPopup.InfoText, null);
        logoText.transform.localPosition = new Vector3(0f, -0.6f, -10f);
        logoText.fontStyle = FontStyles.Bold;
        logoText.text = "CognifyDev";
        logoText.color = new Color(1, 1, 1, 0);
        logoText.fontSize = 0.6f;
        logoText.alignment = TextAlignmentOptions.Center;

        float duration = 1.5f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);

            logo.color = Color.Lerp(Color.clear, Color.white, t);
            logoText.color = Color.Lerp(new Color(1, 1, 1, 0), new Color(1, 1, 1, 0.8f), t);

            yield return null;
        }

        logo.color = Color.white;
        logoText.color = new Color(1, 1, 1, 0.8f);

        yield return new WaitForSeconds(2.5f);

        elapsed = 0f;
        Color startLogoColor = logo.color;
        Color startTextColor = logoText.color;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);

            logo.color = Color.Lerp(startLogoColor, Color.clear, t);
            logoText.color = Color.Lerp(startTextColor, new Color(1, 1, 1, 0), t);

            yield return null;
        }

        logo.color = Color.clear;
        logoText.color = new Color(1, 1, 1, 0);

        if (logo != null)
            Object.Destroy(logo.gameObject);
        if (logoText != null)
            Object.Destroy(logoText.gameObject);

        CognifyDevLogoActive = false;
    }

    private static IEnumerator CoLoadMod(SplashManager __instance)
    {
        yield return __instance.StartCoroutine(CoLoadMod_InitialAnimation(__instance).WrapToIl2Cpp());

        yield return __instance.StartCoroutine(CoLoadMod_ShowLoadingText(__instance).WrapToIl2Cpp());

        yield return __instance.StartCoroutine(CoLoadMod_ExecuteSteps(__instance).WrapToIl2Cpp());

        yield return __instance.StartCoroutine(CoLoadMod_CompletionAnimation(__instance).WrapToIl2Cpp());

        yield return __instance.StartCoroutine(CoLoadMod_Cleanup(__instance).WrapToIl2Cpp());
    }

    private static IEnumerator CoLoadMod_InitialAnimation(SplashManager __instance)
    {
        var logo = CreateObject<SpriteRenderer>("COG-BG", null!, new Vector3(0, 0.5f, -4.9f));
        logo.sprite = LogoSprite;
        logo.color = Color.clear;
        logo.sortingOrder = 1;

        var bg = CreateObject<SpriteRenderer>("COG-LOADBG", null!, new Vector3(0, 0.5f, -5f));
        bg.sprite = BgSprite;
        bg.color = Color.clear;
        bg.sortingOrder = 0;

        var duration = 1.5f;
        var elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            var t = Mathf.SmoothStep(0f, 1f, elapsed / duration);
            logo.color = Color.Lerp(Color.clear, Color.white, t);
            logo.transform.localScale = Vector3.Lerp(Vector3.one * 0.9f, Vector3.one, t);
            yield return null;
        }

        yield return new WaitForSeconds(0.2f);
        elapsed = 0f;

        while (elapsed < duration * 0.7f)
        {
            elapsed += Time.deltaTime;
            var t = Mathf.SmoothStep(0f, 1f, elapsed / (duration * 0.7f));
            bg.color = Color.Lerp(Color.clear, Color.white, t);
            yield return null;
        }
    }

    private static IEnumerator CoLoadMod_ShowLoadingText(SplashManager __instance)
    {
        LoadText = Object.Instantiate(__instance.errorPopup.InfoText, null);
        LoadText.transform.localPosition = new Vector3(0f, -0.28f, -10f);
        LoadText.fontStyle = FontStyles.Bold;
        LoadText.text = "Loading";
        LoadText.color = new Color(1, 1, 1, 0);

        float elapsed = 0f;
        while (elapsed < 0.8f)
        {
            elapsed += Time.deltaTime;
            LoadText.color = Color.Lerp(new Color(1, 1, 1, 0), new Color(1, 1, 1, 0.3f), elapsed / 0.8f);
            yield return null;
        }
    }

    private static IEnumerator CoLoadMod_ExecuteSteps(SplashManager __instance)
    {
        //下载依赖dll
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

    private static IEnumerator CoLoadMod_StepDatas()
    {
        yield return ChangeLoadingText(LanguageConfig.Instance.LoadingDatas, 0.3f);

        try
        {
            //快捷键
            _ = ButtonHotkeyConfig.Instance;

            //指令
            CommandManager.GetManager().RegisterCommands([
            new RpcCommand(),
            new OptionCommand(),
            new DebugCommand()
            ]);


            //预设配置
            _ = SettingsConfig.Instance;


            //职业
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

            //胜利
            CustomWinnerManager.GetManager().RegisterCustomWinnables([
            new CrewmatesCustomWinner(),
            new ImpostorsCustomWinner(),
            new LastPlayerCustomWinner()
            ]);

            //插件
            if (SettingsConfig.Instance.EnablePluginSystem)
            {
                if (!Directory.Exists(JsPluginManager.PluginDirectoryPath))
                    Directory.CreateDirectory(JsPluginManager.PluginDirectoryPath);
                var files = Directory.GetFiles(JsPluginManager.PluginDirectoryPath)
                    .Where(name => name.ToLower().EndsWith(".cog"));
                var enumerable = files.ToArray();
                Main.Logger.LogInfo($"{enumerable.Length} plugin(s) to load.");
                foreach (var file in enumerable)
                    IPluginManager.GetDefaultManager().LoadPlugin(file);
            }
        }
        catch (System.Exception e)
        {
            Main.Logger.LogError($"Error while loading datas: " + e);
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

    private static IEnumerator CoLoadMod_Cleanup(SplashManager __instance)
    {
        float elapsed = 0f;
        var logo = GameObject.Find("COG-BG")?.GetComponent<SpriteRenderer>();
        var bg = GameObject.Find("COG-LOADBG")?.GetComponent<SpriteRenderer>();

        var originalLogoColor = logo?.color ?? Color.clear;
        var originalBgColor = bg?.color ?? Color.clear;
        var originalTextColor = LoadText.color;

        while (elapsed < 1f)
        {
            elapsed += Time.deltaTime;
            var t = elapsed / 1f;

            if (logo != null) logo.color = Color.Lerp(originalLogoColor, Color.clear, t);
            if (bg != null) bg.color = Color.Lerp(originalBgColor, Color.clear, t);
            LoadText.color = Color.Lerp(originalTextColor,
                new Color(originalTextColor.r, originalTextColor.g, originalTextColor.b, 0), t);

            yield return null;
        }

        if (LoadText != null) Object.Destroy(LoadText.gameObject);
        if (logo != null) Object.Destroy(logo.gameObject);
        if (bg != null) Object.Destroy(bg.gameObject);

        __instance.sceneChanger.AllowFinishLoadingScene();
        __instance.startedSceneLoad = true;
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

        if (__instance.doneLoadingRefdata && !__instance.startedSceneLoad && LoadedCognifyDevLogo && !CognifyDevLogoActive && !_loadedCOG)
        {
            _loadedCOG = true;
            __instance.StartCoroutine(CoLoadMod(__instance).WrapToIl2Cpp());
        }
        return false;
    }
}