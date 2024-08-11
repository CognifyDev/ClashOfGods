using UnityEngine;

namespace COG.UI.CustomGameObject;

public abstract class TemplatedCustomGameObject : ICustomGameObject
{
    public GameObject GameObject { get; }
    
    public TemplatedCustomGameObject(GameObject template, Transform transform)
    {
        GameObject = Object.Instantiate(template, transform);
    }
}