using COG.States;
using COG.Utils;
using COG.Utils.Coding;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Debug = System.Diagnostics.Debug;

namespace COG.UI.CustomButton;

#pragma warning disable CS0659

[ShitCode]
public class CustomButton
{
    public const string ModdedFlag = "Modded_";

    private CustomButton(string identifier, Action onClick, Action onMeetingEnd, Action onEffect, Func<bool> couldUse,
        Func<bool> hasButton,
        Sprite sprite, Vector3? position, KeyCode? hotkey, string text, bool hasEffect, Func<float> cooldown,
        float effectTime,
        int usesLimit, string hotkeyName)
    {
        Identifier = identifier;
        OnClick = onClick;
        OnMeetingEnd = onMeetingEnd;
        OnEffect = onEffect;
        CouldUse = couldUse;
        HasButton = hasButton;
        Sprite = sprite;
        Position = position ?? Vector3.zero;
        if (position is null) AutoPosition = true;
        Hotkey = hotkey;
        Text = text;
        HasEffect = hasEffect;
        Cooldown = cooldown;
        EffectTime = effectTime;
        UsesLimit = UsesRemaining = usesLimit;
        HotkeyName = hotkeyName;
        Id = _availableId++;
    }

    private static int _availableId = 0;

    public int Id { get; }
    public string Identifier { get; }

    public ActionButton? ActionButton { get; set; }
    public Func<float> Cooldown { get; set; }
    public Func<bool> CouldUse { get; set; }
    public float EffectTime { get; set; }
    public GameObject? GameObject { get; set; }
    public Func<bool> HasButton { get; set; }
    public bool HasEffect { get; set; }
    public KeyCode? Hotkey { get; set; }
    public string HotkeyName { get; set; }

    public HudManager? Hud { get; set; }
    public bool IsEffectActive { get; set; }
    public Material? Material { get; set; }
    public Action OnClick { get; set; }
    public Action? OnEffect { get; set; }
    public Action OnMeetingEnd { get; set; }
    public PassiveButton? PassiveButton { get; set; }
    public Vector3 Position { get; set; }
    public Sprite Sprite { get; set; }
    public SpriteRenderer? SpriteRenderer { get; set; }

    public string Text { get; set; }
    public TextMeshPro? TextMesh { get; set; }
    public float Timer { get; set; }
    public int UsesLimit { get; set; }
    public int UsesRemaining { get; set; }
    public bool AutoPosition { get; set; }
    public int Row { get; set; }
    public int Order { get; set; }

    public static bool Initialized { get; internal set; }

    /// <summary>
    ///     在游戏中创建一个按钮 (Effect)
    /// </summary>
    /// <param name="onClick">点击后按钮的动作（自动判断是否还在冷却）</param>
    /// <param name="onMeetingEnd">会议结束后按钮的动作</param>
    /// <param name="onEffect">
    ///     按钮等待时间结束后的动作（如纵火犯按下浇油按钮后要等待 <paramref name="effectTime" /> 秒,结束后便执行此动作）（此处为null或
    ///     <paramref name="hasEffect" /> 为false则不执行）
    /// </param>
    /// <param name="couldUse">使用按钮的条件</param>
    /// <param name="hasButton">玩家拥有此按钮的条件</param>
    /// <param name="sprite">按钮的图标</param>
    /// <param name="position">按钮的坐标</param>
    /// <param name="hotkey">按钮的热键</param>
    /// <param name="text">按钮的文本</param>
    /// <param name="cooldown">按钮的冷却</param>
    /// <param name="usesLimit">按钮使用次数限制（≤0为无限）</param>
    /// <param name="hotkeyName">热键名称（留空为自动取名,如果无热键则没有名称）</param>
    /// <returns>CustomButton 的实例</returns>
    public static CustomButton Of(string identifier, Action onClick, Action onMeetingEnd, Action onEffect, Func<bool> couldUse,
        Func<bool> hasButton, Sprite sprite, Vector3 position, KeyCode? hotkey, string text, Func<float> cooldown,
        float effectTime, int usesLimit, string hotkeyName = "", int order = -1)
    {
        return new CustomButton(identifier, onClick, onMeetingEnd, onEffect, couldUse, hasButton, sprite, position, hotkey, text,
                true, cooldown, effectTime, usesLimit, hotkeyName)
        { Order = order };
    }

    /// <summary>
    ///     在游戏中创建一个按钮 (Non-effect)
    /// </summary>
    /// <param name="onClick">点击后按钮的动作（自动判断是否还在冷却）</param>
    /// <param name="onMeetingEnd">会议结束后按钮的动作</param>
    /// <param name="couldUse">使用按钮的条件</param>
    /// <param name="hasButton">玩家拥有此按钮的条件</param>
    /// <param name="sprite">按钮的图标</param>
    /// <param name="position">按钮的坐标</param>
    /// <param name="hotkey">按钮的热键</param>
    /// <param name="text">按钮的文本</param>
    /// <param name="cooldown">按钮的冷却</param>
    /// <param name="usesLimit">按钮使用次数限制（≤0为无限）</param>
    /// <param name="hotkeyName">热键名称（留空为自动取名,如果无热键则没有名称）</param>
    /// <returns>CustomButton 的实例</returns>
    public static CustomButton Of(string identifier, Action onClick, Action onMeetingEnd, Func<bool> couldUse, Func<bool> hasButton,
        Sprite sprite, Vector3 position, KeyCode? hotkey, string text, Func<float> cooldown, int usesLimit,
        string hotkeyName = "", int order = -1)
    {
        return new CustomButton(identifier, onClick, onMeetingEnd, () => { }, couldUse, hasButton, sprite, position, hotkey, text,
                false, cooldown, -1f, usesLimit, hotkeyName)
        { Order = order };
    }

