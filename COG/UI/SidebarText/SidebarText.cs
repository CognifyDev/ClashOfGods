using System.Collections.Generic;

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
}