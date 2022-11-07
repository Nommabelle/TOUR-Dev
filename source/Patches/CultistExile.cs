using HarmonyLib;
using System.Linq;
using TownOfUs.Extensions;

namespace TownOfUs.Patches
{
    [HarmonyPatch(typeof(AirshipExileController), nameof(AirshipExileController.WrapUpAndSpawn))]
    public static class AirshipExileController_WrapUpAndSpawn
    {
        public static void Postfix(AirshipExileController __instance) => CultistExilePatch.ExileControllerPostfix(__instance);
    }
    
    [HarmonyPatch(typeof(ExileController), nameof(ExileController.WrapUp))]
    public class CultistExilePatch
    {

        public static void ExileControllerPostfix(ExileController __instance)
        {
            var exiled = __instance.exiled?.Object;
            var cultist = PlayerControl.AllPlayerControls.ToArray()
                    .Where(x => x.Is(RoleEnum.Necromancer) || x.Is(RoleEnum.Whisperer)).ToList();
            foreach (var cult in cultist)
            {
                if (cult.Data.IsDead)
                {
                    foreach (var player in PlayerControl.AllPlayerControls)
                    {
                        if (player.Data.IsImpostor()) Utils.MurderPlayer(player, player);
                    }
                }
            }
            if (exiled == null) return;
            if (exiled.Is(RoleEnum.Necromancer) || exiled.Is(RoleEnum.Whisperer))
            {
                var alives = PlayerControl.AllPlayerControls.ToArray()
                        .Where(x => !x.Data.IsDead && !x.Data.Disconnected).ToList();
                foreach (var player in alives)
                {
                    if (player.Data.IsImpostor()) Utils.MurderPlayer(player, player);
                }
            }
        }

        public static void Postfix(ExileController __instance) => ExileControllerPostfix(__instance);
    }
}