using System;
using System.Collections.Generic;
using System.Linq;
using COG.Config.Impl;
using COG.Listener.Event.Impl.Game;
using COG.UI.ClientOption;
using COG.UI.ClientOption.Impl;
using COG.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace COG.Listener.Impl;

internal class ClientOptionListener : IListener
{
    private const float TimerCountdown = 5f;

    public const string ModTabName = "ModTab";

    private static bool _isSettingHotkey;
    private static ToggleClientOption? _currentBeingSet;
    private static int _currentIndex = -1;
    private static float _timer = TimerCountdown;
    private static TabGroup? _modTabButton;

    public static List<GameObject> HotkeyButtons { get; } = [];
    public static GameObject? ModTabContainer { get; set; }


    [EventHandler(EventHandlerType.Prefix)]
    public void OnSettingInit(OptionsMenuBehaviourStartEvent @event)
    {
        Main.Logger.LogInfo("Start initting mod options...");

        var menu = @event.Object;
        _modTabButton = Object.Instantiate(menu.Tabs[1], menu.Tabs[1].transform.parent);

        ModTabContainer = new GameObject(ModTabName);
        ModTabContainer.transform.SetParent(Camera.main.transform);
        ModTabContainer.transform.localPosition = new Vector3(0, 0, -910);
        ModTabContainer.layer = LayerMask.NameToLayer("UI");

        _modTabButton.name = "ClientOptionButton";
        _modTabButton.transform.localPosition += Vector3.right;

        menu.Tabs = menu.Tabs.AddItem(_modTabButton).ToArray();
        _modTabButton.Content = ModTabContainer;

        _modTabButton.GetComponent<PassiveButton>().OnClick = new Button.ButtonClickedEvent();
        _modTabButton.GetComponent<PassiveButton>().OnClick.AddListener(new Action(() =>
        {
            menu.OpenTabGroup(GetModTabIndex());

            int GetModTabIndex()
            {
                for (var i = 0; i < menu.Tabs.Length; i++)
                    if (menu.Tabs[i] == _modTabButton)
                        return i;
                Main.Logger.LogWarning("Modded tab button not found!");
                return -1;
            }

            ClientOptionManager.GetManager().GetOptions().DoIf(option => option.Component,
                option => option.Component!.gameObject.SetActive(true));
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

        var optionHandler = LanguageConfig.Instance.GetHandler("option");
        foreach (var option in ClientOptionManager.GetManager().GetOptions())
            if (option is SliderClientOption sliderOption)
            {
                var trueValue = sliderOption.MinValue +
                                (sliderOption.MaxValue - sliderOption.MinValue) * sliderOption.OptionObject!.Value;
                if (sliderOption.OnUpdateTextChange != null)
                    sliderOption.OptionObject.GetComponentInChildren<TextMeshPro>().text =
                        sliderOption.OnUpdateTextChange(trueValue, optionHandler.GetString(sliderOption.Translatable));
            }
            else if (option is ToggleClientOption toggleOption)
            {
                if (toggleOption.OnUpdateTextChange != null)
                    toggleOption.OptionObject!.GetComponentInChildren<TextMeshPro>().text =
                        toggleOption.OnUpdateTextChange(toggleOption.OptionObject!.onState,
                            optionHandler.GetString(toggleOption.Translatable));
            }

        if (_isSettingHotkey)
        {
            _timer -= Time.deltaTime;

            if (_timer <= 0)
            {
                ResetHotkeyState();
                return;
            }

            _currentBeingSet!.OnChange(false);

            foreach (var keyCode in Enum.GetValues<KeyCode>())
                if (Input.GetKeyDown(keyCode) && !keyCode.ToString().StartsWith("Mouse"))
                {
                    ButtonHotkeyConfig.Instance.SetHotkey(_currentIndex, keyCode);
                    ResetHotkeyState();
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

        var a = 0;
        foreach (var btn in ClientOptionManager.GetManager().GetOptions())
            CreateButton(menu, ref a, btn);
    }

    private void LoadHotkeyButtons(OptionsMenuBehaviour menu)
    {
        HotkeyButtons.Clear();
        var a = 0;
        var buttons = new List<ToggleClientOption>();

        for (var i = 1; i < ButtonHotkeyConfig.MaxButtonCount + 1; i++)
        {
            ToggleClientOption clientOption = default!;

            var capturedIndex = i; // Capture the current index for the lambda 

            clientOption = new ToggleClientOption("hotkey.button",
                false,
                _ =>
                {
                    if (!_isSettingHotkey)
                    {
                        _currentBeingSet = clientOption;
                        _currentIndex = capturedIndex;
                        _isSettingHotkey = true;
                        _timer = TimerCountdown;
                    }

                    return false;
                },
                (newValue, origin) =>
                {
                    if (_isSettingHotkey)
                        return LanguageConfig.Instance.PressKeyToSet.CustomFormat(Mathf.CeilToInt(_timer));
                    return LanguageConfig.Instance.GetHandler("option.hotkey").GetString("button") + capturedIndex +
                           (ButtonHotkeyConfig.Instance.GetHotkeys().TryGetValue(capturedIndex, out var keyCode)
                               ? $": {keyCode}"
                               : "");
                });

            CreateButton(menu, ref a, clientOption);
            HotkeyButtons.Add(clientOption.OptionObject!.gameObject);
        }
    }

    /// <summary>
    ///     创建一个按钮
    /// </summary>
    /// <param name="menu">OptionMenuBehaviour 的实例</param>
    /// <param name="idx">（从0开始）加入按钮的序号</param>
    /// <param name="option">对应的客户端选项</param>
    private void CreateButton(OptionsMenuBehaviour menu, ref int idx, IClientOption option)
    {
        var handler = LanguageConfig.Instance.GetHandler("option");
        if (option is ToggleClientOption toggleOption)
        {
            var template = menu.CensorChatButton;
            var button = Object.Instantiate(template, ModTabContainer!.transform);
            Vector3 pos = new(idx % 2 == 0 ? -1.3f : 1.3f, 1.7f - idx / 2 * 0.5f, -0.5f);

            button.transform.localPosition = pos;
            button.transform.localScale = Vector3.one;
            button.onState = toggleOption.DefaultValue;
            button.Background.color = button.onState ? Palette.AcceptedGreen : Palette.EnabledColor;
            button.name = button.Text.text = handler.GetString(toggleOption.Translatable);
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
                button.onState = toggleOption.OnChange(button.onState);
                button.Background.color = button.onState ? Palette.AcceptedGreen : Palette.EnabledColor;
            }));

            toggleOption.OptionObject = button;

            button.gameObject.SetActive(false);

            idx++;
        }
        else if (option is SliderClientOption sliderOption)
        {
            if (idx % 2 == 1) idx++;

            var template = menu.MusicSlider;
            var slider = Object.Instantiate(template, ModTabContainer!.transform);
            Vector3 pos = new(-2, 1.7f - idx / 2 * 0.5f, -0.5f);

            var valueRange = sliderOption.MaxValue - sliderOption.MinValue;

            slider.transform.localPosition = pos;
            slider.transform.localScale = Vector3.one;
            var text = slider.GetComponentInChildren<TextMeshPro>();
            text.TryDestroyComponent<TextTranslatorTMP>();
            slider.name = text.text = handler.GetString(sliderOption.Translatable);
            slider.OnValueChange = new UnityEvent();
            slider.OnValueChange.AddListener(new Action(() =>
            {
                var trueValue = ComputeActualValue(slider.Value);
                slider.SetValue(ComputePercent(sliderOption.OnChange(trueValue)));
                sliderOption.CurrentValue = trueValue;
            }));
            slider.SetValue(ComputePercent(sliderOption.DefaultValue));

            slider.gameObject.SetActive(false);
            sliderOption.OptionObject = slider;

            idx += 2; // Next line

            float ComputePercent(float actual)
            {
                return (actual - sliderOption.MinValue) / valueRange;
            }

            float ComputeActualValue(float percent)
            {
                return sliderOption.MinValue + valueRange * percent;
            }
        }
    }
}