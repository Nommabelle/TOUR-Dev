using HarmonyLib;
using System;
using UnityEngine;
using TownOfUs.Extensions;

namespace TownOfUs
{
    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.SetKillTimer))]
    public static class PatchKillTimer
    {
        public static bool GameStarted = false;
        [HarmonyPriority(Priority.First)]
        public static void Prefix(PlayerControl __instance, ref float time)
        {
            if (__instance.Data.IsImpostor() && time <= 11f
                && Math.Abs(__instance.killTimer - time) > 2 * Time.deltaTime
                && GameStarted == false)
            {
                time = CustomGameOptions.InitialCooldowns - 0.25f;
                GameStarted = true;
            }
        }
    }
}