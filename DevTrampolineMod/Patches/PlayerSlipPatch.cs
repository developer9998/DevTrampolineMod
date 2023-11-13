using System;
using System.Collections;
using System.Text;
using DevTrampolineMod.Behaviours;
using GorillaLocomotion;
using HarmonyLib;
using UnityEngine;

namespace DevTrampolineMod.Patches
{
    [HarmonyPatch(typeof(Player), "GetSlidePercentage")]
    public class PlayerSlipPatch
    {
        private static Bounce component;

        [HarmonyWrapSafe]
        public static void Prefix(RaycastHit raycastHit)
        {
            component = raycastHit.collider.GetComponent<Bounce>() ?? raycastHit.collider.GetComponentInParent<Bounce>();
            component?.Impact(true);
        }
    }
}
