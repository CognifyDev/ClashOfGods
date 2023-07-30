using System;
using COG.UI.CustomOption;

namespace COG.Utils.Resolver;

[Serializable]
public class SerializableCustomOption
{
    private readonly int _id;
    private readonly string _name;
    private readonly object[] _selections;

    private readonly int _defaultSelection;
    private readonly int _selection;
    private readonly SerializableCustomOption? _parent;
    private readonly bool _isHeader;
    private readonly CustomOption.CustomOptionType _type;

    public SerializableCustomOption(CustomOption customOption)
    {
        _id = customOption.ID;
        _name = customOption.Name;
        _selections = customOption.Selections;

        _defaultSelection = customOption.DefaultSelection;
        _selection = customOption.Selection;
        _parent = customOption.Parent == null ? null : new SerializableCustomOption(customOption.Parent);
        _isHeader = customOption.IsHeader;
        _type = customOption.Type;
    }

    public CustomOption ToCustomOption()
    {
        var obj = new CustomOption(_id, _type, _name, _selections, _defaultSelection,
            _parent?.ToCustomOption(), _isHeader)
        {
            Selection = _selection,
        };
        return obj;
    }
}