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

public class MarketplacePluginInfo
{
    public int    Id          { get; set; }
    public string Name        { get; set; } = "";
    public string Author      { get; set; } = "";
    public string Description { get; set; } = "";
    public string Version     { get; set; } = "";
    public string DownloadUrl { get; set; } = "";
    public long   FileSize    { get; set; }
    public string UploadedAt  { get; set; } = "";
}

public class PluginMarketplace
{
    private const float ScreenW  = 10.4f;   // full backdrop width
    private const float ScreenH  =  6.0f;   // full backdrop height

    private const float TitleH   =  0.55f;  // title bar height
    private const float TitleY   =  2.65f;  // title bar centre Y
    private const float ContentY =  0.0f;   // content area centre Y (below title)
    private const float ContentH =  5.0f;   // content area height

    // Left list panel
    private const float ListX    = -2.8f;
    private const float ListW    =  4.6f;

    // Right detail panel
    private const float DetailX  =  2.5f;
    private const float DetailW  =  5.4f;

    // Text sizes
    private const float FzTitle  = 2.2f;
    private const float FzH2     = 1.7f;
    private const float FzBody   = 1.4f;
    private const float FzSmall  = 1.1f;
    private const float FzBtn    = 1.35f;

    // Button dimensions
    private const float BtnW = 2.2f;
    private const float BtnH = 0.52f;

    // ── Colours ───────────
    private static readonly Color ColBg       = new(0.07f, 0.07f, 0.14f, 0.98f);
    private static readonly Color ColPanel    = new(0.11f, 0.11f, 0.21f, 1f);
    private static readonly Color ColItem     = new(0.14f, 0.14f, 0.26f, 1f);
    private static readonly Color ColItemSel  = new(0.16f, 0.22f, 0.40f, 1f);
    private static readonly Color ColAccent   = new(0.30f, 0.58f, 1f,   1f);
    private static readonly Color ColGreen    = new(0.20f, 0.82f, 0.40f, 1f);
    private static readonly Color ColRed      = new(0.88f, 0.24f, 0.24f, 1f);
    private static readonly Color ColText     = Color.white;
    private static readonly Color ColSub      = new(0.68f, 0.68f, 0.78f, 1f);
    private static readonly Color ColDivider  = new(0.25f, 0.35f, 0.55f, 1f);

    // ── State ───────────
    private static readonly HttpClient Http = new();

    private readonly MainMenuManager _host;
    private readonly GameObject      _root;

    private GameObject   _listPanel   = null!;
    private GameObject   _detailPanel = null!;
    private TextMeshPro  _statusText  = null!;

    private readonly List<GameObject>          _listItems       = new();
    private readonly HashSet<string>           _installedPlugins = new();
    private          List<MarketplacePluginInfo> _plugins         = new();
    private          int                       _page;
    private const    int                       PageSize = 5;

    private static string ServerUrl => "https://cog.amongusclub.cn/index.php";

    public static PluginMarketplace? Instance { get; private set; }

    public static void Open(MainMenuManager mainMenu)
    {
        if (Instance != null) { Instance._root.SetActive(true); return; }
        Instance = new PluginMarketplace(mainMenu);
    }

    private PluginMarketplace(MainMenuManager mainMenu)
    {
        _host = mainMenu;

        _root = new GameObject("COGMarketplace");
        Object.DontDestroyOnLoad(_root);
        _root.transform.SetParent(Camera.main!.transform, false);
        _root.transform.localPosition = new Vector3(0f, 0f, -30f);
        _root.transform.localScale    = Vector3.one;

        BuildUI();
        mainMenu.mainMenuUI.SetActive(false);
        RefreshInstalled();
        _host.StartCoroutine(CoLoad().WrapToIl2Cpp());
    }

