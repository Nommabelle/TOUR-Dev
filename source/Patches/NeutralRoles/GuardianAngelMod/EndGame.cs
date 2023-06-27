using HarmonyLib;
using Hazel;
using TownOfUs.Roles;

namespace TownOfUs.NeutralRoles.GuardianAngelMod
{
    [HarmonyPatch(typeof(GameManager), nameof(GameManager.RpcEndGame))]
    public class EndGame
    {
        public static bool Prefix(GameManager __instance, [HarmonyArgument(0)] GameOverReason reason)
        {
            foreach (var role in Role.AllRoles)
                if (role.RoleType == RoleEnum.GuardianAngel && ((GuardianAngel)role).target.Is(Faction.Impostors))
                {
                    if (reason != GameOverReason.HumansByVote && reason != GameOverReason.HumansByTask)
                    {
                        ((GuardianAngel)role).ImpTargetWin();

                        Utils.Rpc(CustomRPC.GAImpWin);
                    }
                    else
                    {
                        ((GuardianAngel)role).ImpTargetLose();

                        Utils.Rpc(CustomRPC.GAImpLose);
                    }
                    return true;
                }
            return true;
        }
    }
}