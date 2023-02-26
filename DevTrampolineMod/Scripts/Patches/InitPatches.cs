using GorillaLocomotion;
using System.Collections;
using HarmonyLib;

namespace DevTrampolineMod.Scripts.Patches
{
    [HarmonyPatch(typeof(Player), "Awake")]
    public class InitPatches
    {
        public static void Postfix(Player __instance)
        {
            __instance.StartCoroutine(Delay());
        }

        public static IEnumerator Delay()
        {
            yield return 0;

            Plugin.Instance.InitMethod();
            yield break;
        }
    }
}
