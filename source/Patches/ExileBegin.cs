using HarmonyLib;
using TownOfUs.Roles;

namespace TownOfUs.Patches
{
    [HarmonyPatch(typeof(ExileController), nameof(ExileController.Begin))]
    internal class ExileBegin
    {
        private static void Postfix(ExileController __instance)
        {
            var exiled = __instance.exiled;
            if (exiled == null) return;
            var player = exiled.Object;

            foreach (var role in Role.GetRoles(RoleEnum.Executioner))
                if (player.PlayerId == ((Executioner)role).target.PlayerId)
                    ((Executioner)role).Wins();

            foreach (var role in Role.GetRoles(RoleEnum.Jester))
                if (player.PlayerId == ((Jester)role).Player.PlayerId)
                    ((Jester)role).Wins();
        }
    }
}