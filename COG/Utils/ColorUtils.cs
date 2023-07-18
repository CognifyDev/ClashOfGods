using UnityEngine;

namespace COG.Utils;

public class ColorUtils
{
    public static string ToAmongUsColorString(Color color, string str)
    {
        return $"<color=#{ToByte(color.r):X2}{ToByte(color.g):X2}{ToByte(color.b):X2}{ToByte(color.a):X2}>{str}</color>";
    }
    
    private static byte ToByte(float f) {
        f = Mathf.Clamp01(f);
        return (byte)(f * 255);
    }
}