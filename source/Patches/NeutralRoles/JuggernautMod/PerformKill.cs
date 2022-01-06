using HarmonyLib;
using TownOfUs.Roles;

namespace TownOfUs.NeutralRoles.JuggernautMod
{
    [HarmonyPatch(typeof(KillButtonManager), nameof(KillButtonManager.PerformKill))]
    internal class PerformKill
    {
        public static bool Prefix(KillButtonManager __instance)
        {
            if (PlayerControl.LocalPlayer.Is(RoleEnum.Juggernaut) && __instance.isActiveAndEnabled &&
                !__instance.isCoolingDown)
                return Role.GetRole<Juggernaut>(PlayerControl.LocalPlayer).UseAbility(__instance);

            return true;
        }
    }
}