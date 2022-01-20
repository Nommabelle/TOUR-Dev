using HarmonyLib;

namespace TownOfUs
{
    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.SetKillTimer))]
    public static class PatchKillTimer
    {
        [HarmonyPriority(Priority.First)]
        public static void Prefix(PlayerControl __instance, ref float time)
        {
            if (
                PlayerControl.GameOptions.KillCooldown > 10
                && __instance.Data.IsImpostor && time == 10
                && (__instance.killTimer > time || __instance.killTimer == 0))
            {
                time = CustomGameOptions.InitialCooldowns;
            }
        }
    }
}