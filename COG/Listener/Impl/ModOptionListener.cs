using System;
using System.Collections;
using System.Collections.Generic;
using COG.Config.Impl;
using COG.Listener.Event.Impl.Game;
using COG.Role;
using COG.UI.CustomButton;
using COG.UI.ModOption;
using COG.Utils;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace COG.Listener.Impl;

internal class ModOptionListener : IListener
{
    private static readonly List<Transform> _vanillaObjects = new();

    private const float TimerCountdown = 5f;

    private static bool _isSettingHotkey = false;
    private static ModOption? _currentBeingSet = null;
    private static int _currentIndex = -1;
    private static float _timer = TimerCountdown;
    private static string _namePrefix = "";

    public static List<GameObject> HotkeyButtons { get; } = new();


    [EventHandler(EventHandlerType.Postfix)]
    public void OnSettingInit(OptionsMenuBehaviourStartEvent @event)
    {
        var menu = @event.Object;
        var transform1 = menu.CensorChatButton.transform;
        Vector3? position = transform1.localPosition;
        var button = menu.EnableFriendInvitesButton;

        var modOptions = Object.Instantiate(menu.CensorChatButton, transform1.parent);

        //设置原版按钮的大小/位置
        menu.CensorChatButton.Text.transform.localScale = new Vector3(1 / 0.66f, 1, 1);
        transform1.localPosition = position.Value + Vector3.left * 0.45f;
        transform1.localScale = new Vector3(0.66f, 1, 1);

        var transform = button.transform;
        transform.localScale = new Vector3(0.66f, 1, 1);
        transform.localPosition += Vector3.right * 0.5f;
        button.Text.transform.localScale = new Vector3(1.2f, 1, 1);

        //设置模组选项按钮
        modOptions.gameObject.SetActive(true);
        modOptions.Text.text = LanguageConfig.Instance.CogOptions;
        var transform2 = modOptions.transform;
        transform2.localPosition = position.Value + Vector3.right * 4f / 3f;
        transform2.localScale = new Vector3(0.66f, 1, 1);
        modOptions.Text.transform.localScale = new Vector3(1 / 0.66f, 1, 1);
        modOptions.Background.color = Palette.EnabledColor;

        var modOptionsButton = modOptions.GetComponent<PassiveButton>();
        modOptionsButton.OnClick = new Button.ButtonClickedEvent();
        modOptionsButton.OnClick.AddListener((Action)(() =>
        {
            LoadNormalButtons(menu);
            LoadHotkeyButtons(menu);
            HideVanillaButtons(menu);
            foreach (var btn in ModOption.Buttons)
                if (btn.ToggleButton)
                    btn.ToggleButton!.gameObject.SetActive(true);
        }));
    }

    [EventHandler(EventHandlerType.Postfix)]
    public void OnSettingUpdate(OptionsMenuBehaviourUpdateEvent @event)
    {
        if (_isSettingHotkey)
        {
            var textMesh = _currentBeingSet!.ToggleButton!.Text;
            var origin = textMesh.text;

            _timer -= Time.deltaTime;
            if (_timer <= 0)
            {
                ResetHotkeyState();
                textMesh.text = origin;
                return;
            }

            textMesh.text = LanguageConfig.Instance.PressKeyToSet.CustomFormat(Mathf.CeilToInt(_timer));

            foreach (var keyCode in Enum.GetValues<KeyCode>())
            {
                if (Input.GetKeyDown(keyCode) && !keyCode.ToString().StartsWith("Mouse"))
                {
                    textMesh.text = $"{_namePrefix} : {keyCode}";
                    ButtonHotkeyConfig.Instance.SetHotkey(_currentIndex, keyCode);
                    ResetHotkeyState();
                }
            }
        }
    }

    public static void ResetHotkeyState()
    {
        _isSettingHotkey = false;
        _currentBeingSet = null;
        _currentIndex = -1;
        _timer = TimerCountdown;
        _namePrefix = "";
    }

    private void HideVanillaButtons(OptionsMenuBehaviour menu)
    {
        _vanillaObjects.Clear();
        for (var i = 0; i < menu.transform.childCount; i++)
        {
            var child = menu.transform.GetChild(i);
            if (child.name == "Background" ||
                child.name == "CloseButton" ||
                child.name == "Tint" ||
                child.name == "TabButtons" ||
                child.name == "GeneralButton" ||
                child.name == "GraphicsButton" ||
                !child.gameObject.active) continue;
            _vanillaObjects.Add(child);
            child.gameObject.SetActive(false);
        }
    }

    private void LoadNormalButtons(OptionsMenuBehaviour menu)
    {
        ModOption.Buttons.Clear();
        foreach (var modOption in ModOptionManager.GetManager().GetOptions()) modOption.Register();
        var a = 0;
        foreach (var btn in ModOption.Buttons)
            CreateButton(menu, a++, btn);
    }

    private void LoadHotkeyButtons(OptionsMenuBehaviour menu)
    {
        HotkeyButtons.Clear();
        var a = 0;
        var buttons = new List<ModOption>();

        for (var i = 0; i < ButtonHotkeyConfig.MaxButtonCount; i++)
        {
            ModOption modOption = default!;

            var buttonString = LanguageConfig.Instance.GetHandler("option.hotkey").GetString("button").CustomFormat(i);

            modOption = new ModOption(buttonString,
                () =>
                {
                    if (!_isSettingHotkey)
                    {
                        _currentBeingSet = modOption;
                        _currentIndex = i;
                        _isSettingHotkey = true;
                        _timer = TimerCountdown;
                        _namePrefix = buttonString;
                    }

                    return false;
                }, false);

            CreateButton(menu, a++, modOption);
            HotkeyButtons.Add(modOption.ToggleButton!.gameObject);
        }
    }

    /// <summary>
    ///     创建一个按钮
    /// </summary>
    /// <param name="menu">OptionMenuBehaviour 的实例</param>
    /// <param name="idx">（从0开始）加入按钮的序号</param>
    /// <param name="option">对应的 ModOption</param>
    private void CreateButton(OptionsMenuBehaviour menu, int idx, ModOption option)
    {
        var template = menu.CensorChatButton;
        var button = Object.Instantiate(template, menu.transform);
        Vector3 pos = new(idx % 2 == 0 ? -1.17f : 1.17f, 1.7f - idx / 2 * 0.5f, -0.5f);

        button.transform.localPosition = pos;
        button.onState = option.DefaultValue;
        button.Background.color = button.onState ? Palette.AcceptedGreen : Palette.EnabledColor;
        button.Text.text = option.Text;
        button.name = option.Text.Replace(" ", "");
        button.gameObject.SetActive(true);

        var passive = button.GetComponent<PassiveButton>();
        passive.OnClick = new Button.ButtonClickedEvent();
        passive.OnMouseOut = new UnityEvent();
        passive.OnMouseOver = new UnityEvent();

        passive.OnMouseOut.AddListener((Action)(() =>
            button.Background.color = button.onState ? Palette.AcceptedGreen : Palette.EnabledColor));
        passive.OnMouseOver.AddListener((Action)(() =>
        {
            if (!button.onState) button.Background.color = Palette.AcceptedGreen;
        }));
        passive.OnClick.AddListener((Action)(() =>
        {
            button.onState = option.OnClick();
            button.Background.color = button.onState ? Palette.AcceptedGreen : Palette.EnabledColor;
        }));

        option.ToggleButton = button;

        button.gameObject.SetActive(false);
    }
}