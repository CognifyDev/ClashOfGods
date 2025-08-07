using System;

namespace COG.Utils.Coding;

[AttributeUsage(AttributeTargets.All, Inherited = false)]
public class DangerousCodeAttribute : Attribute
{
}