    private void BuildUI()
    {
        // Full-screen backdrop
        Quad(_root.transform, "BG", ScreenW, ScreenH, ColBg, 0f, 0f, 0);

        // ── Title bar ────
        Quad(_root.transform, "TitleBar", ScreenW, TitleH, ColPanel, 0f, TitleY, 1);
        Txt(_root.transform, "COG 插件市场", FzTitle, ColAccent,
            new Vector3(-3.8f, TitleY, -2f), bold: true);

        Btn(_root.transform, "BtnRefresh", "刷新", ColAccent,
            new Vector3(3.4f, TitleY, -2f), BtnW * 0.8f, BtnH, FzBtn, () =>
        {
            SetStatus("正在刷新列表...");
            _host.StartCoroutine(CoLoad().WrapToIl2Cpp());
        });
        Btn(_root.transform, "BtnClose", "关闭", ColRed,
            new Vector3(4.8f, TitleY, -2f), BtnW * 0.8f, BtnH, FzBtn, Close);

        // ── Left panel ───────
        _listPanel = new GameObject("ListPanel");
        _listPanel.transform.SetParent(_root.transform, false);
        _listPanel.transform.localPosition = new Vector3(ListX, ContentY, -1f);

        Quad(_listPanel.transform, "Bg", ListW, ContentH, ColPanel, 0f, 0f, 0);
        Txt(_listPanel.transform, "可用插件", FzH2, ColAccent,
            new Vector3(-ListW / 2f + 0.2f, ContentH / 2f - 0.35f, -1f), bold: true);
        Divider(_listPanel.transform, ListW - 0.3f, ContentH / 2f - 0.62f);

        // Pagination
        Btn(_listPanel.transform, "BtnPrev", "◀", ColAccent,
            new Vector3(-0.9f, -ContentH / 2f + 0.35f, -1f), 0.9f, BtnH, FzBtn, () =>
        {
            if (_page > 0) { _page--; RedrawList(); }
        });
        Btn(_listPanel.transform, "BtnNext", "▶", ColAccent,
            new Vector3(0.9f, -ContentH / 2f + 0.35f, -1f), 0.9f, BtnH, FzBtn, () =>
        {
            int max = Mathf.Max(0, (_plugins.Count - 1) / PageSize);
            if (_page < max) { _page++; RedrawList(); }
        });

        // ── Right panel ──────
        _detailPanel = new GameObject("DetailPanel");
        _detailPanel.transform.SetParent(_root.transform, false);
        _detailPanel.transform.localPosition = new Vector3(DetailX, ContentY, -1f);

        Quad(_detailPanel.transform, "Bg", DetailW, ContentH, ColPanel, 0f, 0f, 0);
        Txt(_detailPanel.transform, "← 从左侧选择一个插件", FzBody, ColSub,
            Vector3.back, align: TextAlignmentOptions.Center);

        // ── Status bar ───────
        _statusText = Txt(_root.transform, "", FzSmall, ColSub,
            new Vector3(0f, -ScreenH / 2f + 0.25f, -2f), align: TextAlignmentOptions.Center);
    }

    // ─────────────────────────
    // List rendering
    // ─────────────────────────
    private void RedrawList()
    {
        foreach (var go in _listItems) Object.Destroy(go);
        _listItems.Clear();

        int start = _page * PageSize;
        int end   = Mathf.Min(start + PageSize, _plugins.Count);

        float itemH = 0.78f;
        float topY  = ContentH / 2f - 0.82f;

        for (int i = start; i < end; i++)
            DrawListItem(_plugins[i], i - start, topY - i * itemH + start * itemH);

        // Page label
        var pg = _listPanel.transform.Find("PageLabel")?.GetComponent<TextMeshPro>();
        if (pg == null)
        {
            pg = Txt(_listPanel.transform, "", FzSmall, ColSub,
                new Vector3(0f, -ContentH / 2f + 0.35f, -1f), align: TextAlignmentOptions.Center);
            pg.gameObject.name = "PageLabel";
        }
        int maxPage = Mathf.Max(0, (_plugins.Count - 1) / PageSize);
        pg.text = $"第 {_page + 1} / {maxPage + 1} 页";
    }

