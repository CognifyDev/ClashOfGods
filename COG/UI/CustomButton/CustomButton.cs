using System;
using System.Collections.Generic;
using System.Linq;
using COG.States;
using COG.Utils;
using COG.Utils.Coding;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Debug = System.Diagnostics.Debug;

namespace COG.UI.CustomButton;

[ShitCode]
public class CustomButton
{
    internal static List<ActionButton> AllVanillaButtons = new();

    private CustomButton(Action onClick, Action onMeetingEnd, Action onEffect, Func<bool> couldUse,
        Func<bool> hasButton,
        Sprite sprite, Vector3? position, KeyCode? hotkey, string text, bool hasEffect, Func<float> cooldown,
        float effectTime,
        int usesLimit, string hotkeyName)
    {
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
    }

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
    public static CustomButton Create(Action onClick, Action onMeetingEnd, Action onEffect, Func<bool> couldUse,
        Func<bool>? hasButton, Sprite sprite, Vector3 position, KeyCode? hotkey, string text, Func<float> cooldown,
        float effectTime, int usesLimit, string hotkeyName = "", int order = -1)
    {
        return new CustomButton(onClick, onMeetingEnd, onEffect, couldUse, hasButton, sprite, position, hotkey, text,
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
    public static CustomButton Create(Action onClick, Action onMeetingEnd, Func<bool> couldUse, Func<bool> hasButton,
        Sprite sprite, Vector3 position, KeyCode? hotkey, string text, Func<float> cooldown, int usesLimit,
        string hotkeyName = "", int order = -1)
    {
        return new CustomButton(onClick, onMeetingEnd, () => { }, couldUse, hasButton, sprite, position, hotkey, text,
                false, cooldown, -1f, usesLimit, hotkeyName)
            { Order = order };
    }

    /// <summary>
    ///     在游戏中创建一个按钮 (Non-effect)
    /// </summary>
    /// <param name="row">按钮在hud中显示在第几行（1-2）</param>
    /// <param name="order">按钮在hud中显示顺序（数字越小越靠右，-1为无所谓）</param>
    /// <returns>CustomButton 的实例</returns>
    public static CustomButton Create(Action onClick, Action onMeetingEnd, Func<bool> couldUse, Func<bool> hasButton,
        Sprite sprite, int row, KeyCode? hotkey, string text, Func<float> cooldown, int usesLimit,
        string hotkeyName = "", int order = -1)
    {
        return new CustomButton(onClick, onMeetingEnd, () => { }, couldUse, hasButton, sprite, null, hotkey, text,
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
    public static CustomButton Create(Action onClick, Action onMeetingEnd, Action onEffect, Func<bool> couldUse,
        Func<bool>? hasButton, Sprite sprite, int row, KeyCode? hotkey, string text, Func<float> cooldown,
        float effectTime, int usesLimit, string hotkeyName = "", int order = -1)
    {
        return new CustomButton(onClick, onMeetingEnd, onEffect, couldUse, hasButton, sprite, null, hotkey, text,
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

        if (!PlayerControl.LocalPlayer || GameStates.IsMeeting || ExileController.Instance || !hasButton ||
            PlayerStates.IsShowingMap())
        {
            SetActive(false);
            return;
        }

        SetActive(hasButton);
        if (!hasButton) return;
        var lp = PlayerControl.LocalPlayer;
        if (isCoolingDown && !lp.inVent && lp.moveable) Timer -= Time.deltaTime;
        ActionButton!.SetCoolDown(Timer, Cooldown());
        ActionButton.OverrideText(buttonText);
        if (UsesLimit > 0)
            ActionButton.SetUsesRemaining(UsesRemaining);
        else
            ActionButton.SetInfiniteUses();

        var desat = Shader.PropertyToID("_Desat");
        var couldUse = CouldUse.GetInvocationList().All(d => (bool)d.DynamicInvoke()!) && !GameStates.IsMeeting &&
                       PlayerControl.LocalPlayer.IsAlive();
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
    // Static methods
    public static void ResetAllCooldown()
    {
        CustomButtonManager.GetManager().GetButtons().ForEach(b => b.ResetCooldown());
    }

    public static void SetAllActive(bool active)
    {
        CustomButtonManager.GetManager().GetButtons().ForEach(b => b.SetActive(active));
    }

    internal static void Init(HudManager? hud)
    {
        foreach (var button in CustomButtonManager.GetManager().GetButtons())
        {
            button.ActionButton = Object.Instantiate(hud!.AbilityButton, hud.AbilityButton.transform.parent);

            button.Hud = hud;
            button.SpriteRenderer = button.ActionButton.graphic;
            button.SpriteRenderer.sprite = button.Sprite;

            if (button.UsesLimit > 0)
                button.ActionButton.SetUsesRemaining(button.UsesLimit);
            else
                button.ActionButton.SetInfiniteUses();

            button.ResetCooldown();

            button.Material = button.SpriteRenderer.material;
            button.GameObject = button.ActionButton.gameObject;
            button.GameObject.name = button.Text;
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

    internal static void GetAllVanillaButtons()
    {
        var buttons = GameObject.Find("Main Camera/Hud/Buttons/BottomRight/");
        if (!buttons) return;
        AllVanillaButtons.Clear();
        buttons.ForEachChild(new Action<GameObject>(gameObject =>
        {
            if (!CustomButtonManager.GetManager().GetButtons()
                    .Any(b => b.GameObject!.name == gameObject.name))
                AllVanillaButtons.Add(gameObject.GetComponent<ActionButton>());
        }));
    }

    internal static void ArrangePosition()
    {
        if (GameStates.IsMeeting || PlayerStates.IsShowingMap()) return;
        var vectors = AllVanillaButtons.Where(b => b.isActiveAndEnabled).Select(b => b.transform.localPosition);
        var idx1 = 0;
        var idx2 = 0;
        foreach (var btn in CustomButtonManager.GetManager().GetButtons()
                     .Where(b => b.ActionButton!.isActiveAndEnabled && b.AutoPosition).OrderBy(b => b.Order))
        {
            var row = btn.Row;
            if (row != 1 && row != 2) btn.Row = 2;
            var y = 2 - btn.Row;
            var now = 1;
            if (y == 1) now = ++idx1;
            if (y == 0) now = ++idx2;
            var rowBtnPos = vectors.Where(p => p.y == y).OrderBy(p => p.x).ToList();

            float x = 0, z = 0;

            if (rowBtnPos.Count != 0)
            {
                x = rowBtnPos.FirstOrDefault().x;
                z = rowBtnPos.FirstOrDefault().z;
            }

            var pos = new Vector3(x - now, y, z);
            btn.GameObject!.transform.localPosition = btn.Position = pos;
        }
    }
}