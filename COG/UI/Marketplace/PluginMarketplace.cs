using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using COG.Config;
using COG.Config.Impl;
using COG.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace COG.UI.Marketplace;

/// <summary>
/// 插件市场中插件的信息
/// </summary>
public class MarketplacePluginInfo
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Author { get; set; } = "";
    public string Description { get; set; } = "";
    public string Version { get; set; } = "";
    public string DownloadUrl { get; set; } = "";
    public long FileSize { get; set; }
    public string UploadedAt { get; set; } = "";
}

/// <summary>
/// COG 插件市场界面。
///
/// 关键设计：本类不继承 MonoBehaviour，完全避免 AddComponent 泛型方法
/// （AddComponent&lt;T&gt; 会触发 IL2CPP MethodInfoStore 静态构造函数，
///  在类型注入完成前调用会抛出 NullReferenceException）。
///
/// 所有协程均委托给已存在的 MainMenuManager 实例执行。
/// UI 由纯 GameObject + SpriteRenderer + TextMeshPro 构成，
/// 不需要任何自定义 MonoBehaviour 子类。
/// </summary>
public class PluginMarketplace
{
    // ── 服务端地址 ───────────────────────────────────────
    private static string ServerUrl => "https://cog.amongusclub.cn";
    private static readonly HttpClient HttpClient = new();

    // ── 协程宿主（已注册的原生 MonoBehaviour） ──────────
    private readonly MainMenuManager _host;

    // ── UI 根节点 ────────────────────────────────────────
    private readonly GameObject _root;
    private GameObject _listPanel = null!;
    private GameObject _detailPanel = null!;
    private TextMeshPro _statusText = null!;

    // ── 列表状态 ─────────────────────────────────────────
    private readonly List<GameObject> _listItems = new();
    private readonly HashSet<string> _installedPlugins = new();
    private List<MarketplacePluginInfo> _allPlugins = new();
    private int _currentPage;
    private const int PageSize = 6;

    // ── 颜色 ─────────────────────────────────────────────
    private static readonly Color BackgroundColor = new(0.08f, 0.08f, 0.15f, 0.97f);
    private static readonly Color PanelColor      = new(0.12f, 0.12f, 0.22f, 1f);
    private static readonly Color AccentColor     = new(0.28f, 0.55f, 1f,    1f);
    private static readonly Color InstalledColor  = new(0.2f,  0.85f, 0.4f,  1f);
    private static readonly Color DangerColor     = new(0.9f,  0.25f, 0.25f, 1f);
    private static readonly Color TextColor       = Color.white;
    private static readonly Color SubTextColor    = new(0.7f, 0.7f, 0.8f, 1f);

    // ── 单例 ─────────────────────────────────────────────
    public static PluginMarketplace? Instance { get; private set; }

    // ─────────────────────────────────────────────────────
    // 静态入口
    // ─────────────────────────────────────────────────────
    public static void Open(MainMenuManager mainMenu)
    {
        if (Instance != null)
        {
            Instance._root.SetActive(true);
            return;
        }
        Instance = new PluginMarketplace(mainMenu);
    }

    // ─────────────────────────────────────────────────────
    // 构造 / 初始化
    // ─────────────────────────────────────────────────────
    private PluginMarketplace(MainMenuManager mainMenu)
    {
        _host = mainMenu;

        // 根节点挂在摄像机下，位于所有内容之前
        _root = new GameObject("COGMarketplace_Root");
        Object.DontDestroyOnLoad(_root);
        _root.transform.SetParent(Camera.main!.transform);
        _root.transform.localPosition = new Vector3(0f, 0f, -20f);
        _root.transform.localScale    = Vector3.one;

        BuildUI();

        mainMenu.mainMenuUI.SetActive(false);

        RefreshInstalledPlugins();
        _host.StartCoroutine(CoLoadPluginList().WrapToIl2Cpp());
    }

