using HarmonyLib;
using Hazel;
using TownOfUs.Roles;

namespace TownOfUs.CrewmateRoles.ProsecutorMod
{
    [HarmonyPatch(typeof(PlayerVoteArea))]
    public class AllowExtraVotes
    {
        [HarmonyPatch(typeof(PlayerVoteArea), nameof(PlayerVoteArea.VoteForMe))]
        public static class VoteForMe
        {
            public static bool Prefix(PlayerVoteArea __instance)
            {
                if (!PlayerControl.LocalPlayer.Is(RoleEnum.Prosecutor)) return true;
                var role = Role.GetRole<Prosecutor>(PlayerControl.LocalPlayer);
                if (__instance.Parent.state == MeetingHud.VoteStates.Proceeding ||
                    __instance.Parent.state == MeetingHud.VoteStates.Results)
                    return false;

                if (__instance != role.Prosecute) return true;
                else
                {
                    role.ProsecuteThisMeeting = true;
                    MeetingHud.Instance.SkipVoteButton.gameObject.SetActive(false);
                    AddProsecute.UpdateButton(role, MeetingHud.Instance);
                    var writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId,
                    (byte)CustomRPC.Prosecute, SendOption.Reliable, -1);
                    writer.Write(role.Player.PlayerId);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    return false;
                }
            }
        }
    }
}