using System;

namespace COG.Utils.Coding;

/// <summary>
///     The attribute to describe a target which is not tested yet
/// </summary>
[AttributeUsage(AttributeTargets.All, Inherited = false)]
public sealed class NotTestedAttribute : Attribute
{
    public NotTestedAttribute()
    {
    }

    public NotTestedAttribute(string message)
    {
    }
}