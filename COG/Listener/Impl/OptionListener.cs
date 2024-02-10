using System;
using System.Collections.Generic;
using COG.Config.Impl;
using COG.Rpc;
using COG.UI.CustomOption;
using COG.UI.SidebarText;
using COG.Utils;
using UnityEngine;

namespace COG.Listener.Impl;

public class OptionListener : IListener
{
    private int _typePage = 1;

    public void OnIGameOptionsExtensionsDisplay(ref string result)
    {
        var pages = SidebarTextManager.GetManager().GetSidebarTexts().Count;

        if (pages <= 0) return;

        var sidebars = SidebarTextManager.GetManager().GetSidebarTexts();
        if (_typePage > sidebars.Count || _typePage == 0) _typePage = 1;

        if (sidebars.Count <= 0) return;

        var sidebar = sidebars[_typePage - 1];
        var text = sidebar.Title + Environment.NewLine;

        sidebar.ForResult(ref result);
        foreach (var sidebarObject in sidebar.Objects) text += sidebarObject + Environment.NewLine;
        text += LanguageConfig.Instance.MessageForNextPage.CustomFormat(_typePage, pages);
        result = text;
    }

    public void OnKeyboardJoystickUpdate(KeyboardJoystick keyboardJoystick)
    {
        if (Input.GetKeyDown(KeyCode.Tab)) _typePage += 1;

        // from Alpha1 to Alpha9
        for (var num = (int)KeyCode.Alpha1; num <= (int)KeyCode.Alpha9; num++)
        {
            var page = num - ((int)KeyCode.Alpha1 - 1); // 指代的page
            var keycode = (KeyCode)num;
            if (Input.GetKeyDown(keycode)) _typePage = page;
        }

        // from Keypad1 to Keypad9
        for (var num = (int)KeyCode.Keypad1; num <= (int)KeyCode.Keypad9; num++)
        {
            var page = num - ((int)KeyCode.Keypad1 - 1);
            var keycode = (KeyCode)num;
            if (Input.GetKeyDown(keycode)) _typePage = page;
        }
    }
}