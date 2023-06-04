using HarmonyLib;
using TownOfUs.CrewmateRoles.ImitatorMod;
using TownOfUs.Roles;

namespace TownOfUs.NeutralRoles.DoomsayerMod
{
    [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Start))]
    public class MeetingStart
    {
        public static void Postfix(MeetingHud __instance)
        {
            if (PlayerControl.LocalPlayer.Data.IsDead) return;
            if (!PlayerControl.LocalPlayer.Is(RoleEnum.Doomsayer)) return;
            var doomsayerRole = Role.GetRole<Doomsayer>(PlayerControl.LocalPlayer);
            if (doomsayerRole.LastObservedPlayer != null)
            {
                var playerResults = PlayerReportFeedback(doomsayerRole.LastObservedPlayer);
                var roleResults = RoleReportFeedback(doomsayerRole.LastObservedPlayer);

                if (!string.IsNullOrWhiteSpace(playerResults)) DestroyableSingleton<HudManager>.Instance.Chat.AddChat(PlayerControl.LocalPlayer, playerResults);
                if (!string.IsNullOrWhiteSpace(roleResults)) DestroyableSingleton<HudManager>.Instance.Chat.AddChat(PlayerControl.LocalPlayer, roleResults);
            }
        }

        public static string PlayerReportFeedback(PlayerControl player)
        {
            if (player.Is(RoleEnum.Glitch) || player.Is(RoleEnum.Imitator) || StartImitate.ImitatingPlayer == player
                 || player.Is(RoleEnum.Morphling) || player.Is(RoleEnum.Spy) || player.Is(RoleEnum.Doomsayer))
                return $"You observe that {player.name} views the world through a different lens";
            else if (player.Is(RoleEnum.Altruist) || player.Is(RoleEnum.Amnesiac) || player.Is(RoleEnum.Janitor)
                 || player.Is(RoleEnum.Medium) || player.Is(RoleEnum.Undertaker))
                return $"You observe that {player.name} has an unusual obsession with dead bodies";
            else if (player.Is(RoleEnum.Grenadier) || player.Is(RoleEnum.GuardianAngel) || player.Is(RoleEnum.Medic)
                 || player.Is(RoleEnum.Survivor) || player.Is(RoleEnum.Veteran))
                return $"You observe that {player.name} tries to protect themselves or others by any means necessary";
            else if (player.Is(RoleEnum.Bomber) || player.Is(RoleEnum.Executioner) || player.Is(RoleEnum.Jester)
                 || player.Is(RoleEnum.Swapper) || player.Is(RoleEnum.Transporter))
                return $"You observe that {player.name} is a causer of chaos";
            else if (player.Is(RoleEnum.Blackmailer) || player.Is(RoleEnum.Mayor) || player.Is(RoleEnum.Snitch)
                 || player.Is(RoleEnum.Swooper) || player.Is(RoleEnum.Trapper))
                return $"You observe that {player.name} is concealing information";
            else if (player.Is(RoleEnum.Juggernaut) || player.Is(RoleEnum.Pestilence) || player.Is(RoleEnum.Sheriff)
                 || player.Is(RoleEnum.Traitor) || player.Is(RoleEnum.Vigilante) || player.Is(RoleEnum.Werewolf))
                return $"You observe that {player.name} started innocent but gained the capability to kill";
            else if (player.Is(RoleEnum.Arsonist) || player.Is(RoleEnum.Detective) || player.Is(RoleEnum.Plaguebearer)
                 || player.Is(RoleEnum.Seer) || player.Is(RoleEnum.Tracker))
                return $"You observe that {player.name} likes to interact with others";
            else if (player.Is(RoleEnum.Engineer) || player.Is(RoleEnum.Escapist) || player.Is(RoleEnum.Investigator)
                 || player.Is(RoleEnum.Miner) || player.Is(RoleEnum.Mystic))
                return $"You observe that {player.name} likes exploring";
            else if (player.Is(RoleEnum.Crewmate) || player.Is(RoleEnum.Impostor))
                return $"You observe that {player.name} appears to be roleless";
            else
                return "Error";
        }

        public static string RoleReportFeedback(PlayerControl player)
        {
            if (player.Is(RoleEnum.Glitch) || player.Is(RoleEnum.Imitator) || StartImitate.ImitatingPlayer == player
                 || player.Is(RoleEnum.Morphling) || player.Is(RoleEnum.Spy) || player.Is(RoleEnum.Doomsayer))
                return "(Doomsayer/Glitch/Imitator/Morphling/Spy)";
            else if (player.Is(RoleEnum.Altruist) || player.Is(RoleEnum.Amnesiac) || player.Is(RoleEnum.Janitor)
                 || player.Is(RoleEnum.Medium) || player.Is(RoleEnum.Undertaker))
                return "(Altruist/Amnesiac/Janitor/Medium/Undertaker)";
            else if (player.Is(RoleEnum.Grenadier) || player.Is(RoleEnum.GuardianAngel) || player.Is(RoleEnum.Medic)
                 || player.Is(RoleEnum.Survivor) || player.Is(RoleEnum.Veteran))
                return "(Grenadier/Guardian Angel/Medic/Survivor/Veteran)";
            else if (player.Is(RoleEnum.Bomber) || player.Is(RoleEnum.Executioner) || player.Is(RoleEnum.Jester)
                 || player.Is(RoleEnum.Swapper) || player.Is(RoleEnum.Transporter))
                return "(Bomber/Executioner/Jester/Swapper/Transporter)";
            else if (player.Is(RoleEnum.Blackmailer) || player.Is(RoleEnum.Mayor) || player.Is(RoleEnum.Snitch)
                 || player.Is(RoleEnum.Swooper) || player.Is(RoleEnum.Trapper))
                return "(Blackmailer/Mayor/Snitch/Swooper/Trapper)";
            else if (player.Is(RoleEnum.Juggernaut) || player.Is(RoleEnum.Pestilence) || player.Is(RoleEnum.Sheriff)
                 || player.Is(RoleEnum.Traitor) || player.Is(RoleEnum.Vigilante) || player.Is(RoleEnum.Werewolf))
                return "(Juggernaut/Pestilence/Sheriff/Traitor/Vigilante/Werewolf)";
            else if (player.Is(RoleEnum.Arsonist) || player.Is(RoleEnum.Detective) || player.Is(RoleEnum.Plaguebearer)
                 || player.Is(RoleEnum.Seer) || player.Is(RoleEnum.Tracker))
                return "(Arsonist/Detective/Plaguebearer/Seer/Tracker)";
            else if (player.Is(RoleEnum.Engineer) || player.Is(RoleEnum.Escapist) || player.Is(RoleEnum.Investigator)
                 || player.Is(RoleEnum.Miner) || player.Is(RoleEnum.Mystic))
                return "(Engineer/Escapist/Investigator/Miner/Mystic)";
            else if (player.Is(RoleEnum.Crewmate) || player.Is(RoleEnum.Impostor))
                return "(Crewmate/Impostor)";
            else return "Error";
        }
    }
}