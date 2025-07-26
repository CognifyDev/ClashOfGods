using System.Collections.Generic;
using System.Linq;
using COG.Constant;
using COG.States;
using COG.Utils;
using Il2CppInterop.Runtime;
using UnityEngine;

namespace COG.UI.CustomGameObject.Arrow;

public static class Arrow
{
    public static ArrowBehaviour Create(Vector3 target, Color? color = null)
    {
        var template = (ArrowBehaviour)Resources.FindObjectsOfTypeAll(Il2CppType.Of<ArrowBehaviour>()).First();
        var arrow = Object.Instantiate(template, Camera.main.transform);
        
        arrow.MaxScale = 0.75f;
        arrow.target = target;

        if (color.HasValue)
            arrow.image.color = color.Value;

        arrow.gameObject.SetActive(true);
        return arrow;
    }
}