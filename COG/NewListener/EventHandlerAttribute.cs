using System;

namespace COG.NewListener;

/// <summary>
/// The attribute used to mark a method as a listener method
/// </summary>
[AttributeUsage(AttributeTargets.Method, Inherited = false)]
public sealed class EventHandlerAttribute : Attribute
{
}