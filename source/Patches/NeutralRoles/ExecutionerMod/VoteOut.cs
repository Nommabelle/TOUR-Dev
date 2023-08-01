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

                    if (CustomGameOptions.NeutralEvilWinEndsGame || !CustomGameOptions.ExecutionerTorment) return;
                    if (PlayerControl.LocalPlayer != ((Executioner)role).Player) return;

                    byte[] toKill = MeetingHud.Instance.playerStates.Where(x => x.VotedFor == ((Executioner)role).target.PlayerId).Select(x => x.TargetPlayerId).ToArray();
                    var pk = new PunishmentKill((x) => {
                        Utils.RpcMultiMurderPlayer(((Executioner)role).Player, x);
                    }, (y) => {
                        return toKill.Contains(y.PlayerId);
                    });
                    Coroutines.Start(pk.Open());
                }
                    
        }
    }
}