using System.Linq;
using HarmonyLib;
using TownOfUs.Roles;

namespace TownOfUs.Patches
{
    [HarmonyPatch(typeof(EndGameManager), nameof(EndGameManager.Start))]
    public static class NEWin
    {
        public static void Postfix(EndGameManager __instance)
        {
            if (CustomGameOptions.NeutralEvilWinEndsGame) return;
            var doomRole = Role.AllRoles.FirstOrDefault(x => x.RoleType == RoleEnum.Doomsayer && ((Doomsayer)x).WonByGuessing && ((Doomsayer)x).Player == PlayerControl.LocalPlayer);
            if (doomRole != null)
            {
                __instance.BackgroundBar.material.color = doomRole.Color;
                __instance.WinText.text = "</color><color=#00FF80FF>Victory";
            }
            var exeRole = Role.AllRoles.FirstOrDefault(x => x.RoleType == RoleEnum.Executioner && ((Executioner)x).TargetVotedOut && ((Executioner)x).Player == PlayerControl.LocalPlayer);
            if (exeRole != null)
            {
                __instance.BackgroundBar.material.color = exeRole.Color;
                __instance.WinText.text = "</color><color=#8C4005FF>Victory";
            }
            var jestRole = Role.AllRoles.FirstOrDefault(x => x.RoleType == RoleEnum.Jester && ((Jester)x).VotedOut && ((Jester)x).Player == PlayerControl.LocalPlayer);
            if (jestRole != null)
            {
                __instance.BackgroundBar.material.color = jestRole.Color;
                __instance.WinText.text = "</color><color=#FFBFCCFF>Victory";
            }
            var phantomRole = Role.AllRoles.FirstOrDefault(x => x.RoleType == RoleEnum.Phantom && ((Phantom)x).CompletedTasks && ((Phantom)x).Player == PlayerControl.LocalPlayer);
            if (phantomRole != null)
            {
                __instance.BackgroundBar.material.color = phantomRole.Color;
                __instance.WinText.text = "</color><color=#662962FF>Victory";
            }
        }
    }
}