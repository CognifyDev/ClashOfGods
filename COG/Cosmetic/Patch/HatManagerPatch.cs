using HarmonyLib;

namespace COG.Cosmetics.Patch;

[HarmonyPatch(typeof(HatManager))]
public static class HatManagerPatch
{
    [HarmonyPrefix]
    [HarmonyPatch(nameof(HatManager.GetHatById))]
    public static bool GetHat(string hatId, ref HatData __result)
    {
        if (!CosmeticsManager.Instance.TryGetHat(hatId, out var hat)) return true;
        __result = hat.HatData;
        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(HatManager.GetVisorById))]
    public static bool GetVisor(string visorId, ref VisorData __result)
    {
        if (!CosmeticsManager.Instance.TryGetVisor(visorId, out var visor)) return true;
        __result = visor.VisorData;
        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(HatManager.GetNamePlateById))]
    public static bool GetNamePlate(string namePlateId, ref NamePlateData __result)
    {
        if (!CosmeticsManager.Instance.TryGetNamePlate(namePlateId, out var np)) return true;
        __result = np.NamePlateData;
        return false;
    }
}
