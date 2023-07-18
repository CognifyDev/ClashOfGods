namespace COG.UI.SidebarText.Impl;

public class OriginalSettings : SidebarText
{
    public OriginalSettings() : base("原版设置")
    {
    }

    public override void ForResult(ref string result)
    {
        Objects.Clear();
        Objects.AddRange(new []
        {
            result
        });
    }
}