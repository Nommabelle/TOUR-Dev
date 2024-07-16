using HarmonyLib;
using TownOfUs.Roles;

namespace TownOfUs.CrewmateRoles.PhantomMod
{

    [HarmonyPatch(typeof(KillButton), nameof(KillButton.DoClick))]
    public class PerformKill
    {
        public static bool Prefix(KillButton __instance)
        {
            if (__instance != DestroyableSingleton<HudManager>.Instance.KillButton) return true;
            if (!PlayerControl.LocalPlayer.Is(RoleEnum.Phantom)) return true;
            if (!PlayerControl.LocalPlayer.CanMove) return false;
            if (PlayerControl.LocalPlayer.Data.IsDead) return false;
            var role = Role.GetRole<Phantom>(PlayerControl.LocalPlayer);
            if (role.Caught) return false;
            if (!__instance.enabled) return false;

            // Change the Phantom into a ghost
            role.Caught = true;
            role.Player.Exiled();
            Utils.Rpc(CustomRPC.CatchPhantom, role.Player.PlayerId);

            return false;
        }
    }
}