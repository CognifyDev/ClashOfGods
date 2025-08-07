using System;
using System.Collections.Generic;
using System.Text;
using Reactor.Utilities.Attributes;
using TMPro;
using UnityEngine;

namespace COG.UI.Hud.CustomMessage;

#nullable disable
[RegisterInIl2Cpp]
public class CustomHudMessage : MonoBehaviour
{
    private TextMeshPro _messageObject;

    private Dictionary<string, float> _messageTimers = new();

    public CustomHudMessage(IntPtr ptr) : base(ptr)
    {
    }

    public static CustomHudMessage Instance { get; private set; }

    private void Start()
    {
        Instance = this;

        var template = HudManager.Instance.TaskPanel.taskText;
        _messageObject = Instantiate(template, HudManager.Instance.transform);
        _messageObject.name = nameof(CustomHudMessage);
        _messageObject.transform.localPosition = new Vector3(0, -2, 0);

        _messageObject.fontSize = _messageObject.fontSizeMin = _messageObject.fontSizeMax = 3;
        _messageObject.alignment = TextAlignmentOptions.Center;

        _messageObject.text = "";
    }

    private void Update()
    {
        if (_messageTimers.Count == 0)
        {
            _messageObject.text = "";
            return;
        }

        var modified = new Dictionary<string, float>(_messageTimers);
        var builder = new StringBuilder();

        foreach (var (msg, timer) in _messageTimers)
        {
            if (timer <= 0)
            {
                modified.Remove(msg);
                continue;
            }

            modified[msg] -= Time.deltaTime;
            builder.AppendLine(msg);
        }

        _messageTimers = modified;
        _messageObject.text = builder.ToString();
    }

    public void AddMessage(string message, float duration = 5f)
    {
        if (string.IsNullOrEmpty(message)) return;
        if (_messageTimers.ContainsKey(message))
            _messageTimers[message] = Mathf.Max(_messageTimers[message], duration);
        else
            _messageTimers.Add(message, duration);
    }
}