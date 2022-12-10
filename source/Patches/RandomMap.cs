using HarmonyLib;
using Hazel;
using System;
using TownOfUs.Patches;
using TownOfUs.CustomOption;
using AmongUs.GameOptions;

namespace TownOfUs
{
    [HarmonyPatch]
    class RandomMap
    {
        public static byte previousMap;
        public static float vision;
        public static int commonTasks;
        public static int shortTasks;
        public static int longTasks;

        [HarmonyPatch(typeof(GameStartManager), nameof(GameStartManager.BeginGame))]
        [HarmonyPrefix]
        public static bool Prefix(GameStartManager __instance)
        {
            if (AmongUsClient.Instance.AmHost)
            {
                previousMap = PlayerControl.GameOptions.MapId;
                vision = PlayerControl.GameOptions.CrewLightMod;
                commonTasks = PlayerControl.GameOptions.NumCommonTasks;
                shortTasks = PlayerControl.GameOptions.NumShortTasks;
                longTasks = PlayerControl.GameOptions.NumLongTasks;
                byte map = PlayerControl.GameOptions.MapId;
                if (CustomGameOptions.RandomMapEnabled)
                {
                    map = GetRandomMap();
                    PlayerControl.GameOptions.MapId = map;
                }
                PlayerControl.GameOptions.RoleOptions.SetRoleRate(RoleTypes.Scientist, 0, 0);
                PlayerControl.GameOptions.RoleOptions.SetRoleRate(RoleTypes.Engineer, 0, 0);
                PlayerControl.GameOptions.RoleOptions.SetRoleRate(RoleTypes.GuardianAngel, 0, 0);
                PlayerControl.GameOptions.RoleOptions.SetRoleRate(RoleTypes.Shapeshifter, 0, 0);
                var writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId,
                    (byte)CustomRPC.SetSettings, SendOption.Reliable, -1);
                writer.Write(map);
                AmongUsClient.Instance.FinishRpcImmediately(writer);
                if (CustomGameOptions.AutoAdjustSettings) AdjustSettings(map);
            }
            return true;
        }

