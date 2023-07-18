using System.Collections.Generic;

namespace COG.UI.SidebarText;

public class SidebarTextManager
{
    private static readonly SidebarTextManager Manager = new();

    private readonly List<SidebarText> _sidebarTexts = new();

    public void RegisterSidebarText(SidebarText sidebarText)
    {
        _sidebarTexts.Add(sidebarText);
    }

    public void RegisterSidebarTexts(SidebarText[] sidebarTexts)
    {
        _sidebarTexts.AddRange(sidebarTexts);
    }

    public List<SidebarText> GetSidebarTexts()
    {
        return _sidebarTexts;
    }

    public static SidebarTextManager GetManager()
    {
        return Manager;
    }
}