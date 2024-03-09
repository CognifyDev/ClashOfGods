using System;
using System.Collections.Generic;
using COG.Utils;

namespace COG.Listener.Event;

/// <summary>
/// 事件类
/// </summary>
public class Event
{
    private static readonly List<Type> SubClasses = typeof(Event).GetAllSubclasses();

    public static List<Type> GetSubClasses() => new(SubClasses);
    
    public string Name { get; }
    public int Id { get; }

    protected Event()
    {
        Name = GetType().Name;
        Id = Name.GetHashCode();
    }
}