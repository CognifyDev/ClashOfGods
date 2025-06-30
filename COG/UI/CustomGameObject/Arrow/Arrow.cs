using System.Collections.Generic;
using COG.Constant;
using COG.States;
using COG.Utils;
using UnityEngine;

namespace COG.UI.CustomGameObject.Arrow;

public class Arrow : ICustomGameObject
{
#nullable disable
    public Arrow(Vector3 target, Color? color = null)
    {
        if (!GameStates.InRealGame) return;
        Target = target;
        Color = color;

        ArrowObject = new GameObject("Arrow") { layer = 5 };
        Renderer = ArrowObject.AddComponent<SpriteRenderer>();
        Behaviour = ArrowObject.AddComponent<ArrowBehaviour>();

        if (Color.HasValue)
            Renderer.color = Color.Value;
        else
            Renderer.color = UnityEngine.Color.yellow;
        
        Renderer.sprite = ResourceUtils.LoadSprite(ResourceConstant.ArrowImage, 200f);

        Behaviour.image = Renderer;
        Behaviour.target = Target;
        ArrowObject.SetActive(true);

        CreatedArrows.Add(this);
    }
#nullable restore

    public static List<Arrow> CreatedArrows { get; } = new();

    public ArrowBehaviour Behaviour { get; }
    public SpriteRenderer Renderer { get; set; }
    public Vector3 Target { get; set; }
    public Color? Color { get; set; }
    public GameObject ArrowObject { get; }

    public void Update()
    {
        Behaviour.Update();
    }

    public void Destroy()
    {
        ArrowObject.Destroy();
        CreatedArrows.Remove(this);
    }
}