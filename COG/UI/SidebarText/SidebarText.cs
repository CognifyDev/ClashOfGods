using System;
using System.Collections.Generic;
using COG.Utils;
using UnityEngine;

namespace COG.UI.SidebarText;

public class SidebarText
{
    public SidebarText(string title)
    {
        Title = title;
        Objects = new List<string>();
    }

    public string Title { get; protected set; }

    public List<string> Objects { get; protected set; }

    public virtual void ForResult(ref string result)
    {
    }

    public static string GetOptionMessage(CustomOption.CustomOption.CustomOptionType type)
    {
        var text = "";
        CustomOption.CustomOption? parent = null;
        foreach (var customOption in CustomOption.CustomOption.Options)
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