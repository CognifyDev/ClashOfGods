using System;
using UnityEngine;

namespace COG.Utils;

public class ColorUtils
{
    public static string ToColorString(Color color, string str)
    {
        return
            $"<color=#{ToByte(color.r):X2}{ToByte(color.g):X2}{ToByte(color.b):X2}{ToByte(color.a):X2}>{str}</color>";
    }

    private static byte ToByte(float f)
    {
        f = Mathf.Clamp01(f);
        return (byte)(f * 255);
    }

    public static Color AsColor(string color)
    {
        var red = Convert.ToByte(color.Substring(1, 2), 16);
        var green = Convert.ToByte(color.Substring(3, 2), 16);
        var blue = Convert.ToByte(color.Substring(5, 2), 16);

        return new Color(red, green, blue);
    }
}