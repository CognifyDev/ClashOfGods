using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using COG.Config.Impl;
using COG.Listener.Event.Impl.Game;
using COG.Role;
using COG.UI.CustomButton;
using COG.UI.ClientOption;
using COG.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace COG.Listener.Impl;

internal class ClientOptionListener : IListener
{
    private const float TimerCountdown = 5f;

    private static bool _isSettingHotkey = false;
    private static ClientOption? _currentBeingSet = null;
    private static int _currentIndex = -1;
    private static float _timer = TimerCountdown;
    private static TabGroup? _modTabButton;

    public static List<GameObject> HotkeyButtons { get; } = new();
    public static GameObject? ModTabContainer { get; set; }

    public const string ModTabName = "ModTab";


    [EventHandler(EventHandlerType.Prefix)]
    public void OnSettingInit(OptionsMenuBehaviourStartEvent @event)
    {
        Main.Logger.LogInfo("Start initting mod options...");

        var menu = @event.Object;
        _modTabButton = Object.Instantiate(menu.Tabs[1], menu.Tabs[1].transform.parent);

        ModTabContainer = new(ModTabName);
        ModTabContainer.transform.SetParent(Camera.main.transform);
        ModTabContainer.transform.localPosition = new(0,0, -910);
        ModTabContainer.layer = LayerMask.NameToLayer("UI");

        _modTabButton.name = "ClientOptionButton";
        _modTabButton.transform.localPosition += Vector3.right;

        menu.Tabs = menu.Tabs.AddItem(_modTabButton).ToArray();
        _modTabButton.Content = ModTabContainer;

        _modTabButton.GetComponent<PassiveButton>().OnClick = new();
        _modTabButton.GetComponent<PassiveButton>().OnClick.AddListener(new Action(() => 
        { 
            menu.OpenTabGroup(GetModTabIndex());

            int GetModTabIndex()
            {
                for (var i = 0; i < menu.Tabs.Length; i++)
                {
                    if (menu.Tabs[i].GetInstanceID() == _modTabButton!.GetInstanceID()) return i;
                }
                Main.Logger.LogWarning("Modded tab button not found!");
                return -1;
            }

            foreach (var btn in ClientOption.Buttons)
                if (btn.ToggleButton)
                    btn.ToggleButton!.gameObject.SetActive(true);
        }));

        LoadNormalButtons(menu);
        LoadHotkeyButtons(menu);
        AdjustTabButtons();

        void AdjustTabButtons()
        {
            var tabs = menu.Tabs.ToList();
            var generalButton = tabs.First(t => t.name == "GeneralButton");
            var graphicsButton = tabs.First(t => t.name == "GraphicsButton");
            var dataButton = tabs.FirstOrDefault(t => t.name == "DataButton");

            var offset = new Vector3(1.6f, 0, 0);
            if (!Object.FindObjectOfType<MainMenuManager>())
            {
                var initial = new Vector3(-1.6f, 2.4f, -1);
                generalButton.transform.localPosition = initial;
                graphicsButton.transform.localPosition = initial + offset;
                _modTabButton!.transform.localPosition = initial + offset * 2;
            }
            else
            {
                var initial = new Vector3(-2.4f, 0, -1);
                generalButton.transform.localPosition = initial;
                graphicsButton.transform.localPosition = initial + offset;
                dataButton!.transform.localPosition = initial + offset * 2;
                _modTabButton!.transform.localPosition = initial + offset * 3;
            }
        }
    }

    [EventHandler(EventHandlerType.Postfix)]
    public void OnSettingUpdate(OptionsMenuBehaviourUpdateEvent @event)
    {
        _modTabButton!.GetComponentInChildren<TextMeshPro>().text = LanguageConfig.Instance.CogOptions;

        if (_isSettingHotkey)
        {
            var textMesh = _currentBeingSet!.ToggleButton!.Text;

            _timer -= Time.deltaTime;
            if (_timer <= 0)
            {
                ResetHotkeyState();
                return;
            }

            textMesh.text = LanguageConfig.Instance.PressKeyToSet.CustomFormat(Mathf.CeilToInt(_timer));

            foreach (var keyCode in Enum.GetValues<KeyCode>())
            {
                if (Input.GetKeyDown(keyCode) && !keyCode.ToString().StartsWith("Mouse"))
                {
                    ButtonHotkeyConfig.Instance.SetHotkey(_currentIndex, keyCode);
                    ResetHotkeyState();
                }
            }
        }
        else
        {
            var index = 1;
            foreach (var button in HotkeyButtons)
            {
                bool succeeded = ButtonHotkeyConfig.Instance.GetHotkeys().TryGetValue(index, out var keyCode);
                button.GetComponent<ToggleButtonBehaviour>().Text.text = $"{LanguageConfig.Instance.GetHandler("option.hotkey").GetString("button").CustomFormat(index)}{(succeeded ? $": {keyCode}" : "")}";
                index++;
            }
        }
    }

    public static void ResetHotkeyState()
    {
        _isSettingHotkey = false;
        _currentBeingSet = null;
        _currentIndex = -1;
        _timer = TimerCountdown;
    }

    private void LoadNormalButtons(OptionsMenuBehaviour menu)
    {
        Main.Logger.LogInfo("Initting normal buttons");

        ClientOption.Buttons.Clear();
        foreach (var clientOption in ClientOptionManager.GetManager().GetOptions()) clientOption.Register();
        var a = 0;
        foreach (var btn in ClientOption.Buttons)
            CreateButton(menu, a++, btn);
    }

    private void LoadHotkeyButtons(OptionsMenuBehaviour menu)
    {
        HotkeyButtons.Clear();
        var a = 0;
        var buttons = new List<ClientOption>();

        for (var i = 1; i < ButtonHotkeyConfig.MaxButtonCount + 1; i++)
        {
            ClientOption clientOption = default!;

            var buttonString = LanguageConfig.Instance.GetHandler("option.hotkey").GetString("button").CustomFormat(i);
            var capturedIndex = i; // Capture the current index for the lambda 

            clientOption = new ClientOption(buttonString,
                () =>
                {
                    if (!_isSettingHotkey)
                    {
                        _currentBeingSet = clientOption;
                        _currentIndex = capturedIndex;
                        _isSettingHotkey = true;
                        _timer = TimerCountdown;
                    }

                    return false;
                }, false);

            CreateButton(menu, a++, clientOption);
            HotkeyButtons.Add(clientOption.ToggleButton!.gameObject);
        }
    }

    /// <summary>
    ///     创建一个按钮
    /// </summary>
    /// <param name="menu">OptionMenuBehaviour 的实例</param>
    /// <param name="idx">（从0开始）加入按钮的序号</param>
    /// <param name="option">对应的客户端选项</param>
    private void CreateButton(OptionsMenuBehaviour menu, int idx, ClientOption option)
    {
        var template = menu.CensorChatButton;
        var button = Object.Instantiate(template, ModTabContainer!.transform);
        Vector3 pos = new(idx % 2 == 0 ? -1.3f : 1.3f, 1.7f - idx / 2 * 0.5f, -0.5f);

        button.transform.localPosition = pos;
        button.transform.localScale = Vector3.one;
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