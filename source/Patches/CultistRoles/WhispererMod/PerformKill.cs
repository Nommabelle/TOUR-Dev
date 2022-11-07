using HarmonyLib;
using Hazel;
using TownOfUs.Roles;
using TownOfUs.Roles.Cultist;
using UnityEngine;
using System.Collections.Generic;
using System;
using static StatsManager;

namespace TownOfUs.CultistRoles.WhispererMod
{
    [HarmonyPatch(typeof(KillButton), nameof(KillButton.DoClick))]
    public class Whisper
    {
        public static bool Prefix(KillButton __instance)
        {
            var flag = PlayerControl.LocalPlayer.Is(RoleEnum.Whisperer);
            if (!flag) return true;
            if (!PlayerControl.LocalPlayer.CanMove) return false;
            if (PlayerControl.LocalPlayer.Data.IsDead) return false;
            var role = Role.GetRole<Whisperer>(PlayerControl.LocalPlayer);
            if (__instance == role.WhisperButton)
            {
                if (__instance.isCoolingDown) return false;
                if (!__instance.isActiveAndEnabled) return false;
                if (role.WhisperTimer() != 0) return false;

                var flag2 = role.WhisperButton.isCoolingDown;
                if (flag2) return false;
                if (!__instance.enabled) return false;
                var closestPlayers = GetClosestPlayers(role.Player);
                var oldStats = role.PlayerConversion;
                role.PlayerConversion = new List<(PlayerControl, float)>();
                foreach (var conversionRate in oldStats)
                {
                    var player = conversionRate.Item1;
                    var stats = conversionRate.Item2;
                    if (closestPlayers.Contains(player))
                    {
                        stats -= role.WhisperConversion;
                    }
                    role.PlayerConversion.Add((player, stats));
                }
                CheckConversion(role);
                role.WhisperCount += 1;
                role.LastWhispered = DateTime.UtcNow;
                return false;
            }

            return true;
        }

        public static void CheckConversion(Whisperer role)
        {
            foreach (var playerConversion in role.PlayerConversion)
            {
                if (playerConversion.Item2 <= 0)
                {
                    Utils.Convert(playerConversion.Item1);
                    role.ConversionCount += 1;
                    role.WhisperConversion -= CustomGameOptions.DecreasedPercentagePerConversion;

                    var writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId,
                        (byte)CustomRPC.Convert, SendOption.Reliable, -1);
                    writer.Write(playerConversion.Item1.PlayerId);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                }
            }
        }

        public static Il2CppSystem.Collections.Generic.List<PlayerControl> GetClosestPlayers(PlayerControl player)
        {
            Il2CppSystem.Collections.Generic.List<PlayerControl> playerControlList = new Il2CppSystem.Collections.Generic.List<PlayerControl>();
            float flashRadius = CustomGameOptions.WhisperRadius * 5;
            Vector2 truePosition = player.GetTruePosition();
            Il2CppSystem.Collections.Generic.List<GameData.PlayerInfo> allPlayers = GameData.Instance.AllPlayers;
            for (int index = 0; index < allPlayers.Count; ++index)
            {
                GameData.PlayerInfo playerInfo = allPlayers[index];
                if (!playerInfo.Disconnected)
                {
                    Vector2 vector2 = new Vector2(playerInfo.Object.GetTruePosition().x - truePosition.x, playerInfo.Object.GetTruePosition().y - truePosition.y);
                    float magnitude = ((Vector2)vector2).magnitude;
                    if (magnitude <= flashRadius)
                    {
                        PlayerControl playerControl = playerInfo.Object;
                        playerControlList.Add(playerControl);
                    }
                }
            }
            return playerControlList;
        }
    }
}