    // ─────────────────────────────────────────────────────
    // UI 构建
    // ─────────────────────────────────────────────────────
    private void BuildUI()
    {
        // 全屏背景
        CreateQuad(_root.transform, new Vector2(18f, 11f), BackgroundColor, "BG")
            .transform.localPosition = Vector3.zero;

        BuildTitleBar();
        BuildListPanel();
        BuildDetailPanel();

        _statusText = CreateText(_root.transform, "", 0.35f, SubTextColor);
        _statusText.transform.localPosition = new Vector3(0f, -4.7f, -1f);
        _statusText.alignment = TextAlignmentOptions.Center;
    }

    private void BuildTitleBar()
    {
        var bar = CreateQuad(_root.transform, new Vector2(18f, 0.9f), PanelColor, "TitleBar");
        bar.transform.localPosition = new Vector3(0f, 4.8f, -0.5f);

        var title = CreateText(bar.transform, "COG 插件市场", 0.55f, AccentColor);
        title.transform.localPosition = new Vector3(-3f, 0f, -0.2f);
        title.fontStyle = FontStyles.Bold;

        CreateButton(bar.transform, "↻ 刷新", new Vector3(6.2f, 0f, -0.2f), AccentColor, 0.38f, () =>
        {
            SetStatus("正在刷新列表...");
            _host.StartCoroutine(CoLoadPluginList().WrapToIl2Cpp());
        });

        CreateButton(bar.transform, "✕ 关闭", new Vector3(7.8f, 0f, -0.2f), DangerColor, 0.38f, Close);
    }

    private void BuildListPanel()
    {
        _listPanel = new GameObject("ListPanel");
        _listPanel.transform.SetParent(_root.transform);
        _listPanel.transform.localPosition = new Vector3(-4.5f, 0f, -0.5f);
        _listPanel.transform.localScale    = Vector3.one;

        CreateQuad(_listPanel.transform, new Vector2(8.5f, 9f), PanelColor, "ListBg")
            .transform.localPosition = Vector3.zero;

        var lbl = CreateText(_listPanel.transform, "可用插件", 0.38f, AccentColor);
        lbl.transform.localPosition = new Vector3(-3.2f, 4.1f, -0.2f);
        lbl.fontStyle = FontStyles.Bold;

        CreateButton(_listPanel.transform, "◀", new Vector3(-1.5f, -4.1f, -0.2f), AccentColor, 0.4f, () =>
        {
            if (_currentPage > 0) { _currentPage--; RefreshList(); }
        });
        CreateButton(_listPanel.transform, "▶", new Vector3(1.5f, -4.1f, -0.2f), AccentColor, 0.4f, () =>
        {
            int maxPage = Mathf.Max(0, (_allPlugins.Count - 1) / PageSize);
            if (_currentPage < maxPage) { _currentPage++; RefreshList(); }
        });
    }

    private void BuildDetailPanel()
    {
        _detailPanel = new GameObject("DetailPanel");
        _detailPanel.transform.SetParent(_root.transform);
        _detailPanel.transform.localPosition = new Vector3(3.8f, 0f, -0.5f);
        _detailPanel.transform.localScale    = Vector3.one;

        CreateQuad(_detailPanel.transform, new Vector2(9.5f, 9f), PanelColor, "DetailBg")
            .transform.localPosition = Vector3.zero;

        var hint = CreateText(_detailPanel.transform, "← 从左侧选择一个插件", 0.38f, SubTextColor);
        hint.transform.localPosition = new Vector3(0f, 0f, -0.2f);
        hint.alignment = TextAlignmentOptions.Center;
    }