    private void DrawListItem(MarketplacePluginInfo p, int slot, float y)
    {
        float itemH = 0.72f;
        float itemW = ListW - 0.2f;

        var item = new GameObject($"Item_{p.Id}");
        item.transform.SetParent(_listPanel.transform, false);
        item.transform.localPosition = new Vector3(0f, y, -1f);
        _listItems.Add(item);

        bool inst = _installedPlugins.Contains(p.Name);

        // Background quad – Simple mode, sized via localScale
        var bgGo = Quad(item.transform, "Bg", itemW, itemH,
            inst ? new Color(0.14f, 0.24f, 0.14f, 1f) : ColItem, 0f, 0f, 0);

        // Name
        Txt(item.transform, p.Name, FzBody, ColText,
            new Vector3(-itemW / 2f + 0.18f, 0.13f, -1f), bold: true);

        // Author / version
        Txt(item.transform, $"by {p.Author}  v{p.Version}", FzSmall, ColSub,
            new Vector3(-itemW / 2f + 0.18f, -0.16f, -1f));

        if (inst)
            Txt(item.transform, "OK", FzBody, ColGreen,
                new Vector3(itemW / 2f - 0.22f, 0f, -1f), align: TextAlignmentOptions.Right);

        // Click — collider on the bg quad (same GO as SpriteRenderer)
        var col = bgGo.AddComponent<BoxCollider2D>();
        col.size = new Vector2(itemW, itemH);
        var btn = bgGo.AddComponent<PassiveButton>();
        btn.OnClick = new Button.ButtonClickedEvent();
        var captured = p;
        btn.OnClick.AddListener((Action)(() => ShowDetail(captured)));

        // Hover tint
        var sr = bgGo.GetComponent<SpriteRenderer>();
        btn.OnMouseOver = new UnityEngine.Events.UnityEvent();
        btn.OnMouseOver.AddListener((Action)(() => sr.color = ColItemSel));
        btn.OnMouseOut  = new UnityEngine.Events.UnityEvent();
        btn.OnMouseOut.AddListener((Action)(() => sr.color = inst
            ? new Color(0.14f, 0.24f, 0.14f, 1f) : ColItem));
    }

    // ─────────────────────────
    // Detail panel
    // ─────────────────────────
    private void ShowDetail(MarketplacePluginInfo p)
    {
        // Clear old children except the background
        for (int i = _detailPanel.transform.childCount - 1; i >= 0; i--)
        {
            var ch = _detailPanel.transform.GetChild(i);
            if (ch.name != "Bg") Object.Destroy(ch.gameObject);
        }

        bool inst = _installedPlugins.Contains(p.Name);
        float hw  = DetailW / 2f;

        // Plugin name
        Txt(_detailPanel.transform, p.Name, FzH2, ColText,
            new Vector3(0f, ContentH / 2f - 0.42f, -1f), bold: true, align: TextAlignmentOptions.Center);

        Divider(_detailPanel.transform, DetailW - 0.4f, ContentH / 2f - 0.72f);

        // Info rows
        float ry = ContentH / 2f - 1.05f;
        DetailRow("作者",     p.Author,           ref ry);
        DetailRow("版本",     p.Version,          ref ry);
        DetailRow("大小",     FmtSize(p.FileSize), ref ry);
        DetailRow("上传",     p.UploadedAt,       ref ry);

        // Description
        Divider(_detailPanel.transform, DetailW - 0.4f, ry - 0.05f);
        ry -= 0.2f;
        var descTmp = Txt(_detailPanel.transform, p.Description.Length > 0 ? p.Description : "(无描述)",
            FzSmall, ColSub, new Vector3(0f, ry - 0.5f, -1f), align: TextAlignmentOptions.TopJustified);
        descTmp.enableWordWrapping  = true;
        descTmp.overflowMode        = TextOverflowModes.Ellipsis;
        descTmp.rectTransform.sizeDelta = new Vector2(DetailW - 0.5f, 1.6f);

        // Action buttons
        float btnY = -ContentH / 2f + 0.9f;
        if (!inst)
        {
            Btn(_detailPanel.transform, "BtnInstall", "安装插件", ColAccent,
                new Vector3(0f, btnY, -1f), BtnW, BtnH, FzBtn,
                () => _host.StartCoroutine(CoInstall(p).WrapToIl2Cpp()));
        }
        else
        {
            Btn(_detailPanel.transform, "BtnInstalled", "已安装", ColGreen,
                new Vector3(-1.3f, btnY, -1f), BtnW, BtnH, FzBtn,
                () => SetStatus($"{p.Name} 已安装，重启游戏后生效。"));
            Btn(_detailPanel.transform, "BtnUninstall", "卸载", ColRed,
                new Vector3(1.3f, btnY, -1f), BtnW, BtnH, FzBtn,
                () => DoUninstall(p));
        }

        Btn(_detailPanel.transform, "BtnDel", "从服务端删除", ColRed,
            new Vector3(0f, btnY - 0.62f, -1f), BtnW * 1.2f, BtnH * 0.85f, FzSmall,
            () => _host.StartCoroutine(CoServerDelete(p).WrapToIl2Cpp()));
    }