        [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnGameEnd))]
        [HarmonyPostfix]
        public static void Postfix(AmongUsClient __instance)
        {
            if (__instance.AmHost)
            {
                if (CustomGameOptions.AutoAdjustSettings)
                {
                    if (CustomGameOptions.SmallMapHalfVision && vision != 0) PlayerControl.GameOptions.CrewLightMod = vision;
                    if (PlayerControl.GameOptions.MapId == 1) AdjustCooldowns(CustomGameOptions.SmallMapDecreasedCooldown);
                    if (PlayerControl.GameOptions.MapId >= 4) AdjustCooldowns(-CustomGameOptions.LargeMapIncreasedCooldown);
                }
                if (CustomGameOptions.RandomMapEnabled) PlayerControl.GameOptions.MapId = previousMap;
                if (!(commonTasks == 0 && shortTasks == 0 && longTasks == 0))
                {
                    PlayerControl.GameOptions.NumCommonTasks = commonTasks;
                    PlayerControl.GameOptions.NumShortTasks = shortTasks;
                    PlayerControl.GameOptions.NumLongTasks = longTasks;
                }
            }
        }

        public static byte GetRandomMap()
        {
            Random _rnd = new Random();
            float totalWeight = 0;
            totalWeight += CustomGameOptions.RandomMapSkeld;
            totalWeight += CustomGameOptions.RandomMapMira;
            totalWeight += CustomGameOptions.RandomMapPolus;
            totalWeight += CustomGameOptions.RandomMapAirship;
            if (SubmergedCompatibility.Loaded) totalWeight += CustomGameOptions.RandomMapSubmerged;

            if (totalWeight == 0) return PlayerControl.GameOptions.MapId;

            float randomNumber = _rnd.Next(0, (int)totalWeight);
            if (randomNumber < CustomGameOptions.RandomMapSkeld) return 0;
            randomNumber -= CustomGameOptions.RandomMapSkeld;
            if (randomNumber < CustomGameOptions.RandomMapMira) return 1;
            randomNumber -= CustomGameOptions.RandomMapMira;
            if (randomNumber < CustomGameOptions.RandomMapPolus) return 2;
            randomNumber -= CustomGameOptions.RandomMapPolus;
            if (randomNumber < CustomGameOptions.RandomMapAirship) return 4;
            randomNumber -= CustomGameOptions.RandomMapAirship;
            if (SubmergedCompatibility.Loaded && randomNumber < CustomGameOptions.RandomMapSubmerged) return 5;

            return PlayerControl.GameOptions.MapId;
        }

        public static void AdjustSettings(byte map)
        {
            if (map <= 1)
            {
                if (CustomGameOptions.SmallMapHalfVision) PlayerControl.GameOptions.CrewLightMod *= 0.5f;
                PlayerControl.GameOptions.NumShortTasks += CustomGameOptions.SmallMapIncreasedShortTasks;
                PlayerControl.GameOptions.NumLongTasks += CustomGameOptions.SmallMapIncreasedLongTasks;
            }
            if (map == 1) AdjustCooldowns(-CustomGameOptions.SmallMapDecreasedCooldown);
            if (map >= 4)
            {
                PlayerControl.GameOptions.NumShortTasks -= CustomGameOptions.LargeMapDecreasedShortTasks;
                PlayerControl.GameOptions.NumLongTasks -= CustomGameOptions.LargeMapDecreasedLongTasks;
                AdjustCooldowns(CustomGameOptions.LargeMapIncreasedCooldown);
            }
            return;
        }

        public static void AdjustCooldowns(float change)
        {
            Generate.InitialExamineCooldown.Set((float)Generate.InitialExamineCooldown.Value + change, false);
            Generate.SeerCooldown.Set((float)Generate.SeerCooldown.Value + change, false);
            Generate.TrackCooldown.Set((float)Generate.TrackCooldown.Value + change, false);
            Generate.TrapCooldown.Set((float)Generate.TrapCooldown.Value + change, false);
            Generate.SheriffKillCd.Set((float)Generate.SheriffKillCd.Value + change, false);
            Generate.AlertCooldown.Set((float)Generate.AlertCooldown.Value + change, false);
            Generate.TransportCooldown.Set((float)Generate.TransportCooldown.Value + change, false);
            Generate.ProtectCd.Set((float)Generate.ProtectCd.Value + change, false);
            Generate.VestCd.Set((float)Generate.VestCd.Value + change, false);
            Generate.DouseCooldown.Set((float)Generate.DouseCooldown.Value + change, false);
            Generate.InfectCooldown.Set((float)Generate.InfectCooldown.Value + change, false);
            Generate.PestKillCooldown.Set((float)Generate.PestKillCooldown.Value + change, false);
            Generate.MimicCooldownOption.Set((float)Generate.MimicCooldownOption.Value + change, false);
            Generate.HackCooldownOption.Set((float)Generate.HackCooldownOption.Value + change, false);
            Generate.GlitchKillCooldownOption.Set((float)Generate.GlitchKillCooldownOption.Value + change, false);
            Generate.RampageCooldown.Set((float)Generate.RampageCooldown.Value + change, false);
            Generate.GrenadeCooldown.Set((float)Generate.GrenadeCooldown.Value + change, false);
            Generate.MorphlingCooldown.Set((float)Generate.MorphlingCooldown.Value + change, false);
            Generate.SwoopCooldown.Set((float)Generate.SwoopCooldown.Value + change, false);
            Generate.PoisonCooldown.Set((float)Generate.PoisonCooldown.Value + change, false);
            Generate.MineCooldown.Set((float)Generate.MineCooldown.Value + change, false);
            Generate.DragCooldown.Set((float)Generate.DragCooldown.Value + change, false);
            Generate.EscapeCooldown.Set((float)Generate.EscapeCooldown.Value + change, false);
            Generate.JuggKillCooldown.Set((float)Generate.JuggKillCooldown.Value + change, false);
            Generate.ReviveCooldown.Set((float)Generate.ReviveCooldown.Value + change, false);
            Generate.WhisperCooldown.Set((float)Generate.WhisperCooldown.Value + change, false);
            PlayerControl.GameOptions.KillCooldown += change;
            if (change % 5 != 0)
            {
                if (change > 0) change -= 2.5f;
                else if (change < 0) change += 2.5f;
            }
            PlayerControl.GameOptions.EmergencyCooldown += (int)change;
            return;
        }
    }
}