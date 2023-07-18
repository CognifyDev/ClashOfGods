using System;
using COG.Config.Impl;
using COG.UI.SidebarText;
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
        foreach (var sidebarObject in sidebar.Objects)
        {
            text += sidebarObject + Environment.NewLine;
        }
        text += LanguageConfig.Instance.MessageForNextPage.Replace("%correctpage%", _typePage + "").Replace("%pagecount%", pages + "");
        result = text;
    }

    public void OnKeyboardJoystickUpdate(KeyboardJoystick keyboardJoystick)
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            _typePage += 1;
        }
        
        // from 49 to 57
        for (int num = 49; num <= 57; num ++)
        {
            var page = num - (49 - 1); // 指代的page
            var keycode = (KeyCode) num;
            if (Input.GetKeyDown(keycode))
            {
                _typePage = page;
            } 
        }
        
        // from 257 to 265
        for (int num = 257; num <= 265; num++)
        {
            var page = num - (257 - 1);
            var keycode = (KeyCode) num;
            if (Input.GetKeyDown(keycode))
            {
                _typePage = page;
            }
        }
    }
}