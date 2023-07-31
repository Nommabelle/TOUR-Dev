using HarmonyLib;
using Reactor.Utilities;
using System.Linq;
using TownOfUs.Patches.NeutralRoles;
using TownOfUs.Roles;

namespace TownOfUs.NeutralRoles.JesterMod
{
    [HarmonyPatch(typeof(ExileController), nameof(ExileController.Begin))]
    internal class MeetingExiledEnd
    {
        private static void Postfix(ExileController __instance)
        {
            var exiled = __instance.exiled;
            if (exiled == null) return;
            var player = exiled.Object;

            var role = Role.GetRole(player);
            if (role == null) return;
            if (role.RoleType == RoleEnum.Jester)
            {
                ((Jester)role).Wins();

                if (CustomGameOptions.NeutralEvilWinEndsGame) return;
                if (!PlayerControl.LocalPlayer.Is(RoleEnum.Jester)) return;
                PlayerVoteArea[] pv = MeetingHud.Instance.playerStates;

                byte[] toKill = MeetingHud.Instance.playerStates.Where(x => x.VotedFor == player.PlayerId).Select(x => x.TargetPlayerId).ToArray();
                var pk = new PunishmentKill((x) =>
                {
                    Utils.RpcMultiMurderPlayer(player, x);
                }, (y) =>
                {
                    return toKill.Contains(y.PlayerId);
                });
                Coroutines.Start(pk.Open());
            }
        }
    }
}