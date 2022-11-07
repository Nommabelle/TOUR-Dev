using System;
using HarmonyLib;
using TownOfUs.Roles;
using TownOfUs.Roles.Cultist;

namespace TownOfUs.CultistRoles.SeerMod
{
    [HarmonyPatch(typeof(IntroCutscene._CoBegin_d__19), nameof(IntroCutscene._CoBegin_d__19.MoveNext))]
    public static class Start
    {
        public static void Postfix(IntroCutscene._CoBegin_d__19 __instance)
        {
            foreach (var role in Role.GetRoles(RoleEnum.CultistSeer))
            {
                var seer = (CultistSeer) role;
                seer.LastInvestigated = DateTime.UtcNow;
                seer.LastInvestigated = seer.LastInvestigated.AddSeconds(CustomGameOptions.InitialCooldowns - CustomGameOptions.SeerCd);
            }
        }
    }
}