    /// <summary>
    ///     在游戏中创建一个按钮 (Non-effect)
    /// </summary>
    /// <param name="row">按钮在hud中显示在第几行（1-2）</param>
    /// <param name="order">按钮在hud中显示顺序（数字越小越靠右，-1为无所谓）</param>
    /// <returns>CustomButton 的实例</returns>
    public static CustomButton Of(string identifier, Action onClick, Action onMeetingEnd, Func<bool> couldUse, Func<bool> hasButton,
        Sprite sprite, int row, KeyCode? hotkey, string text, Func<float> cooldown, int usesLimit,
        string hotkeyName = "", int order = -1)
    {
        return new CustomButton(identifier, onClick, onMeetingEnd, () => { }, couldUse, hasButton, sprite, null, hotkey, text,
            false, cooldown, -1f, usesLimit, hotkeyName)
        {
            Row = row,
            Order = order
        };
    }

    /// <summary>
    ///     在游戏中创建一个按钮 (Effect)
    /// </summary>
    /// <param name="row">按钮在hud中显示在第几行（1-2）</param>
    /// <param name="order">按钮在hud中显示顺序（数字越小越靠右，-1为无所谓）</param>
    public static CustomButton Of(string identifier, Action onClick, Action onMeetingEnd, Action onEffect, Func<bool> couldUse,
        Func<bool>? hasButton, Sprite sprite, int row, KeyCode? hotkey, string text, Func<float> cooldown,
        float effectTime, int usesLimit, string hotkeyName = "", int order = -1)
    {
        return new CustomButton(identifier, onClick, onMeetingEnd, onEffect, couldUse, hasButton, sprite, null, hotkey, text,
            true, cooldown, effectTime, usesLimit, hotkeyName)
        {
            Row = row,
            Order = order
        };
    }

    public void SetActive(bool active)
    {
        if (active)
            ActionButton!.Show();
        else
            ActionButton!.Hide();
    }

    public void ResetCooldown()
    {
        Timer = Cooldown();
    }

    public void ResetEffectTime()
    {
        Timer = EffectTime;
    }

    public void SetCooldown(float cd)
    {
        Cooldown = () => cd;
        ResetCooldown();
    }

    public void SetEffectTime(float et)
    {
        EffectTime = et;
        ResetEffectTime();
    }

    public void Update()
    {
        var hasButton = !GameStates.IsMeeting && PlayerControl.LocalPlayer.IsAlive() &&
                        HasButton.GetInvocationList().All(d => (bool)d.DynamicInvoke()!);
        var isCoolingDown = Timer > 0f;
        var hotkeyText = "";
        if (HotkeyName == "") hotkeyText = Hotkey.HasValue ? Hotkey.Value.ToString() : HotkeyName;

        var buttonText = $"{Text}<size=75%> ({hotkeyText})</size>";

        if (!PlayerControl.LocalPlayer || GameStates.IsMeeting || ExileController.Instance || !hasButton)
        {
            SetActive(false);
            return;
        }

        SetActive(hasButton);
        if (!hasButton) return;
        
        if (PlayerStates.IsShowingMap()) SetActive(false);
        var lp = PlayerControl.LocalPlayer;
        if (isCoolingDown && !lp.inVent && lp.moveable) Timer -= Time.deltaTime;
        ActionButton!.SetCoolDown(Timer, Cooldown());
        ActionButton.OverrideText(buttonText);
        if (UsesLimit > 0)
            ActionButton.SetUsesRemaining(UsesRemaining);
        else
            ActionButton.SetInfiniteUses();

        var desat = Shader.PropertyToID("_Desat");
        var couldUse = CouldUse.GetInvocationList().All(d => (bool)d.DynamicInvoke()!)
            && !GameStates.IsMeeting
            && PlayerControl.LocalPlayer.IsAlive();
        if (hasButton && !isCoolingDown && couldUse)
        {
            SpriteRenderer!.color = TextMesh!.color = Palette.EnabledColor;
            Material!.SetFloat(desat, 0f);
        }
        else
        {
            SpriteRenderer!.color = TextMesh!.color = Palette.DisabledClear;
            Material!.SetFloat(desat, 1f);
        }

        if (Hotkey.HasValue && Input.GetKeyDown(Hotkey.Value)) CheckClick();

        if (!AutoPosition && Hud!.UseButton)
            GameObject!.transform.localPosition = Hud.UseButton.transform.localPosition + Position;
    }

