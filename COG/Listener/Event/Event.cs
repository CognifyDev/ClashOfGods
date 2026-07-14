using System;
using System.Collections.Generic;
using COG.Utils;

namespace COG.Listener.Event;

/// <summary>
///     事件类
/// </summary>
public class Event
{
    private static readonly List<Type> SubClasses = typeof(Event).GetAllSubclasses();
    private static int _nextId = 1;
    private static readonly Dictionary<Type, int> TypeToId = [];

    protected Event()
    {
        Name = GetType().Name;
        Id = GetOrCreateId(GetType());
    }

    public string Name { get; }
    public int Id { get; }

    private static int GetOrCreateId(Type type)
    {
        if (!TypeToId.TryGetValue(type, out var id))
        {
            id = _nextId++;
            TypeToId[type] = id;
        }
        return id;
    }

    public static List<Type> GetSubClasses()
    {
        return [..SubClasses];
    }
}