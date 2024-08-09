using UnityEngine;

namespace COG.UI.CustomGameObject;

public abstract class TemplatedCustomGameObject : ICustomGameObject
{
    public GameObject GameObject { get; }
    
    public TemplatedCustomGameObject(GameObject template)
    {
        GameObject = template;
    }

    public abstract void Update();

    public abstract void Destroy();
}