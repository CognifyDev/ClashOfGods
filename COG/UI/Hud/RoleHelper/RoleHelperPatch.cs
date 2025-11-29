using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using static COG.Utils.ResourceUtils;

namespace COG.UI.Hud.RoleHelper
{
    [HarmonyPatch(typeof(HudManager),nameof(HudManager.Update))]
    public static class RoleHelperPatch
    {
        public static GameObject Panel;
        public static TextMeshPro Title;
        public static TextMeshPro SubTitle;
        public static TextMeshPro Text;
        public static void IntiAll(HudManager hud)
        {
            Panel.name = "RolePanel";
            Panel.transform.SetParent(hud.transform);
            var sr = Panel.AddComponent<SpriteRenderer>();
            sr.sprite = LoadSprite("COG.")
        }
    }
}
