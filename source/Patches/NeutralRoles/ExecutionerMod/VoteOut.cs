using HarmonyLib;
using Reactor.Utilities;
using System.Linq;
using TownOfUs.Patches.NeutralRoles;
using TownOfUs.Roles;

namespace TownOfUs.NeutralRoles.ExecutionerMod
{
    [HarmonyPatch(typeof(ExileController), nameof(ExileController.Begin))]
    internal class MeetingExiledEnd
    {
        private static void Postfix(ExileController __instance)
        {
            var exiled = __instance.exiled;
            if (exiled == null) return;
            var player = exiled.Object;

            foreach (var role in Role.GetRoles(RoleEnum.Executioner))
                if (player.PlayerId == ((Executioner)role).target.PlayerId)
                {
                    ((Executioner)role).Wins();

                    if (!CustomGameOptions.NeutralEvilWinEndsGame) return;
                    if (PlayerControl.LocalPlayer != player) return;

                    PlayerVoteArea[] pv = MeetingHud.Instance.playerStates;

                    byte[] toKill = MeetingHud.Instance.playerStates.Where(x => x.VotedFor == ((Executioner)role).target.PlayerId && x.TargetPlayerId != player.PlayerId).Select(x => x.TargetPlayerId).ToArray();
                    var pk = new PunishmentKill((x) => {
                        Utils.RpcMultiMurderPlayer(player, x);
                    }, (y) => {
                        return toKill.Contains(y.PlayerId);
                    });
                    Coroutines.Start(pk.Open());
                }
                    
        }
    }
}