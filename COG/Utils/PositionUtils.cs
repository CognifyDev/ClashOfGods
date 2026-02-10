using System.Collections.Generic;
using UnityEngine;

namespace COG.Utils;

public static class PositionUtils
{
    public static List<Vector3> skeldSpawn = new()
    {
        new Vector3(-1.1028f, 4.9466f, 0.0f), // cafeteria
        new Vector3(9.119f, 1.4038f, 0.0f), // weapons
        new Vector3(6.5369f, -3.5533f, 0.0f), // O2
        new Vector3(16.7503f, -4.9249f, 0.0f), // navigation
        new Vector3(8.9308f, -11.9944f, 0.0f), // shields
        new Vector3(4.0746f, -15.8506f, 0.0f), // communications
        new Vector3(-2.1067f, -16.1015f, 0.0f), // storage
        new Vector3(-7.0197f, -8.9111f, 0.0f), // electric
        new Vector3(-17.0391f, -9.6947f, 0.0f), // lower-engine
        new Vector3(-20.9293f, -5.3046f, 0.0f), // reactor
        new Vector3(-13.3043f, -5.3046f, 0.0f), // security
        new Vector3(-15.3514f, 1.1165f, 0.0f), // upper-engine
        new Vector3(-8.946f, -3.6638f, 0.0f) // medbay
    };

    public static List<Vector3> miraSpawn = new()
    {
        new Vector3(-4.5314f, 3.1964f, 0.0f), // launchpad
        new Vector3(15.3814f, -1.0567f, 0.0f), // medbay
        new Vector3(6.1146f, 6.3599f, 0.0f), // decontimination
        new Vector3(9.5266f, 12.3496f, 0.0f), // reactor
        new Vector3(1.7446f, 10.7656f, 0.0f), // lab
        new Vector3(17.8588f, 14.4313f, 0.0f), // communications
        new Vector3(21.0038f, 20.4957f, 0.0f), //admin
        new Vector3(14.8338f, 20.6414f, 0.0f), //greenhouse left
        new Vector3(17.8238f, 23.7692f, 0.0f), //bottom right cross
        new Vector3(25.5716f, 1.8964f, 0.0f), //balcony 11
        new Vector3(25.1819f, -1.8909f, 0.0f),
        new Vector3(19.5687f, 2.1507f, 0.0f)
    };

    public static List<Vector3> polusSpawn = new()
    {
        new Vector3(16.4563f, -6.9233f, 0.0f), // dropship
        new Vector3(5.4774f, -9.7978f, 0.0f), // electric
        new Vector3(3.3069f, -19.4683f, 0.0f), // O2
        new Vector3(9.7891f, -20.5683f, 0.0f), // death valley
        new Vector3(12.5632f, -23.3549f, 0.0f), // weapons
        new Vector3(14.8053f, -13.9657f, 0.0f), // lab
        new Vector3(20.5668f, -12.0237f, 0.0f), // upper-decontimination
        new Vector3(28.7699f, -9.7874f, 0.0f), // specimen
        new Vector3(39.1572f, -9.9311f, 0.0f), // lower-decontimination
        new Vector3(36.4155f, -21.3363f, 0.0f), // office
        new Vector3(24.0103f, -24.829f, 0.0f), //comms table 11
        new Vector3(21.9853f, -19.0738f, 0.0f)
    };

    public static List<Vector3> airshipSpawn = new()
    {
        new Vector3(-13.5475f, -12.1318f, 0.0f),
        new Vector3(1.9533f, -12.1844f, 0.0f),
        new Vector3(24.6739f, -5.5841f, 0.0f),
        new Vector3(34.1575f, -0.4562f, 0.0f),
        new Vector3(25.7596f, 7.3206f, 0.0f),
        new Vector3(11.4754f, 8.6953f, 0.0f),
        new Vector3(4.0469f, 8.7285f, 0.0f),
        new Vector3(11.1479f, 15.8621f, 0.0f),
        new Vector3(-8.7757f, 6.7163f, 0.0f),
        new Vector3(-21.1178f, -1.3707f, 0.0f),
        new Vector3(-3.5572f, -1.0386f, 0.0f)
    };

    public static List<Vector3> fungleSpawn = new()
    {
        new Vector3(-7.4664f, 8.8714f, 0.0f),
        new Vector3(21.3894f, 13.616f, 0.0f),
        new Vector3(19.6109f, 7.4753f, 0.0f),
        new Vector3(15.0743f, -16.3425f, -0.0f),
        new Vector3(-15.7243f, -7.7109f, 0.0f),
        new Vector3(-16.3124f, -1.9259f, -0.0f),
        new Vector3(-4.1911f, -10.534f, -0.0f),
        new Vector3(-18.1464f, 6.9797f, -0.0f),
        new Vector3(20.5093f, -8.3817f, 0.0f)
    };

    public static List<Vector3> miraDoorway = new()
    {
        new Vector3(7.2639f, 14.1907f, 0.0f),
        new Vector3(6.2961f, 3.7184f, 0.0f),
        new Vector3(13.5582f, 4.2025f, 0.0f),
        new Vector3(13.6618f, 0.2669f, 0.0f),
        new Vector3(-4.3283f, 0.0749f, 0.0f),
        new Vector3(13.1907f, 7.2752f, 0.0f),
        new Vector3(22.21f, 7.2627f, 0.0f),
        new Vector3(25.5939f, -1.173f, 0.0f),
        new Vector3(22.0724f, -0.9444f, 0.0f),
        new Vector3(19.5806f, 1.2253f, 0.0f),
        new Vector3(17.8747f, 15.9618f, 0.0f)
    };
    public static Vector3 AsVector3(this Vector2 vec, float z)
    {
        Vector3 result = vec;
        result.z = z;
        return result;
    }
}