    private void DetailRow(string label, string value, ref float y)
    {
        float hw = DetailW / 2f;
        Txt(_detailPanel.transform, label + ":", FzSmall, ColAccent,
            new Vector3(-hw + 0.2f, y, -1f), bold: true);
        Txt(_detailPanel.transform, value, FzSmall, ColText,
            new Vector3(-hw + 1.5f, y, -1f));
        y -= 0.38f;
    }

    // ─────────────────────────
    // Network coroutines
    // ─────────────────────────
    private IEnumerator CoLoad()
    {
        SetStatus("正在获取插件列表...");
        _plugins.Clear();
        string json = "";

        yield return Task.Run(async () =>
        {
            try   { json = await Http.GetStringAsync($"{ServerUrl}/api/plugins"); }
            catch (System.Exception ex) { Main.Logger.LogError($"[Market] {ex.Message}"); }
        }).WaitAsCoroutine();

        if (string.IsNullOrEmpty(json))
        {
            SetStatus("无法连接到服务端，请检查server-url。");
            RedrawList();
            yield break;
        }

        try
        {
            _plugins = ParseList(json);
            RefreshInstalled();
            RedrawList();
            SetStatus($"共 {_plugins.Count} 个插件。");
        }
        catch (System.Exception ex)
        {
            Main.Logger.LogError($"[Market] Parse error: {ex.Message}");
            SetStatus("解析数据失败。");
        }
    }

    private IEnumerator CoInstall(MarketplacePluginInfo p)
    {
        SetStatus($"正在下载 {p.Name}…");
        byte[] data = Array.Empty<byte>();

        yield return Task.Run(async () =>
        {
            try   { data = await Http.GetByteArrayAsync($"{ServerUrl}/api/plugins/{p.Id}/download"); }
            catch (System.Exception ex) { Main.Logger.LogError($"[Market] Download: {ex.Message}"); }
        }).WaitAsCoroutine();

        if (data.Length == 0) { SetStatus($"下载 {p.Name} 失败。"); yield break; }

        try
        {
            var dir = Path.Combine(ConfigBase.DataDirectoryName, "plugins");
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
            File.WriteAllBytes(Path.Combine(dir, $"{p.Name}.ca"), data);
            _installedPlugins.Add(p.Name);
            SetStatus($"{p.Name} 安装成功，重启游戏后生效。");
            ShowDetail(p);
            RedrawList();
        }
        catch (System.Exception ex) { SetStatus($"保存失败：{ex.Message}"); }
    }

    private void DoUninstall(MarketplacePluginInfo p)
    {
        try
        {
            var path = Path.Combine(ConfigBase.DataDirectoryName, "plugins", $"{p.Name}.ca");
            if (File.Exists(path)) File.Delete(path);
            _installedPlugins.Remove(p.Name);
            SetStatus($"✓ {p.Name} 已卸载，重启后完全移除。");
            ShowDetail(p);
            RedrawList();
        }
        catch (System.Exception ex) { SetStatus($"卸载失败：{ex.Message}"); }
    }

    private IEnumerator CoServerDelete(MarketplacePluginInfo p)
    {
        SetStatus($"正在从服务端删除 {p.Name}…");
        int code = 0;

        yield return Task.Run(async () =>
        {
            try
            {
                var r = await Http.DeleteAsync($"{ServerUrl}/api/plugins/{p.Id}");
                code = (int)r.StatusCode;
            }
            catch (System.Exception ex) { Main.Logger.LogError($"[Market] Delete: {ex.Message}"); }
        }).WaitAsCoroutine();

        if (code is >= 200 and < 300)
        {
            SetStatus($"已从服务端删除 {p.Name}。");
            _host.StartCoroutine(CoLoad().WrapToIl2Cpp());
        }
        else
        {
            SetStatus($"服务端删除失败 (HTTP {code})。");
        }
    }

