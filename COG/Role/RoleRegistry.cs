using System;
using System.Collections.Generic;

namespace COG.Role;

/// <summary>
/// Central registry for role instances. Replaces static _instance pattern.
/// </summary>
public static class RoleRegistry
{
    private static readonly Dictionary<Type, CustomRole> _instances = new();
    
    public static void Register(CustomRole role)
    {
        _instances[role.GetType()] = role;
    }
    
    public static T? Get<T>() where T : CustomRole
    {
        return _instances.TryGetValue(typeof(T), out var role) ? (T)role : null;
    }
    
    public static CustomRole? Get(Type type)
    {
        return _instances.TryGetValue(type, out var role) ? role : null;
    }
    
    public static bool IsRegistered<T>() where T : CustomRole
    {
        return _instances.ContainsKey(typeof(T));
    }
    
    public static void Clear()
    {
        _instances.Clear();
    }
}
