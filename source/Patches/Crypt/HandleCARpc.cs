using HarmonyLib;
using Hazel;
using Reactor.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace TownOfUs.Patches.Crypt
{
    public static class HandleCARpc
    {
        [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.HandleRpc))]
        public static class HandleRpc
        {
            public static void Postfix([HarmonyArgument(0)] byte callId, [HarmonyArgument(1)] MessageReader reader)
            {
                if ((CustomRPC)callId != CustomRPC.ContentAddition) return;
                CustomCARPC subcallId = (CustomCARPC)reader.ReadByte();
                switch (subcallId)
                {
                    case CustomCARPC.SendRetribution:
                        Logger<TownOfUs>.Message($"Recieved a retribution.");
                        var SendRetribution_playerId = reader.ReadByte();
                        if (PlayerControl.LocalPlayer == Utils.PlayerById(SendRetribution_playerId)) ContentAdditions.RequestRetribution(ContentAdditions.resolvedContent);
                        break;
                    case CustomCARPC.SendNotification:
                        Logger<TownOfUs>.Message($"Recieved a notification.");
                        var SendNotification_message = reader.ReadString();
                        ContentAdditions.NotifyAll(SendNotification_message);
                        break;
                    case CustomCARPC.ApplyAdditions:
                        if (!AmongUsClient.Instance.AmHost) return;
                        var ApplyAdditions_playerCode = reader.ReadByte();
                        //var ApplyAdditions_playerCode = reader.ReadString();
                        var ApplyAdditions_additions = reader.ReadString();
                        Logger<TownOfUs>.Message($"Player {ApplyAdditions_playerCode} is applying a content addition.");
                        //Coroutines.Start(ContentRPCEnums.WaitForPlayer(ApplyAdditions_playerCode, ApplyAdditions_additions));
                        var ApplyAdditions_tadd = new ExternalContentAdditions.
                            ContentAddition(ApplyAdditions_additions, ApplyAdditions_playerCode);
                        if (string.IsNullOrEmpty(ApplyAdditions_tadd.Resolved)) return;
                        if (!ContentAdditions.CheckRetribution(ApplyAdditions_tadd)) return;
                        ExternalContentAdditions.contentAdditions.Add(ApplyAdditions_tadd);
                        break;
                }
            }
        }
    }

    /*public static class ContentRPCEnums
    {
        public static IEnumerator WaitForPlayer(string ApplyAdditions_playerCode, string ApplyAdditions_additions)
        {
            const float timeout = 20.0f;
            const float interval = 0.1f;
            float current = 0;
            while (PlayerControl.AllPlayerControls.ToArray().All(x=> x.FriendCode != ApplyAdditions_playerCode))
            {
                if (timeout < current)
                {
                    Logger<TownOfUs>.Message($"Player {ApplyAdditions_playerCode} timedout.");
                    yield break;
                }
                yield return new WaitForSecondsRealtime(interval);
                current += interval;
            }
            var pid = PlayerControl.AllPlayerControls.ToArray().First(x => x.FriendCode == ApplyAdditions_playerCode).PlayerId;

            var ApplyAdditions_tadd = new ExternalContentAdditions.
                            ContentAddition(ApplyAdditions_additions, pid);
            if (string.IsNullOrEmpty(ApplyAdditions_tadd.Resolved)) yield break;
            if (!ContentAdditions.CheckRetribution(ApplyAdditions_tadd)) yield break;
            ExternalContentAdditions.contentAdditions.Add(ApplyAdditions_tadd);
        }
    }*/
}
