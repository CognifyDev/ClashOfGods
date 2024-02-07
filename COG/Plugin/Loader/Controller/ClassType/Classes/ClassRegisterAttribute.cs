using System;

namespace COG.Plugin.Loader.Controller.ClassType.Classes;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class ClassRegisterAttribute : Attribute
{
    public string ClassName { get; }

    public ClassRegisterAttribute(string className)
    {
        ClassName = className;
    }
}