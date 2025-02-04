using System;

namespace COG.Utils.Coding;

/// <summary>
///     The attribute to describe a shit-code project you shouldn't touch.
/// </summary>
[AttributeUsage(AttributeTargets.All, Inherited = false)]
public sealed class ShitCodeAttribute : Attribute
{
}