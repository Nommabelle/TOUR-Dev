using HarmonyLib;
using TownOfUs.Roles;

namespace TownOfUs.NeutralRoles.PhantomMod
{
    [HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
    public static class HudManagerUpdate
    {
        public static void Postfix(HudManager __instance)
        {
            if (PlayerControl.AllPlayerControls.Count <= 1) return;
            if (PlayerControl.LocalPlayer == null) return;
            if (PlayerControl.LocalPlayer.Data == null) return;
            if (!PlayerControl.LocalPlayer.Is(RoleEnum.Phantom)) return;
            var role = Role.GetRole<Phantom>(PlayerControl.LocalPlayer);

            // set kill button which can be used by phantom to turn to a ghost with PerformKill
            __instance.KillButton.gameObject.SetActive(!role.Caught);
        }
    }
}