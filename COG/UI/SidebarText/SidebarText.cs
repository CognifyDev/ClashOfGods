using System;
using System.Collections.Generic;
using COG.Modules;
using COG.Utils;
using Epic.OnlineServices.Presence;
using UnityEngine;

namespace COG.UI.SidebarText;

public class SidebarText
{
    public string Title { get; protected set; }
    
    public List<string> Objects { get; protected set; }

    public SidebarText(string title)
    {
        Title = title;
        Objects = new();
    }
    
    public virtual void ForResult(ref string result) {}

    public static string GetOptionMessage(CustomOption.CustomOptionType type)
    {
        var text = "";
        CustomOption? parent = null;
        foreach (var customOption in CustomOption.Options)
        {
            if (customOption == null) continue;
            if (customOption.Type != type) continue;
            if (parent != null && parent == customOption.Parent) goto SetOptions;
            parent = customOption.Parent;
            if (parent == null) continue;
            text += parent.Name + Environment.NewLine;
            SetOptions:
            if (customOption.Name.Equals("") || customOption.Name.Equals(" ")) continue;
            text += ColorUtils.ToAmongUsColorString(Color.gray, "→ ") + customOption.Name + Environment.NewLine;
        }

        return text;
    }
}