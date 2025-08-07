using UnityEngine;

namespace COG.UI.ClientOption;

public interface IClientOption
{
    string Translatable { get; }
    Component? Component { get; }
}