using HarmonyLib;

namespace TownOfUs.NeutralRoles.GlitchMod
{
    [HarmonyPatch(typeof(GameManager), nameof(GameManager.ShouldCheckForGameEnd))]
    internal class GameOverDueToDeathPatch
    {
        public static void Postfix(out bool __result)
        {
            __result = false;
        }
    }
}