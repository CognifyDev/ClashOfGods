using System;

namespace COG.Utils.Coding;

public sealed class FixMeAttribute : Attribute
{
    public FixMeAttribute(string message)
    {
    }
}
