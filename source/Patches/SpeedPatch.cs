using HarmonyLib;
using TownOfUs.Extensions;
using TownOfUs.Roles;

namespace TownOfUs.Patches
{
    [HarmonyPatch]
    public static class SpeedPatch
    {
        [HarmonyPatch(typeof(PlayerPhysics), nameof(PlayerPhysics.FixedUpdate))]
        [HarmonyPostfix]
        public static void PostfixPhysics(PlayerPhysics __instance)
        {
            if (__instance.AmOwner && GameData.Instance && __instance.myPlayer.CanMove && !__instance.myPlayer.Data.IsDead)
            {
                __instance.body.velocity *= __instance.myPlayer.GetAppearance().SpeedFactor;
                foreach (var role in Role.GetRoles(RoleEnum.Venerer))
                {
                    var venerer = (Venerer)role;
                    if (venerer.Enabled)
                    {
                        if (venerer.KillsAtStartAbility >= 2 && venerer.Player == PlayerControl.LocalPlayer) __instance.body.velocity *= CustomGameOptions.SprintSpeed;
                        else if (venerer.KillsAtStartAbility >= 3) __instance.body.velocity *= CustomGameOptions.FreezeSpeed;
                    }
                }
            }
        }

        [HarmonyPatch(typeof(CustomNetworkTransform), nameof(CustomNetworkTransform.FixedUpdate))]
        [HarmonyPostfix]
        public static void PostfixNetwork(CustomNetworkTransform __instance)
        {
            if (!__instance.AmOwner && __instance.interpolateMovement != 0.0f && !__instance.gameObject.GetComponent<PlayerControl>().Data.IsDead)
            {
                var player = __instance.gameObject.GetComponent<PlayerControl>();
                __instance.body.velocity *= player.GetAppearance().SpeedFactor;

                foreach (var role in Role.GetRoles(RoleEnum.Venerer))
                {
                    var venerer = (Venerer)role;
                    if (venerer.Enabled)
                    {
                        if (venerer.KillsAtStartAbility >= 2 && venerer.Player == player) __instance.body.velocity *= CustomGameOptions.SprintSpeed;
                        else if (venerer.KillsAtStartAbility >= 3) __instance.body.velocity *= CustomGameOptions.FreezeSpeed;
                    }
                }
            }
        }
    }
}