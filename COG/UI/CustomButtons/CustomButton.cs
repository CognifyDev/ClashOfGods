using Sentry.Internal;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace COG.UI.CustomButtons
{
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
        public bool IsEffectActive = false;
        public float Cooldown;
        public float EffectTime;

        public HudManager Hud;

        /// <summary>
        /// 在游戏中创建一个按钮
        /// </summary>
        /// <param name="onClick">点击后按钮的动作</param>
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
        public CustomButton(Action onClick, Action onMeetingEnd, Action? onEffect, Func<bool> couldUse, Func<bool> hasButton, Sprite sprite, Vector3 position, KeyCode? hotkey, string text, bool hasEffect, float cooldown, float effectTime, HudManager hud)
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
        }

        public void SetActive(bool active)
        {
            this.GameObject.SetActive(active);
            this.SpriteRenderer.enabled = active;
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
            if (!PlayerControl.LocalPlayer || MeetingHud.Instance || ExileController.Instance || !this.HasButton())
            {
                this.SetActive(false);
                return;
            }
            this.SetActive(Hud.UseButton.isActiveAndEnabled || Hud.PetButton.isActiveAndEnabled);

            var lp = PlayerControl.LocalPlayer;
            if (Timer > 0f && !lp.inVent && lp.moveable) Timer -= Time.deltaTime;

            this.ActionButton.cooldownTimerText.text = $"{Timer:00}";
            this.ActionButton.OverrideText(this.Text);
            this.SpriteRenderer.color = this.TextMesh.color = Palette.DisabledClear;
            this.Material.SetFloat(Shader.PropertyToID("_Desat"), this.CouldUse() ? 0f : 1f);

            if (Timer <= 0f && this.HasEffect && this.IsEffectActive)
            {
                this.IsEffectActive = false;
                this.ActionButton.cooldownTimerText.color = Palette.EnabledColor;
                this.OnEffect();
            }

            if (Hud.UseButton != null)
            {
                var pos = Hud.UseButton.transform.localPosition;
                this.ActionButton.transform.localPosition = pos + this.Position;
            }

            if (Hotkey.HasValue && Input.GetKeyDown(Hotkey.Value)) this.OnClick();
        }

        // Static methods
        public static void ResetAllCooldown()
        {
            foreach (var button in CustomButtonManager.GetManager().GetButtons())
                if (button != null) button.ResetCooldown();
        }

        public static void Init(HudManager hud)
        {
            foreach(var button in CustomButtonManager.GetManager().GetButtons())
            {
                if (button == null) continue;
                button.ActionButton = GameObject.Instantiate(hud.AbilityButton);
                button.SpriteRenderer = button.ActionButton.graphic;
                button.SpriteRenderer.sprite = button.Sprite;
                button.Material = button.SpriteRenderer.material;
                button.GameObject = button.ActionButton.gameObject;
                button.PassiveButton = button.ActionButton.GetComponent<PassiveButton>();
                button.TextMesh = button.ActionButton.buttonLabelText;
                button.TextMesh.text = button.Text;
                button.PassiveButton.OnClick = new();
                button.PassiveButton.OnClick.AddListener((UnityEngine.Events.UnityAction)(() =>
                {
                    button.OnClick();
                    if (button.HasEffect) button.IsEffectActive = true;
                }));
            }
        }
    }
}
