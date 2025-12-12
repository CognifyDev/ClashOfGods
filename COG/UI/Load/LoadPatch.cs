using System.Collections;
using System.IO;
using System.Linq;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using COG.Command;
using COG.Command.Impl;
using COG.Config.Impl;
using COG.Constant;
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
public static class LoadPatch
{
    private static readonly Sprite LogoSprite = LoadSprite(ResourceConstant.BgLogoSprite, 300f);
    private static readonly Sprite BgSprite = LoadSprite(ResourceConstant.LoadBgSprite);
    public static TextMeshPro LoadText = null!;

    public static bool LoadedTeamLogo;

    private static bool _loadedFracturedTruth;

    // 添加一个标志来跟踪TeamLogo是否还在显示
    public static bool TeamLogoActive;

    public static string LoadingText
    {
        set => LoadText.text = value;
    }

    public static IEnumerator CoLoadTeamLogo(SplashManager __instance)
    {
        TeamLogoActive = true;

        // 创建logo对象，初始状态为透明
        var logo = CreateObject<SpriteRenderer>("TeamLogo", null!, new Vector3(0, 0.5f, -5f));
        logo.sprite = LoadSprite(ResourceConstant.TeamLogoSprite, 70f);
        logo.color = Color.clear;
        logo.transform.localScale = Vector3.one * 0.8f;

        // 创建团队名称文字
        var teamText = Object.Instantiate(__instance.errorPopup.InfoText, null);
        teamText.transform.localPosition = new Vector3(0f, -1f, -10f); // 放在logo下方
        teamText.fontStyle = FontStyles.Bold;
        teamText.text = "CognifyDev";
        teamText.color = new Color(1, 1, 1, 0); // 初始透明
        teamText.fontSize = 0.8f; // 调整字体大小

        // 第一阶段：平滑淡入 + 缩放
        var duration = 1.2f;
        var elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            var t = Mathf.SmoothStep(0f, 1f, elapsed / duration);

            logo.color = Color.Lerp(Color.clear, Color.white, t);
            logo.transform.localScale = Vector3.Lerp(Vector3.one * 0.8f, Vector3.one, t);
            teamText.color = Color.Lerp(new Color(1, 1, 1, 0), new Color(1, 1, 1, 0.7f), t);

            yield return null;
        }

        logo.color = Color.white;
        logo.transform.localScale = Vector3.one;
        teamText.color = new Color(1, 1, 1, 0.7f);

        // 第二阶段：保持显示
        yield return new WaitForSeconds(3f);

        // 第三阶段：平滑淡出
        elapsed = 0f;
        duration = 1.5f;
        var startColor = logo.color;
        var startTextColor = teamText.color;
        var originalScale = logo.transform.localScale;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            var t = Mathf.SmoothStep(0f, 1f, elapsed / duration);

            logo.color = Color.Lerp(startColor, Color.clear, t);
            logo.transform.localScale = Vector3.Lerp(originalScale, originalScale * 0.9f, t);
            teamText.color = Color.Lerp(startTextColor, new Color(1, 1, 1, 0), t);