    public void OnMeetingEndSpawn()
    {
        OnMeetingEnd();
    }


#nullable disable
    private void CheckClick()
    {
        if (Timer <= 0f && CouldUse() && PlayerControl.LocalPlayer.IsAlive())
        {
            if (HasEffect && IsEffectActive)
            {
                IsEffectActive = false;
                if (ActionButton != null) ActionButton.cooldownTimerText.color = Palette.EnabledColor;
                Debug.Assert(OnEffect != null, $"{nameof(OnEffect)} != null");
                ResetCooldown();
                OnEffect();
            }
            else
            {
                if (UsesRemaining <= 0 && UsesLimit > 0) return;

                ResetCooldown();
                OnClick();

                if (HasEffect && !IsEffectActive)
                {
                    IsEffectActive = true;
                    ResetEffectTime();
                }

                if (UsesLimit > 0) UsesRemaining--;
                if (UsesLimit <= 0 || UsesRemaining > 0) ResetCooldown();
            }
        }
    }

#nullable enable

    public override bool Equals(object? obj)
    {
        if (obj is not CustomButton btn) return false;
        return btn.Id == Id;
    }

    public static void ResetAllCooldown()
    {
        CustomButtonManager.GetManager().GetButtons().ForEach(b => b.ResetCooldown());
    }
    internal static void Init(HudManager hud)
    {
        var moddedParent = new GameObject("ModdedBottomRight");
        var vanillaParent = hud.AbilityButton.transform.parent;
        moddedParent.transform.SetParent(vanillaParent.parent);
        moddedParent.transform.localPosition = vanillaParent.localPosition;

        foreach (var button in CustomButtonManager.GetManager().GetButtons())
        {
            button.ActionButton = Object.Instantiate(hud!.AbilityButton, moddedParent.transform);

            button.Hud = hud;
            button.SpriteRenderer = button.ActionButton.graphic;
            button.SpriteRenderer.sprite = button.Identifier == "general-kill" ? hud.KillButton.graphic.sprite : button.Sprite;

            if (button.UsesLimit > 0)
                button.ActionButton.SetUsesRemaining(button.UsesLimit);
            else
                button.ActionButton.SetInfiniteUses();

            button.ResetCooldown();

            button.Material = button.SpriteRenderer.material;
            button.GameObject = button.ActionButton.gameObject;
            button.GameObject.name = ModdedFlag + button.Text;
            button.PassiveButton = button.ActionButton.GetComponent<PassiveButton>();
            button.TextMesh = button.ActionButton.buttonLabelText;
            button.TextMesh.text = button.Text;
            var tm = button.TextMesh;
            tm.fontSizeMax = tm.fontSizeMin = tm.fontSize;
            button.PassiveButton.OnClick = new Button.ButtonClickedEvent();

            button.PassiveButton.OnClick.AddListener((UnityAction)button.CheckClick);
            button.SetActive(false);
        }

        Initialized = true;
    }

    internal static void ArrangePosition()
    {
        if (GameStates.IsMeeting || PlayerStates.IsShowingMap()) return;

        bool exit = false;
        
        var vectors = GridArrange.currentChildren.ToArray().Select(b =>
        {
            var pos = b.transform.localPosition;
            var (x, y, z) = ((int)pos.x, (int)pos.y, (int)pos.z);
            return (x, y, z);
        }).ToList();
        
        if (exit) return;
        
        var idx1 = 0;
        var idx2 = 0;
        foreach (var btn in CustomButtonManager.GetManager().GetButtons()
                     .Where(b => b.ActionButton!.isActiveAndEnabled && b.AutoPosition).OrderBy(b => b.Order))
        {
            btn.Row = Mathf.Clamp(btn.Row, 1, 2); // 限制按钮的行数
            var y = 2 - btn.Row; // 对应y坐标（第一行 => y = 1; 第二行 => y = 0）
            var now = 1; // 确定每行会有几个自定义按钮
            if (y == 1) now = ++idx1;
            if (y == 0) now = ++idx2;
            var rowBtnPos = vectors.Where(p => p.y == y).OrderBy(p => p.x).ToList(); // 按序排列与当前按钮在同一行的原版按钮
            
            float x = 0;

            if (rowBtnPos.Any())
                x = rowBtnPos.FirstOrDefault().x; // 如果数量不为0则取最左侧按钮的x坐标

            var pos = new Vector3(x - now, y, -9); // 按钮的x坐标为最靠左的原版按钮x坐标减去当前按钮在同一行自定义按钮的Index
            btn.GameObject!.transform.localPosition = btn.Position = pos;

            //if (Input.GetKeyDown(KeyCode.F1))
            //    Main.Logger.LogInfo($"""
            //    idx: {idx1} {idx2}
            //    row: {btn.Row}
            //    y: {y}
            //    now: {now}
            //    vectors: {vectors.AsString()}
            //    rowBtnPos: {rowBtnPos.AsString()}
            //    x: {x}
            //    pos: {pos}
            //    """);
        }
    }
}

#pragma warning restore CS0659