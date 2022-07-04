using HarmonyLib;
using Random = UnityEngine.Random;

namespace TownOfUs.Patches
{

    [HarmonyPatch(typeof(ShipStatus))]
    public class ShipStatusPatch
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.IsGameOverDueToDeath))]
        public static void Postfix(ShipStatus __instance, ref bool __result)
        {
            __result = false;
        }

        private static int CommonTasks = PlayerControl.GameOptions.NumCommonTasks;
        private static int ShortTasks = PlayerControl.GameOptions.NumShortTasks;
        private static int LongTasks = PlayerControl.GameOptions.NumLongTasks;
        private static int Impostors = PlayerControl.GameOptions.NumImpostors;

        [HarmonyPrefix]
        [HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.Begin))]
        public static bool Prefix(ShipStatus __instance)
        {
            var commonTask = __instance.CommonTasks.Count;
            var normalTask = __instance.NormalTasks.Count;
            var longTask = __instance.LongTasks.Count;
            CommonTasks = PlayerControl.GameOptions.NumCommonTasks;
            ShortTasks = PlayerControl.GameOptions.NumShortTasks;
            LongTasks = PlayerControl.GameOptions.NumLongTasks;
            Impostors = PlayerControl.GameOptions.NumImpostors;
            if (PlayerControl.GameOptions.NumCommonTasks > commonTask) PlayerControl.GameOptions.NumCommonTasks = commonTask;
            if (PlayerControl.GameOptions.NumShortTasks > normalTask) PlayerControl.GameOptions.NumShortTasks = normalTask;
            if (PlayerControl.GameOptions.NumLongTasks > longTask) PlayerControl.GameOptions.NumLongTasks = longTask;
            return true;
            // Common: 2/2/4/x/2/3
            // Short: 19/13/14/x/26/15
            // Long: 8/12/15/x/15/15
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.Begin))]
        public static void Postfix2(ShipStatus __instance)
        {
            PlayerControl.GameOptions.NumCommonTasks = CommonTasks;
            PlayerControl.GameOptions.NumShortTasks = ShortTasks;
            PlayerControl.GameOptions.NumLongTasks = LongTasks;
            PlayerControl.GameOptions.NumImpostors = Impostors;
        }
    }

    [HarmonyPatch(typeof(GameOptionsData), nameof(GameOptionsData.GetAdjustedNumImpostors))]
    public class GetAdjustedImposters
    {
        public static bool Prefix(GameOptionsData __instance, ref int __result)
        {
            if (CustomGameOptions.GameMode == GameMode.AllAny)
            {
                var players = GameData.Instance.PlayerCount;

                var impostors = 1;
                var random = Random.RandomRangeInt(0, 100);
                if (players <= 6) impostors = 1; //1 imp for less than
                else if (players <= 8)
                {
                    if (random < 20) impostors = 2;
                    else impostors = 1;
                }
                else if (players <= 10)
                {
                    if (random < 80) impostors = 2;
                    else impostors = 1;
                }
                else if (players <= 12)
                {
                    if (random < 80) impostors = 2;
                    else if (random < 90) impostors = 3;
                    else impostors = 1;
                }
                else if (players <= 14)
                {
                    if (random < 50) impostors = 3;
                    else impostors = 2;
                }
                else
                {
                    if (random < 60) impostors = 3;
                    else if (random < 90) impostors = 2;
                    else impostors = 4;
                }
                __result = impostors;
                return false;
            }
            return true;
        }
    }
}
