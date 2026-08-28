using COG.UI.CustomOption;
using COG.UI.CustomOption.ValueRules.Impl;

namespace COG.Role.Options;

/// <summary>
/// Type-safe wrapper for integer options.
/// </summary>
public class IntOption
{
    private readonly CustomOption _option;
    
    public IntOption(CustomRole role, string key, int min, int max, int defaultValue, int step = 1)
    {
        _option = CustomOption.Of(
            CustomRole.GetTabType(role),
            () => role.GetContextFromLanguage(key),
            new IntOptionValueRule(defaultValue, min, max, step));
        _option.Register();
        role.AllOptions.Add(_option);
    }
    
    public int Value => _option.GetInt();
    public static implicit operator int(IntOption opt) => opt.Value;
}
