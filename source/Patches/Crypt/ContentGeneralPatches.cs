using HarmonyLib;
using Reactor.Utilities;
using System.Collections;

namespace TownOfUs.Patches.Crypt
{
    [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnGameJoined))]
    public static class HandleGameJoin
    {
        public static void Postfix()
        {
            Logger<TownOfUs>.Message("Attempting to join a game");
            if (!ContentAdditions.IApplyContent()) return;
            Logger<TownOfUs>.Message($"Sending content additions as {DestroyableSingleton<EOSManager>.Instance.FriendCode}");
            Coroutines.Start(WaitForId());
        }

        public static IEnumerator WaitForId()
        {
            while (PlayerControl.LocalPlayer == null) yield return null;
            if (AmongUsClient.Instance.AmHost) ExternalContentAdditions.contentAdditions.Add(ContentAdditions.resolvedContent);
            else Utils.Rpc(CustomRPC.ContentAddition, (byte)CustomCARPC.ApplyAdditions, PlayerControl.LocalPlayer.PlayerId, ContentAdditions.resolvedContent.gpgUnResolved);
        }
    }
}