    // ─────────────────────────────────────────────────────
    // 列表刷新
    // ─────────────────────────────────────────────────────
    private void RefreshList()
    {
        foreach (var go in _listItems) Object.Destroy(go);
        _listItems.Clear();

        int start = _currentPage * PageSize;
        int end   = Mathf.Min(start + PageSize, _allPlugins.Count);
        for (int i = start; i < end; i++)
            CreateListItem(_allPlugins[i], i - start);

        // 页码标签（懒创建）
        var pageLabelGo = _listPanel.transform.Find("PageLabel")?.gameObject;
        TextMeshPro pageLabel;
        if (pageLabelGo == null)
        {
            pageLabel = CreateText(_listPanel.transform, "", 0.32f, SubTextColor);
            pageLabel.gameObject.name = "PageLabel";
            pageLabel.transform.localPosition = new Vector3(0f, -4.1f, -0.2f);
            pageLabel.alignment = TextAlignmentOptions.Center;
        }
        else
        {
            pageLabel = pageLabelGo.GetComponent<TextMeshPro>();
        }

        int maxPage = Mathf.Max(0, (_allPlugins.Count - 1) / PageSize);
        pageLabel.text = $"第 {_currentPage + 1} / {maxPage + 1} 页";
    }

    private void CreateListItem(MarketplacePluginInfo plugin, int idx)
    {
        float yPos   = 3.3f - idx * 1.3f;
        var   item   = new GameObject($"Item_{plugin.Id}");
        item.transform.SetParent(_listPanel.transform);
        item.transform.localPosition = new Vector3(0f, yPos, -0.2f);
        item.transform.localScale    = Vector3.one;
        _listItems.Add(item);

        bool installed = _installedPlugins.Contains(plugin.Name);
        var  bgColor   = installed ? new Color(0.15f, 0.25f, 0.15f, 1f)
                                   : new Color(0.16f, 0.16f, 0.28f, 1f);

        var bg = CreateQuad(item.transform, new Vector2(7.8f, 1.1f), bgColor, "Bg");
        bg.transform.localPosition = Vector3.zero;

        var nameTmp = CreateText(item.transform, plugin.Name, 0.38f, TextColor);
        nameTmp.transform.localPosition = new Vector3(-2.8f, 0.2f, -0.1f);
        nameTmp.fontStyle = FontStyles.Bold;

        var infoTmp = CreateText(item.transform, $"作者: {plugin.Author}  v{plugin.Version}", 0.28f, SubTextColor);
        infoTmp.transform.localPosition = new Vector3(-2.8f, -0.2f, -0.1f);

        if (installed)
        {
            var badge = CreateText(item.transform, "✓ 已安装", 0.28f, InstalledColor);
            badge.transform.localPosition = new Vector3(2.8f, 0f, -0.1f);
            badge.alignment = TextAlignmentOptions.Right;
        }

        // 点击事件：BoxCollider2D + PassiveButton 均为原生 AU 类型，无需注册
        var col = bg.AddComponent<BoxCollider2D>();
        col.size = new Vector2(7.8f, 1.1f);
        var btn = bg.AddComponent<PassiveButton>();
        btn.OnClick = new Button.ButtonClickedEvent();
        var captured = plugin;
        btn.OnClick.AddListener((Action)(() => ShowDetail(captured)));
    }

