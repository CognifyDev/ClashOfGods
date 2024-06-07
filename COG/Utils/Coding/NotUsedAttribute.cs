using System;

namespace COG.Utils.Coding;

/// <summary>
///     The attribute to describe a target which is not used but will be used later
/// </summary>
[AttributeUsage(AttributeTargets.All, Inherited = false)]
public sealed class NotUsedAttribute : Attribute
{
}