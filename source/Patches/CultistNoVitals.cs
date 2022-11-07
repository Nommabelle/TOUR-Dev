using HarmonyLib;
using UnityEngine;

namespace TownOfUs.Patches
{
    [HarmonyPatch(typeof(VitalsMinigame), nameof(VitalsMinigame.Begin))]
    public class NoVitals
    {
        public static bool Prefix(VitalsMinigame __instance)
        {
            if (CustomGameOptions.GameMode == GameMode.Cultist)
            {
                Object.Destroy(__instance.gameObject);
                return false;
            }

            return true;
        }
    }
}