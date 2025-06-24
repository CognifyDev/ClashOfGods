using System;

namespace COG.Utils.Coding;

[AttributeUsage(AttributeTargets.All, Inherited = false)]
public sealed class TodoAttribute : Attribute
{
	public TodoAttribute()
	{
	}

	public TodoAttribute(string todo)
	{
	}
}