using System;

namespace COG.Listener.Attribute;

/// <summary>
/// Marks a delegate handler registered via On&lt;T&gt;() so that it only fires for the local player's role instance.
/// </summary>
[AttributeUsage(AttributeTargets.Method, Inherited = false)]
public sealed class LocalOnlyAttribute : System.Attribute;