            yield return null;
        }

        logo.color = Color.clear;
        teamText.color = new Color(1, 1, 1, 0);

        // 清理对象
        if (logo != null)
            Object.Destroy(logo.gameObject);
        if (teamText != null)
            Object.Destroy(teamText.gameObject);

        TeamLogoActive = false;
    }

    private static IEnumerator CoLoadMod(SplashManager __instance)
    {
        var logo = CreateObject<SpriteRenderer>("COG-BG", null!, new Vector3(0, 0.5f, -4.9f));
        logo.sprite = LogoSprite;
        logo.color = Color.clear;
        logo.sortingOrder = 1;

        var bg = CreateObject<SpriteRenderer>("COG-LOADBG", null!, new Vector3(0, 0.5f, -5f));
        bg.sprite = BgSprite;
        bg.color = Color.clear;
        bg.sortingOrder = 0;

        // 使用 Mathf.SmoothStep 替代复杂的缓动函数
        var duration = 1.5f;
        var elapsed = 0f;

        // logo 淡入动画
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            var t = Mathf.SmoothStep(0f, 1f, elapsed / duration);
            logo.color = Color.Lerp(Color.clear, Color.white, t);
            logo.transform.localScale = Vector3.Lerp(Vector3.one * 0.9f, Vector3.one, t);
            yield return null;
        }

        // bg 延迟淡入
        yield return new WaitForSeconds(0.2f);
        elapsed = 0f;

        while (elapsed < duration * 0.7f)
        {
            elapsed += Time.deltaTime;
            var t = Mathf.SmoothStep(0f, 1f, elapsed / (duration * 0.7f));
            bg.color = Color.Lerp(Color.clear, Color.white, t);
            yield return null;
        }

        // 创建初始加载文字
        LoadText = Object.Instantiate(__instance.errorPopup.InfoText, null);
        LoadText.transform.localPosition = new Vector3(0f, -0.28f, -10f);
        LoadText.fontStyle = FontStyles.Bold;
        LoadText.text = LanguageConfig.Instance.Loading;
        LoadText.color = new Color(1, 1, 1, 0);

        // 文字淡入
        elapsed = 0f;
        while (elapsed < 0.8f)
        {
            elapsed += Time.deltaTime;
            LoadText.color = Color.Lerp(new Color(1, 1, 1, 0), new Color(1, 1, 1, 0.3f), elapsed / 0.8f);
            yield return null;
        }

        // 加载步骤
        string[] loadingSteps =
        [
            LanguageConfig.Instance.LoadingHotKey,
            LanguageConfig.Instance.LoadingListeners,
            LanguageConfig.Instance.LoadingWinners,
            LanguageConfig.Instance.LoadingRoles,
            LanguageConfig.Instance.LoadingSettings,
            LanguageConfig.Instance.LoadingPlugins,
            LanguageConfig.Instance.LoadingCommand,
            LanguageConfig.Instance.LoadingCompeleted
        ];

        // 执行每个加载步骤
        for (var i = 0; i < loadingSteps.Length; i++)
        {
            var step = loadingSteps[i];

            // 如果是最后一步（加载完成），先等待一下再显示
            if (i == loadingSteps.Length - 1) yield return new WaitForSeconds(0.5f);

            // 使用协程来确保文字切换动画完成
            yield return ChangeLoadingText(step, 0.3f);

            try
            {
                if (step == LanguageConfig.Instance.LoadingHotKey)
                {
                    _ = ButtonHotkeyConfig.Instance;
                }
                else if (step == LanguageConfig.Instance.LoadingListeners)
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
                else if (step == LanguageConfig.Instance.LoadingWinners)
                {
                    CustomWinnerManager.GetManager().RegisterCustomWinnables([
                        new CrewmatesCustomWinner(),
                    new ImpostorsCustomWinner(),
                    new LastPlayerCustomWinner()
                    ]);
                }
                else if (step == LanguageConfig.Instance.LoadingRoles)
                {
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
                }
                else if (step == LanguageConfig.Instance.LoadingSettings)
                {
                    _ = SettingsConfig.Instance;
                }
                else if (step == LanguageConfig.Instance.LoadingPlugins)
                {
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
                else if (step == LanguageConfig.Instance.LoadingCommand)
                {
                    CommandManager.GetManager().RegisterCommands([
                        new RpcCommand(),
                    new OptionCommand(),
                    new DebugCommand()
                    ]);
                }
            }
            catch (System.Exception e)
            {
                Main.Logger.LogError($"Error while loading: " + e);
            }

            // 如果不是最后一步，等待一下再继续
            if (i < loadingSteps.Length - 1) yield return new WaitForSeconds(0.3f);
        }

        // 加载完成后的效果 - 确保只有"加载完成"显示
        yield return new WaitForSeconds(0.5f);

        // 将文字颜色改为绿色
        elapsed = 0f;
        var startColor = LoadText.color;
        var targetColor = Color.green.AlphaMultiplied(0.6f);

        while (elapsed < 0.5f)
        {
            elapsed += Time.deltaTime;
            LoadText.color = Color.Lerp(startColor, targetColor, elapsed / 0.5f);
            yield return null;
        }

        // 平滑闪烁效果
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

        // 同时淡出所有元素
        elapsed = 0f;
        var originalLogoColor = logo.color;
        var originalBgColor = bg.color;
        var originalTextColor = LoadText.color;

        while (elapsed < 1f)
        {
            elapsed += Time.deltaTime;
            var t = elapsed / 1f;

            logo.color = Color.Lerp(originalLogoColor, Color.clear, t);
            bg.color = Color.Lerp(originalBgColor, Color.clear, t);
            LoadText.color = Color.Lerp(originalTextColor,
                new Color(originalTextColor.r, originalTextColor.g, originalTextColor.b, 0), t);

            yield return null;
        }

        // 清理
        if (LoadText != null) Object.Destroy(LoadText.gameObject);
        if (logo != null) Object.Destroy(logo.gameObject);
        if (bg != null) Object.Destroy(bg.gameObject);

        __instance.sceneChanger.AllowFinishLoadingScene();
        __instance.startedSceneLoad = true;
    }

    // 专门处理文字切换的方法
    private static IEnumerator ChangeLoadingText(string newText, float duration)
    {
        // 如果是第一次显示文字，不需要淡出效果
        if (LoadText.text == LanguageConfig.Instance.Loading)
        {
            LoadText.text = newText;
            yield break;
        }

        var elapsed = 0f;

        // 先淡出当前文字
        while (elapsed < duration / 2)
        {
            elapsed += Time.deltaTime;
            var t = elapsed / (duration / 2);
            LoadText.color = Color.Lerp(new Color(1, 1, 1, 0.3f), new Color(1, 1, 1, 0), t);
            yield return null;
        }

        // 更新文字
        LoadText.text = newText;

        // 再淡入新文字
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
        if (__instance.doneLoadingRefdata && !__instance.startedSceneLoad && Time.time - __instance.startTime > 4.2f &&
            !LoadedTeamLogo)
        {
            LoadedTeamLogo = true;
            __instance.StartCoroutine(CoLoadTeamLogo(__instance).WrapToIl2Cpp());
            return false;
        }

        // 只有在TeamLogo完全结束后才触发FracturedTruth加载
        if (__instance.doneLoadingRefdata && !__instance.startedSceneLoad && LoadedTeamLogo && !TeamLogoActive &&
            !_loadedFracturedTruth)
        {
            _loadedFracturedTruth = true;
            __instance.StartCoroutine(CoLoadMod(__instance).WrapToIl2Cpp());
        }

        return false;
    }

    // 辅助方法：更新加载文字（带动画）
    private static void UpdateLoadingText(string text)
    {
        if (LoadText != null) LoadText.text = text;
    }

    // 辅助方法：文字变化动画
    private static IEnumerator AnimateTextChange(string newText)
    {
        var elapsed = 0f;
        var duration = 0.2f;

        // 先缩小淡出
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            var t = elapsed / duration;
            LoadText.transform.localScale = Vector3.Lerp(Vector3.one, Vector3.one * 0.8f, t);
            LoadText.color = Color.Lerp(new Color(1, 1, 1, 0.3f), new Color(1, 1, 1, 0), t);
            yield return null;
        }

        // 更新文字
        LoadText.text = newText;

        // 再放大淡入
        elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            var t = elapsed / duration;
            LoadText.transform.localScale = Vector3.Lerp(Vector3.one * 0.8f, Vector3.one, t);
            LoadText.color = Color.Lerp(new Color(1, 1, 1, 0), new Color(1, 1, 1, 0.3f), t);
            yield return null;
        }
    }
}