using System;

namespace COG.Utils.Coding;

/// <summary>
///     The attribute to describe a target which is not finished yet
/// </summary>
[AttributeUsage(AttributeTargets.All, Inherited = false)]
public sealed class WorkInProgressAttribute : Attribute
{
    // ReSharper disable once UnusedParameter.Local
    public WorkInProgressAttribute(string message)
    {
    }
    
    public WorkInProgressAttribute()
    {
    }
}