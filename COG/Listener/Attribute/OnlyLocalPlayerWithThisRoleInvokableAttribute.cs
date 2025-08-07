using System;

namespace COG.Listener.Attribute;

[AttributeUsage(AttributeTargets.Method, Inherited = false)]
public sealed class OnlyLocalPlayerWithThisRoleInvokableAttribute : System.Attribute
{
}