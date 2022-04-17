using System;

namespace TownOfUs.Roles
{
    public class Blackmailer : Role
    {
        public KillButton _blackmailButton;
        
        public PlayerControl ClosestPlayer;
        public PlayerControl Blackmailed;

        public Blackmailer(PlayerControl player) : base(player)
        {
            Name = "Blackmailer";
            ImpostorText = () => "Silence crewmates during meetings";
            TaskText = () => "Silence a crewmate for the next meeting";
            Color = Patches.Colors.Impostor;
            RoleType = RoleEnum.Blackmailer;
            AddToRoleHistory(RoleType);
            Faction = Faction.Impostors;
        }

        public KillButton BlackmailButton
        {
            get => _blackmailButton;
            set
            {
                _blackmailButton = value;
                ExtraButtons.Clear();
                ExtraButtons.Add(value);
            }
        }
    }
}