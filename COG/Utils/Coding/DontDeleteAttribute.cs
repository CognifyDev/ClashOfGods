using System;

namespace COG.Utils.Coding;

/// <summary>
///     The attribute to describe a target which shouldn't be deleted due to something<para/>
///     For example, there is an unused class, but reflection needs it
/// </summary>
[AttributeUsage(AttributeTargets.All, Inherited = false)]
public sealed class DontDeleteAttribute : Attribute
{
    public DontDeleteAttribute()
    {
    }

    public DontDeleteAttribute(string msg)
    {
    }
}