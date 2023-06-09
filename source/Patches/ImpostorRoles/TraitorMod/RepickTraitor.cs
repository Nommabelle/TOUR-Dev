using HarmonyLib;
using System.Linq;
using Hazel;
using UnityEngine;

namespace TownOfUs.ImpostorRoles.TraitorMod
{
    [HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
    public class RepickTraitor
    {
        private static void Postfix(HudManager __instance)
        {
            if (PlayerControl.AllPlayerControls.Count <= 1) return;
            if (PlayerControl.LocalPlayer == null) return;
            if (PlayerControl.LocalPlayer.Data == null) return;
            if (PlayerControl.LocalPlayer != SetTraitor.WillBeTraitor) return;
            if (PlayerControl.LocalPlayer.Is(Faction.Impostors)) return;
            if (!PlayerControl.LocalPlayer.Data.IsDead && !PlayerControl.LocalPlayer.Is(RoleEnum.Vampire)) return;
            var toChooseFrom = PlayerControl.AllPlayerControls.ToArray().Where(x => x.Is(Faction.Crewmates) && !x.Is(RoleEnum.Mayor) &&
                !x.Is(ModifierEnum.Lover) && !x.Data.IsDead && !x.Data.Disconnected && !x.IsExeTarget()).ToList();
            if (toChooseFrom.Count == 0)
            {
                SetTraitor.WillBeTraitor = null;
                var writer2 = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId,
                (byte)CustomRPC.SetTraitor, SendOption.Reliable, -1);
                writer2.Write(byte.MaxValue);
                AmongUsClient.Instance.FinishRpcImmediately(writer2);
            }
            else
            {
                var rand = Random.RandomRangeInt(0, toChooseFrom.Count);
                var pc = toChooseFrom[rand];

                SetTraitor.WillBeTraitor = pc;

                var writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId,
                    (byte)CustomRPC.SetTraitor, SendOption.Reliable, -1);
                writer.Write(pc.PlayerId);
                AmongUsClient.Instance.FinishRpcImmediately(writer);
            }
            return;
        }
    }
}
