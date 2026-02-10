using System.Linq;
using UnityEngine;

namespace COG.Utils;

public static class CameraUtils
{
    public static Camera FindCamera(int cameraLayer) => Camera.allCameras.FirstOrDefault(c => (c.cullingMask & (1 << cameraLayer)) != 0);
    public static Vector3 WorldToScreenPoint(Vector3 worldPos, int cameraLayer)
    {
        return FindCamera(cameraLayer)?.WorldToScreenPoint(worldPos) ?? Vector3.zero;
    }

    public static Vector3 ScreenToWorldPoint(Vector3 screenPos, int cameraLayer)
    {
        return FindCamera(cameraLayer)?.ScreenToWorldPoint(screenPos) ?? Vector3.zero;
    }
}