using UnityEngine;

namespace COG.UI.CustomGameObject;

public class CustomGameObject
{
    public CustomGameObject(string name)
    {
        Name = name;
        GameObject = new GameObject(Name);
    }

    public string Name { get; }

    public GameObject GameObject { get; }

    public void Update()
    {
    }
}