    // ─────────────────────────
    // Helpers
    // ─────────────────────────
    private void RefreshInstalled()
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
        if (b < 1024)        return $"{b} B";
        if (b < 1048576)     return $"{b / 1024.0:F1} KB";
        return $"{b / 1048576.0:F1} MB";
    }

    // ─────────────────────────
    // UI factory helpers
    // ─────────────────────────

    /// <summary>
    /// Create a solid-colour rectangle using SpriteRenderer in Simple mode.
    /// Size is applied via localScale so no border info is needed on the sprite.
    /// </summary>
    private static GameObject Quad(Transform parent, string name,
        float w, float h, Color color, float lx, float ly, int order)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.localPosition = new Vector3(lx, ly, 0f);
        go.transform.localScale    = new Vector3(w, h, 1f);

        // 1×1 white texture, pixelsPerUnit = 1 → sprite is exactly 1 world-unit
        // after which localScale stretches it to (w × h) world units.
        var tex = new Texture2D(1, 1, TextureFormat.RGBA32, false);
        tex.SetPixel(0, 0, Color.white);
        tex.Apply();

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite       = Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
        sr.color        = color;
        sr.drawMode     = SpriteDrawMode.Simple;   // ← Simple, not Sliced
        sr.sortingOrder = order;
        return go;
    }

    /// <summary>
    /// Create a TextMeshPro label. fontSize is in world units (1.4–2.2 is readable).
    /// </summary>
    private static TextMeshPro Txt(Transform parent, string text, float fontSize, Color color,
        Vector3 localPos, bool bold = false, TextAlignmentOptions align = TextAlignmentOptions.Left)
    {
        var safe = text.Length > 20 ? text[..20] : text;
        safe = safe.Replace(" ", "_").Replace("/", "_").Replace("\\", "_");
        var go = new GameObject($"T_{safe}");
        go.transform.SetParent(parent, false);
        go.transform.localPosition = localPos;
        go.transform.localScale    = Vector3.one;

        var tmp = go.AddComponent<TextMeshPro>();
        tmp.text               = text;
        tmp.fontSize           = fontSize;
        tmp.color              = color;
        tmp.alignment          = align;
        tmp.enableWordWrapping = false;
        tmp.fontStyle          = bold ? FontStyles.Bold : FontStyles.Normal;
        tmp.sortingOrder       = 5;
        return tmp;
    }

    /// <summary>
    /// Create a clickable button: a coloured Quad with a centred text label.
    /// The collider is placed on the Quad so its world size matches exactly.
    /// </summary>
    private static void Btn(Transform parent, string name, string label,
        Color color, Vector3 localPos, float w, float h, float fontSize, Action onClick)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.localPosition = localPos;
        go.transform.localScale    = Vector3.one;

        // Background
        var bgGo = Quad(go.transform, "Bg", w, h, color, 0f, 0f, 3);

        // Label (slightly in front)
        var lbl = Txt(go.transform, label, fontSize, Color.white,
            new Vector3(0f, 0f, -0.1f), align: TextAlignmentOptions.Center);
        lbl.sortingOrder = 4;

        // Collider on the bg quad (localScale = (w,h,1) so BoxCollider2D size (1,1) = w×h world)
        var col = bgGo.AddComponent<BoxCollider2D>();
        col.size = Vector2.one;   // bg scale is already (w, h, 1), so (1,1) collider = exact fit

        var btn = bgGo.AddComponent<PassiveButton>();
        btn.OnClick = new Button.ButtonClickedEvent();
        btn.OnClick.AddListener(onClick);

        // Hover brightness
        var sr = bgGo.GetComponent<SpriteRenderer>();
        btn.OnMouseOver = new UnityEngine.Events.UnityEvent();
        btn.OnMouseOver.AddListener((Action)(() =>
            sr.color = new Color(
                Mathf.Min(color.r + 0.12f, 1f),
                Mathf.Min(color.g + 0.12f, 1f),
                Mathf.Min(color.b + 0.12f, 1f), 1f)));
        btn.OnMouseOut = new UnityEngine.Events.UnityEvent();
        btn.OnMouseOut.AddListener((Action)(() => sr.color = color));
    }

    /// <summary>Thin horizontal divider line.</summary>
    private void Divider(Transform parent, float w, float y)
        => Quad(parent, "Div", w, 0.018f, ColDivider, 0f, y, 2);

    // ─────────────────────────
    // JSON parser (no external dependencies)
    // ─────────────────────────
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
                    var obj = ParseObj(json.Substring(start, i - start + 1));
                    if (obj != null) result.Add(obj);
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
}
