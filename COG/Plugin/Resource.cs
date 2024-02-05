using System;

namespace COG.Plugin;

/// <summary>
/// A resource class for plugins
/// </summary>
[Serializable]
public class Resource
{
    public byte[] Bytes { get; }
    
    public Resource(string file)
    {
        Bytes = System.IO.File.ReadAllBytes(file);
    }
}