    // ─────────────────────────────────────────────────────
    // 详情面板
    // ─────────────────────────────────────────────────────
    private void ShowDetail(MarketplacePluginInfo plugin)
    {
        // 清除上次内容，保留背景
        for (int i = _detailPanel.transform.childCount - 1; i >= 0; i--)
        {
            var child = _detailPanel.transform.GetChild(i);
            if (child.name != "DetailBg") Object.Destroy(child.gameObject);
        }

        bool installed = _installedPlugins.Contains(plugin.Name);

        var nameTmp = CreateText(_detailPanel.transform, plugin.Name, 0.5f, TextColor);
        nameTmp.transform.localPosition = new Vector3(0f, 3.6f, -0.2f);
        nameTmp.fontStyle  = FontStyles.Bold;
        nameTmp.alignment  = TextAlignmentOptions.Center;

        CreateQuad(_detailPanel.transform, new Vector2(8.5f, 0.02f), AccentColor, "Line")
            .transform.localPosition = new Vector3(0f, 3.2f, -0.2f);

        InfoRow("作者",     plugin.Author,              2.8f);
        InfoRow("版本",     plugin.Version,             2.3f);
        InfoRow("大小",     FmtSize(plugin.FileSize),   1.8f);
        InfoRow("上传时间", plugin.UploadedAt,          1.3f);

        var descLbl = CreateText(_detailPanel.transform, "描述", 0.35f, AccentColor);
        descLbl.transform.localPosition = new Vector3(-4f, 0.7f, -0.2f);
        descLbl.fontStyle = FontStyles.Bold;

        var desc = CreateText(_detailPanel.transform, plugin.Description, 0.32f, SubTextColor);
        desc.transform.localPosition     = new Vector3(0f, -0.2f, -0.2f);
        desc.alignment                   = TextAlignmentOptions.TopLeft;
        desc.rectTransform.sizeDelta     = new Vector2(8f, 2.5f);
        desc.overflowMode                = TextOverflowModes.Ellipsis;
        desc.enableWordWrapping          = true;

        if (!installed)
        {
            CreateButton(_detailPanel.transform, "⬇ 安装", new Vector3(0f, -3.5f, -0.2f), AccentColor, 0.4f,
                () => _host.StartCoroutine(CoInstall(plugin).WrapToIl2Cpp()));
        }
        else
        {
            CreateButton(_detailPanel.transform, "✓ 已安装", new Vector3(-2f, -3.5f, -0.2f), InstalledColor, 0.4f,
                () => SetStatus($"插件 {plugin.Name} 已安装，重启生效。"));
            CreateButton(_detailPanel.transform, "🗑 卸载", new Vector3(2f, -3.5f, -0.2f), DangerColor, 0.4f,
                () => Uninstall(plugin));
        }

        CreateButton(_detailPanel.transform, "⚠ 服务端删除", new Vector3(0f, -4.15f, -0.2f), DangerColor, 0.32f,
            () => _host.StartCoroutine(CoServerDelete(plugin).WrapToIl2Cpp()));
    }

    private void InfoRow(string label, string value, float y)
    {
        var l = CreateText(_detailPanel.transform, label + ":", 0.33f, AccentColor);
        l.transform.localPosition = new Vector3(-3.5f, y, -0.2f);
        l.fontStyle = FontStyles.Bold;

        var v = CreateText(_detailPanel.transform, value, 0.33f, TextColor);
        v.transform.localPosition = new Vector3(0.5f, y, -0.2f);
    }

    // ─────────────────────────────────────────────────────
    // 网络协程（全部在 _host 上运行）
    // ─────────────────────────────────────────────────────
    private IEnumerator CoLoadPluginList()
    {
        SetStatus("正在获取插件列表...");
        _allPlugins.Clear();

        string json = "";
        yield return Task.Run(async () =>
        {
            try   { json = await HttpClient.GetStringAsync($"{ServerUrl}/api/plugins"); }
            catch (System.Exception ex) { Main.Logger.LogError($"[Marketplace] 列表请求失败: {ex.Message}"); }
        }).WaitAsCoroutine();

        if (string.IsNullOrEmpty(json))
        {
            SetStatus("✗ 无法连接到服务端。");
            RefreshList();
            yield break;
        }

        try
        {
            _allPlugins = ParseList(json);
            RefreshInstalledPlugins();
            RefreshList();
            SetStatus($"已加载 {_allPlugins.Count} 个插件。");
        }
        catch (System.Exception ex)
        {
            Main.Logger.LogError($"[Marketplace] 解析失败: {ex.Message}");
            SetStatus("✗ 解析插件数据失败。");
        }
    }

