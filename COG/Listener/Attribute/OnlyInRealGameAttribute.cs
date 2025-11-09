using System;

namespace COG.Listener.Attribute;

[AttributeUsage(AttributeTargets.Method, Inherited = false)]
public sealed class OnlyInRealGameAttribute : System.Attribute;