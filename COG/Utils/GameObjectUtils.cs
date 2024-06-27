using UnityEngine;

namespace COG.Utils;

public static class GameObjectUtils
{
    public static void Destroy(this Object obj) => Object.Destroy(obj);
    public static void DestroyImmediate(this Object obj) => Object.DestroyImmediate(obj);
}