    private IEnumerator CoInstall(MarketplacePluginInfo plugin)
    {
        SetStatus($"正在下载 {plugin.Name}...");
        byte[] data = Array.Empty<byte>();

        yield return Task.Run(async () =>
        {
            try   { data = await HttpClient.GetByteArrayAsync($"{ServerUrl}/api/plugins/{plugin.Id}/download"); }
            catch (System.Exception ex) { Main.Logger.LogError($"[Marketplace] 下载失败: {ex.Message}"); }
        }).WaitAsCoroutine();

        if (data.Length == 0) { SetStatus($"✗ 下载 {plugin.Name} 失败。"); yield break; }

        try
        {
            var dir = Path.Combine(ConfigBase.DataDirectoryName, "plugins");
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
            File.WriteAllBytes(Path.Combine(dir, $"{plugin.Name}.ca"), data);
            _installedPlugins.Add(plugin.Name);
            SetStatus($"✓ {plugin.Name} 安装成功！重启游戏后生效。");
            ShowDetail(plugin);
            RefreshList();
        }
        catch (System.Exception ex)
        {
            Main.Logger.LogError($"[Marketplace] 保存失败: {ex.Message}");
            SetStatus($"✗ 保存失败：{ex.Message}");
        }
    }

    private void Uninstall(MarketplacePluginInfo plugin)
    {
        try
        {
            var path = Path.Combine(ConfigBase.DataDirectoryName, "plugins", $"{plugin.Name}.ca");
            if (File.Exists(path)) File.Delete(path);
            _installedPlugins.Remove(plugin.Name);
            SetStatus($"✓ {plugin.Name} 已卸载，重启后完全移除。");
            ShowDetail(plugin);
            RefreshList();
        }
        catch (System.Exception ex) { SetStatus($"✗ 卸载失败：{ex.Message}"); }
    }

    private IEnumerator CoServerDelete(MarketplacePluginInfo plugin)
    {
        SetStatus($"正在从服务端删除 {plugin.Name}...");
        int code = 0;

        yield return Task.Run(async () =>
        {
            try
            {
                var resp = await HttpClient.DeleteAsync($"{ServerUrl}/api/plugins/{plugin.Id}");
                code = (int)resp.StatusCode;
            }
            catch (System.Exception ex) { Main.Logger.LogError($"[Marketplace] 服务端删除失败: {ex.Message}"); }
        }).WaitAsCoroutine();

        if (code is >= 200 and < 300)
        {
            SetStatus($"✓ 已从服务端删除 {plugin.Name}。");
            _host.StartCoroutine(CoLoadPluginList().WrapToIl2Cpp());
        }
        else
        {
            SetStatus($"✗ 服务端删除失败 (HTTP {code})。");
        }
    }

    // ─────────────────────────────────────────────────────
    // 辅助
    // ─────────────────────────────────────────────────────
    private void RefreshInstalledPlugins()
    {
        _installedPlugins.Clear();
        var dir = Path.Combine(ConfigBase.DataDirectoryName, "plugins");
        if (!Directory.Exists(dir)) return;
        foreach (var f in Directory.GetFiles(dir, "*.ca"))
            _installedPlugins.Add(Path.GetFileNameWithoutExtension(f));
    }

    private void SetStatus(string msg) { if (_statusText != null) _statusText.text = msg; }

    public void Close()
    {
        _host.mainMenuUI.SetActive(true);
        Instance = null;
        Object.Destroy(_root);
    }

    private static string FmtSize(long b)
    {
        if (b < 1024)           return $"{b} B";
        if (b < 1024 * 1024)    return $"{b / 1024.0:F1} KB";
        return $"{b / 1024.0 / 1024.0:F1} MB";
    }

    // ── 轻量 JSON 解析 ────────────────────────────────────
    private static List<MarketplacePluginInfo> ParseList(string json)
    {
        var result = new List<MarketplacePluginInfo>();
        json = json.Trim();
        if (!json.StartsWith("[")) return result;

        int depth = 0, start = -1;
        for (int i = 0; i < json.Length; i++)
        {
            if      (json[i] == '{') { depth++; if (depth == 1) start = i; }
            else if (json[i] == '}')
            {
                depth--;
                if (depth == 0 && start >= 0)
                {
                    var p = ParseObj(json.Substring(start, i - start + 1));
                    if (p != null) result.Add(p);
                    start = -1;
                }
            }
        }
        return result;
    }

