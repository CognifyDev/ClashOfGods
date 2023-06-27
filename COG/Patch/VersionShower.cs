using System;
using System.Collections;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using HarmonyLib;
using Newtonsoft.Json.Linq;
using TMPro;
using UnityEngine;

namespace COG.Patch
{
    public static class VersionShower//From SNR
    {
        public static string BaseCredentials = $@"<color=#DD1717>Clash</color> <color=#690B0B>Of</color> <color=#12EC3D>Gods</color> v1.0.0 beta";

        [HarmonyPatch(typeof(PingTracker), nameof(PingTracker.Update))]
        private static class PingTrackerPatch
        {
            static void Postfix(PingTracker __instance)
            {
                __instance.text.alignment = TextAlignmentOptions.TopRight;
                if (AmongUsClient.Instance.GameState == InnerNet.InnerNetClient.GameStates.Started)
                {
                    __instance.text.text = $"{BaseCredentials}\n{__instance.text.text}";
                    __instance.gameObject.GetComponent<AspectPosition>().DistanceFromEdge = new Vector3(1.2f, 0.1f, 0.5f);
                }
                else
                {
                    __instance.text.text = $"{BaseCredentials}\n{__instance.text.text}";
                    __instance.transform.localPosition = new Vector3(4f, __instance.transform.localPosition.y, __instance.transform.localPosition.z);
                }



            }
        }
    }
}