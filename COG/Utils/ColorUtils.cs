using System;
using UnityEngine;

namespace COG.Utils;

public static class ColorUtils
{
    public static string ToColorString(this Color color, string str)
    {
        return
            $"<color=#{ToByte(color.r):X2}{ToByte(color.g):X2}{ToByte(color.b):X2}{ToByte(color.a):X2}>{str}</color>";
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="color"></param>
    /// <returns>Returns a hex string that starts with "#" and represents <paramref name="color"/> </returns>
    public static string ToColorHexString(this Color color)
    {
        return $"#{ToByte(color.r):X2}{ToByte(color.g):X2}{ToByte(color.b):X2}{ToByte(color.a):X2}";
    }

    public static Color FromColor32(this Color32 color32) 
        => new(color32.r / (float)byte.MaxValue, color32.g / (float)byte.MaxValue, color32.b / (float)byte.MaxValue, color32.a / (float)byte.MaxValue);

    public static Color FromColor32(byte r, byte g, byte b, byte a = byte.MaxValue)
        => new(r / (float)byte.MaxValue, g / (float)byte.MaxValue, b / (float)byte.MaxValue, a / (float)byte.MaxValue);

    private static byte ToByte(float f)
    {
        f = Mathf.Clamp01(f);
        return (byte)(f * 255);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="color">A string that starts with symbol "#".</param>
    /// <returns></returns>
    public static Color AsColor(string color)
    {
        var red = Convert.ToByte(color.Substring(1, 2), 16);
        var green = Convert.ToByte(color.Substring(3, 2), 16);
        var blue = Convert.ToByte(color.Substring(5, 2), 16);

        return FromColor32(red, green, blue, byte.MaxValue);
    }
}