using System;
using UnityEngine;

namespace TownOfUs.Roles
{
    public class Amnesiac : Role
    {
        public Amnesiac(PlayerControl player) : base(player)
        {
            Name = "Amnesiac";
            ImpostorText = () => "Remember a role of a deceased player";
            TaskText = () => "Remember who you were.\nFake Tasks:";
            Color = Patches.Colors.Amnesiac;
            RoleType = RoleEnum.Amnesiac;
            Faction = Faction.Neutral;
        }

        public DeadBody CurrentTarget;

        public void Loses()
        {
            Player.Data.IsImpostor = true;
        }
    }
}