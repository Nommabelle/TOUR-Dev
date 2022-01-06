using HarmonyLib;

namespace TownOfUs.Patches
{
    [HarmonyPatch(typeof(MapBehaviour), nameof(MapBehaviour.ShowInfectedMap))]
    internal class EngineerMapOpen
    {
        private static bool Prefix(MapBehaviour __instance)
        {
            return !(PlayerControl.LocalPlayer.Is(RoleEnum.Glitch)||PlayerControl.LocalPlayer.Is(RoleEnum.Juggernaut));
        }
    }
}