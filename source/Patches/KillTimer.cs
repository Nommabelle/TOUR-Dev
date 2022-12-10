using HarmonyLib;
using UnityEngine;

namespace TownOfUs.Patches
{
    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.SetKillTimer))]
    public static class KillTimer
    {
        public static bool Prefix(PlayerControl __instance, ref float time)
        {
            if (__instance.Data.Role.CanUseKillButton)
            {
                if (GameOptionsManager.Instance.normalGameHostOptions.KillCooldown <= 0f)
                {
                    return false;
                }

                var maxvalue = time > GameOptionsManager.Instance.normalGameHostOptions.KillCooldown ? time + 1f : GameOptionsManager.Instance.normalGameHostOptions.KillCooldown;
                __instance.killTimer = Mathf.Clamp(time, 0, maxvalue);
                DestroyableSingleton<HudManager>.Instance.KillButton.SetCoolDown(__instance.killTimer, maxvalue);
            }

            return false;
        } 
    }
}