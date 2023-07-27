using System;
using UnityEngine;
using UnityEngine.Events;
using Object = UnityEngine.Object;

namespace COG.UI.CustomButtons;

public class CustomButton
{
    public ActionButton ActionButton;
    public SpriteRenderer SpriteRenderer;
    public PassiveButton PassiveButton;
    public TMPro.TextMeshPro TextMesh;
    public Material Material;
    public GameObject GameObject;
    public Action OnClick;
    public Action OnMeetingEnd;
    public Action? OnEffect;
    public Func<bool> CouldUse;
    public Func<bool> HasButton;
    public Sprite Sprite;
    public Vector3 Position;
    public KeyCode? Hotkey;

    public string Text;
    public float Timer;
    public bool HasEffect;
    public bool IsEffectActive;
    public float Cooldown;
    public float EffectTime;
    public int UsesLimit;
    public int UsesRemaining;
    public string HotkeyName;

    public HudManager Hud;
        
    /// <summary>
    /// 在游戏中创建一个按钮
    /// </summary>
    /// <param name="onClick">点击后按钮的动作（自动判断是否还在冷却）</param>
    /// <param name="onMeetingEnd">会议结束后按钮的动作</param>
    /// <param name="onEffect">按钮等待时间结束后的动作（如纵火犯按下浇油按钮后要等待 <paramref name="effectTime"/> 秒，结束后便执行此动作）（此处为null或 <paramref name="hasEffect"/> 为false则不执行）</param>
    /// <param name="couldUse">使用按钮的条件</param>
    /// <param name="hasButton">玩家拥有此按钮的条件</param>
    /// <param name="sprite">按钮的图标</param>
    /// <param name="position">按钮的坐标</param>
    /// <param name="hotkey">按钮的热键</param>
    /// <param name="text">按钮的文本</param>
    /// <param name="cooldown">按钮的冷却</param>
    /// <param name="hud">HudManager 的实例</param>
    /// <param name="usesLimit">按钮使用次数限制（≤0为无限）</param>
    /// <param name="hotkeyName">热键名称（留空为自动取名，如果无热键则没有名称）</param>
    public CustomButton(Action onClick, Action onMeetingEnd, Action? onEffect, Func<bool> couldUse, Func<bool> hasButton, Sprite sprite, Vector3 position, KeyCode? hotkey, string text, bool hasEffect, float cooldown, float effectTime, HudManager hud, int usesLimit, string hotkeyName = "")
    {
        OnClick = onClick;
        OnMeetingEnd = onMeetingEnd;
        OnEffect = onEffect;
        CouldUse = couldUse;
        HasButton = hasButton;
        Sprite = sprite;
        Position = position;
        Hotkey = hotkey;
        Text = text;
        HasEffect = hasEffect;
        Cooldown = cooldown;
        EffectTime = effectTime;
        Hud = hud;
        UsesLimit = UsesRemaining = usesLimit;
        HotkeyName = hotkeyName;
    }

    // Position from The Other Roles
    // Link: https://github.com/TheOtherRolesAU/TheOtherRoles/blob/main/TheOtherRoles/Objects/CustomButton.cs#L40
    public static class ButtonPositions
    {
        public static readonly Vector3 LowerRowRight = new(-2f, -0.06f, 0);  // Not usable for imps beacuse of new button positions!
        public static readonly Vector3 LowerRowCenter = new(-3f, -0.06f, 0);
        public static readonly Vector3 LowerRowLeft = new(-4f, -0.06f, 0);
        public static readonly Vector3 UpperRowRight = new(0f, 1f, 0f);  // Not usable for imps beacuse of new button positions!
        public static readonly Vector3 UpperRowCenter = new(-1f, 1f, 0f);  // Not usable for imps beacuse of new button positions!
        public static readonly Vector3 UpperRowLeft = new(-2f, 1f, 0f);
        public static readonly Vector3 UpperRowFarLeft = new(-3f, 1f, 0f);
    }

    public void SetActive(bool active)
    {
        if (active)
            this.ActionButton.Show();
        else
            this.ActionButton.Hide();
    }

