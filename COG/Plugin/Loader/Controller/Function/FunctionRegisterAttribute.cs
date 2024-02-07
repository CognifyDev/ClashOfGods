using System;

namespace COG.Plugin.Loader.Controller.Function;

[AttributeUsage(AttributeTargets.Method, Inherited = false)]
public sealed class FunctionRegisterAttribute : Attribute
{
    public string FunctionName { get; }

    public FunctionRegisterAttribute(string functionName)
    {
        FunctionName = functionName;
    }
}