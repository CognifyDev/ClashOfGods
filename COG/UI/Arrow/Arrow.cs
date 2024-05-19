using COG.Constant;
using COG.States;
using COG.Utils;
using Reactor.Utilities.Extensions;
using System.Collections.Generic;
using UnityEngine;

namespace COG.UI.Arrow;

public class Arrow
{
    public static List<Arrow> CreatedArrows { get; } = new();

    public ArrowBehaviour Behaviour { get; }
    public SpriteRenderer Renderer { get; set; }
    public Vector3 Target { get; set; }
    public Color? Color { get; set; }
    public GameObject ArrowObject { get; }

    public Arrow(Vector3 target, Color? color = null)
    {
        if (!GameStates.InGame) return;
        Target = target;
        Color = color;

        ArrowObject = new("Arrow") { layer = 5 };
        Renderer = ArrowObject.AddComponent<SpriteRenderer>();
        Behaviour = ArrowObject.AddComponent<ArrowBehaviour>();

        if (Color.HasValue)
            Renderer.color = Color.Value;
        else
            Renderer.color = new(0.9034f, 1f, 0, 1f);
        Renderer.sprite = ResourceUtils.LoadSprite(ResourcesConstant.ArrowImage, 200f);

        Behaviour.image = Renderer;
        Behaviour.target = Target;
        ArrowObject.SetActive(true);

        CreatedArrows.Add(this);
    }

    public void Update() => Behaviour.Update();

    public void Destroy()
    {
        ArrowObject.Destroy();
        CreatedArrows.Remove(this);
    }
}