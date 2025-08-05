using Reactor.Utilities.Attributes;
using System;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

namespace COG.UI.CustomGameObject.HudMessage;

#nullable disable
[RegisterInIl2Cpp]
public class HudMessage : MonoBehaviour
{
    public HudMessage(IntPtr ptr) : base(ptr)
    {
    }

    public static HudMessage Instance { get; private set; }

    private Dictionary<string, float> _messageTimers = new();
    private TextMeshPro _messageObject;

    void Start()
    {
        Instance = this;

        var template = HudManager.Instance.TaskPanel.taskText;
        _messageObject = Object.Instantiate(template, HudManager.Instance.transform);
        _messageObject.name = nameof(HudMessage);
        _messageObject.transform.localPosition = new(0, -2, 0);

        _messageObject.fontSize = _messageObject.fontSizeMin = _messageObject.fontSizeMax = 3;
        _messageObject.alignment = TextAlignmentOptions.Center;

        _messageObject.text = "";
    }

    void Update()
    {
        if(_messageTimers.Count == 0)
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
        {
            _messageTimers[message] = Mathf.Max(_messageTimers[message], duration);
        }
        else
        {
            _messageTimers.Add(message, duration);
        }
    }
}
#nullable restore