    public void ResetCooldown()
    {
        this.Timer = this.Cooldown;
    }

    public void ResetEffectTime()
    {
        this.Timer = this.EffectTime;
    }

    public void SetCooldown(float cd)
    {
        this.Cooldown = cd;
        this.ResetCooldown();
    }

    public void SetEffectTime(float et)
    {
        this.EffectTime = et;
        this.ResetEffectTime();
    }

    public void Update()
    {
        bool isCoolingDown = Timer > 0f;
        string hotkeyText = "";
        if (HotkeyName == "")
            if (Hotkey.HasValue)
                hotkeyText = Hotkey.Value.ToString();
            else
                hotkeyText = HotkeyName;

        string buttonText = $"{Text}<size=75%> ({hotkeyText})</size>";

        if (!PlayerControl.LocalPlayer || MeetingHud.Instance || ExileController.Instance || !HasButton())
        {
            this.SetActive(false);
            return;
        }

        this.SetActive(HasButton());
        var lp = PlayerControl.LocalPlayer;
        if (isCoolingDown && !lp.inVent && lp.moveable) Timer -= Time.deltaTime;
        this.ActionButton.SetCoolDown(Timer, Cooldown);
        this.ActionButton.OverrideText(buttonText);
        this.ActionButton.SetUsesRemaining(UsesRemaining);

        int desat = Shader.PropertyToID("_Desat");
        if (CouldUse() && !isCoolingDown)
        {
            this.SpriteRenderer.color = this.TextMesh.color = Palette.EnabledColor;
            this.Material.SetFloat(desat, 0f);
        }
        else
        {
            this.SpriteRenderer.color = this.TextMesh.color = Palette.DisabledClear;
            this.Material.SetFloat(desat, 1f);
        }

        if (Hud.UseButton != null) this.GameObject.transform.localPosition = Hud.UseButton.transform.localPosition + this.Position;
            
        if (Hotkey.HasValue && Input.GetKeyDown(Hotkey.Value)) this.CheckClick();
    }

    public void OnMeetingEndSpawn() => this.OnMeetingEnd();
        

    #nullable disable
    public void CheckClick()
    {
        var button = this;
        if (button.Timer <= 0f)
        {
            if (button.HasEffect && button.IsEffectActive)
            {
                button.IsEffectActive = false;
                button.ActionButton.cooldownTimerText.color = Palette.EnabledColor;
                if (button.OnEffect != null) button.OnEffect();
                button.ResetCooldown();
            }
            else
            {
                if (button.UsesRemaining <= 0) return;
                button.OnClick();
                if (button.HasEffect && !button.IsEffectActive)
                {
                    button.IsEffectActive = true;
                    button.ResetEffectTime();
                }
                if (button.UsesLimit > 0) button.UsesRemaining--;
            }
        }
    }

    #nullable enable
    // Static methods
    public static void ResetAllCooldown()
    {
        foreach (var button in CustomButtonManager.GetManager().GetButtons())
            button.ResetCooldown();
    }

    public static void Init(HudManager hud)
    {
        foreach(var button in CustomButtonManager.GetManager().GetButtons())
        {
            button.ActionButton = Object.Instantiate(hud.AbilityButton, hud.AbilityButton.transform.parent);
                
            button.SpriteRenderer = button.ActionButton.graphic;
            button.SpriteRenderer.sprite = button.Sprite;

            if (button.UsesLimit > 0)
                button.ActionButton.SetUsesRemaining(button.UsesLimit);
            else
                button.ActionButton.SetInfiniteUses();

            button.ResetCooldown();

            button.Material = button.SpriteRenderer.material;
            button.GameObject = button.ActionButton.gameObject;
            button.PassiveButton = button.ActionButton.GetComponent<PassiveButton>();
            button.TextMesh = button.ActionButton.buttonLabelText;
            button.TextMesh.text = button.Text;
            button.PassiveButton.OnClick = new();

            void Action()
            {
                button.CheckClick();
            }

            button.PassiveButton.OnClick.AddListener((UnityAction) (Action?)Action);
            button.SetActive(false);
        }
    }
}