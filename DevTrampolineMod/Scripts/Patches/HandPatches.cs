using System;
using System.Collections;
using System.Text;
using GorillaLocomotion;
using HarmonyLib;
using UnityEngine;

namespace DevTrampolineMod.Scripts.Patches
{
    [HarmonyPatch(typeof(Player), "GetSlidePercentage")]
    public class HandPatches
    {
        public static void Prefix(Player __instance, ref float __result, RaycastHit raycastHit)
        {
            __instance.StartCoroutine(Check(raycastHit));
        }

        public static IEnumerator Check(RaycastHit raycastHit)
        {
            var cmp = raycastHit.collider.GetComponentInParent<Bounce>() ?? raycastHit.collider.GetComponent<Bounce>();
            cmp?.Impact(true);
            yield break;
        }
    }
}