    private static MarketplacePluginInfo? ParseObj(string s)
    {
        try
        {
            var p = new MarketplacePluginInfo
            {
                Id          = int.Parse(JVal(s, "id")),
                Name        = JVal(s, "name"),
                Author      = JVal(s, "author"),
                Description = JVal(s, "description"),
                Version     = JVal(s, "version"),
                DownloadUrl = JVal(s, "download_url"),
                UploadedAt  = JVal(s, "uploaded_at"),
            };
            if (long.TryParse(JVal(s, "file_size"), out var sz)) p.FileSize = sz;
            return p;
        }
        catch { return null; }
    }

    private static string JVal(string s, string key)
    {
        var search = $"\"{key}\":";
        int i = s.IndexOf(search, StringComparison.Ordinal);
        if (i < 0) return "";
        i += search.Length;
        while (i < s.Length && s[i] == ' ') i++;
        if (i >= s.Length) return "";

        if (s[i] == '"')
        {
            i++;
            var sb = new StringBuilder();
            while (i < s.Length && s[i] != '"')
            {
                if (s[i] == '\\') i++;
                if (i < s.Length) sb.Append(s[i]);
                i++;
            }
            return sb.ToString();
        }

        int e = i;
        while (e < s.Length && s[e] != ',' && s[e] != '}') e++;
        return s.Substring(i, e - i).Trim();
    }

    // ── UI 工厂方法（只用原生 AU 组件，无自定义 MonoBehaviour） ──
    private static GameObject CreateQuad(Transform parent, Vector2 size, Color color, string name)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent);
        go.transform.localScale    = Vector3.one;
        go.transform.localPosition = Vector3.zero;

        var tex = new Texture2D(1, 1);
        tex.SetPixel(0, 0, Color.white);
        tex.Apply();

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite   = Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
        sr.color    = color;
        sr.drawMode = SpriteDrawMode.Sliced;
        sr.size     = size;
        return go;
    }

    private static TextMeshPro CreateText(Transform parent, string text, float size, Color color)
    {
        var safe = (text.Length > 16 ? text[..16] : text).Replace(" ", "_").Replace("/", "_");
        var go   = new GameObject($"T_{safe}");
        go.transform.SetParent(parent);
        go.transform.localScale    = Vector3.one;
        go.transform.localPosition = Vector3.zero;

        var tmp = go.AddComponent<TextMeshPro>();
        tmp.text               = text;
        tmp.fontSize           = size;
        tmp.color              = color;
        tmp.alignment          = TextAlignmentOptions.Left;
        tmp.enableWordWrapping = false;
        tmp.sortingOrder       = 5;
        return tmp;
    }

    private static void CreateButton(Transform parent, string label, Vector3 pos, Color color,
                                     float fontSize, Action onClick)
    {
        var go = new GameObject($"Btn_{label.Replace(" ", "_")[..Math.Min(label.Length, 12)]}");
        go.transform.SetParent(parent);
        go.transform.localPosition = pos;
        go.transform.localScale    = Vector3.one;

        var bg = CreateQuad(go.transform, new Vector2(2.8f, 0.6f), color, "Bg");
        bg.transform.localPosition = Vector3.zero;

        var txt = CreateText(go.transform, label, fontSize, TextColor);
        txt.transform.localPosition = new Vector3(0f, 0f, -0.1f);
        txt.alignment = TextAlignmentOptions.Center;

        // BoxCollider2D 和 PassiveButton 均是原生 AU 类型，不需要 RegisterInIl2Cpp
        var col = bg.AddComponent<BoxCollider2D>();
        col.size = new Vector2(2.8f, 0.6f);

        var btn = bg.AddComponent<PassiveButton>();
        btn.OnClick = new Button.ButtonClickedEvent();
        btn.OnClick.AddListener(onClick);
    }
}
