using System.Collections.Generic;
using COG.Constant;
using COG.States;
using COG.Utils;
using Reactor.Utilities.Extensions;
using UnityEngine;

namespace COG.UI.CustomGameObject.Arrow;

public class Arrow
{
    public Arrow(Vector3 target, Color? color = null)
    {
        if (!GameStates.InGame) return;
        Target = target;
        Color = color;

        ArrowObject = new GameObject("Arrow") { layer = 5 };
        Renderer = ArrowObject.AddComponent<SpriteRenderer>();
        Behaviour = ArrowObject.AddComponent<ArrowBehaviour>();

        if (Color.HasValue)
            Renderer.color = Color.Value;
        else
            Renderer.color = new Color(0.9034f, 1f, 0, 1f);
        Renderer.sprite = ResourceUtils.LoadSprite(ResourcesConstant.ArrowImage, 200f);

        Behaviour.image = Renderer;
        Behaviour.target = Target;
        ArrowObject.SetActive(true);

        CreatedArrows.Add(this);
    }

    public static List<Arrow> CreatedArrows { get; } = new();

    public ArrowBehaviour Behaviour { get; }
    public SpriteRenderer Renderer { get; set; }
    public Vector3 Target { get; set; }
    public Color? Color { get; set; }
    public UnityEngine.GameObject ArrowObject { get; }

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