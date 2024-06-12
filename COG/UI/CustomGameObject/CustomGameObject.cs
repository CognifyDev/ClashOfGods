namespace COG.UI.CustomGameObject;

public class CustomGameObject
{
    public string Name { get; }
    
    public UnityEngine.GameObject GameObject { get; }

    public CustomGameObject(string name)
    {
        Name = name;
        GameObject = new UnityEngine.GameObject(Name);
    }

    public void Update()
    {
        
    }
}