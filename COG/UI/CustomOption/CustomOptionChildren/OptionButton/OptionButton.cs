using System.Collections.Generic;
using COG.Config.Impl;

namespace COG.UI.CustomOption.CustomOptionChildren.OptionButton;

public class OptionButton : ICustomOptionChildren
{
    public string[] Selections { get; set; }

    public ulong Position { get; private set; }

    public void Next()
    {
        if (Position + 1 > (ulong) Selections.Length)
        {
            Position = 0;
            return;
        }
        Position++;
    }

    public override string ToString()
    {
        return "OptionButton{selections" + $"{Selections}, Position: {Position}" + "}";
    }

    public override int GetHashCode()
    {
        return ToString().GetHashCode();
    }

    public class Builder
    {
        private readonly OptionButton _optionButton = new();
        
        public Builder SetSelections(string[] selections)
        {
            _optionButton.Selections = selections;
            return this;
        }

        public Builder SetSelectionsAsSwitch()
        {
            _optionButton.Selections = new[] { LanguageConfig.Instance.Enable, LanguageConfig.Instance.Disable };
            return this;
        }

        public Builder SetSelectionsAsNumber(long maxValue, long minValue)
        {
            var list = new List<string>();
            for (var i = minValue; i <= maxValue; i++)
            {
                list.Add(i.ToString());
            }

            _optionButton.Selections = list.ToArray();
            return this;
        }

        public OptionButton Build() => _optionButton;
        
        public Builder NewBuilder()
        {
            return new Builder();
        }
    }
}