using UnityEngine;

namespace COG.UI.CustomOption.CustomOptionTab;

public class CustomOptionTab
{
    public string DisplayName { get; set; }
    
    public Color TextColor { get; set; }
    public Color BackgroundColor { get; set; }
    
    /// <summary>
    /// 这个tab标签的权重
    /// 权重是tab标签的优先级，其越大，排名越靠前
    /// </summary>
    public uint Weight { get; set; }

    public class Builder
    {
        private readonly CustomOptionTab _tab = new();

        public Builder NewBuilder()
        {
            return new Builder();
        }

        public Builder SetDisplayName(string name)
        {
            _tab.DisplayName = name;
            return this;
        }

        public Builder SetTextColor(Color color)
        {
            _tab.TextColor = color;
            return this;
        }

        public Builder SetBackground(Color color)
        {
            _tab.BackgroundColor = color;
            return this;
        }

        public Builder SetWeight(uint weight)
        {
            _tab.Weight = weight;
            return this;
        }

        public CustomOptionTab Build() => _tab;
    }
}