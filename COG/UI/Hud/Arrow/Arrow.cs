using System.Linq;
using Il2CppInterop.Runtime;
using UnityEngine;

namespace COG.UI.Hud.Arrow;

public static class Arrow
{
    public static ArrowBehaviour Create(Vector3 target, Color? color = null)
    {
        var template = Resources.FindObjectsOfTypeAll(Il2CppType.Of<ArrowBehaviour>()).First().Cast<ArrowBehaviour>();
        var arrow = Object.Instantiate(template, Camera.main.transform);

        arrow.MaxScale = 0.75f;
        arrow.target = target;
        arrow.image = arrow.GetComponent<SpriteRenderer>();

        if (color.HasValue)
            arrow.image.color = color.Value;

        arrow.gameObject.SetActive(true);
        return arrow;
    }
}