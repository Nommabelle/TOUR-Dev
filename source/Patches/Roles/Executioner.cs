using Il2CppSystem.Collections.Generic;
using UnityEngine;

namespace TownOfUs.Roles
{
    public class Executioner : Role
    {
        public PlayerControl target;
        public bool TargetVotedOut;

        public Executioner(PlayerControl player) : base(player)
        {
            Name = "Executioner";
            ImpostorText = () =>
                target == null ? "You don't have a target for some reason... weird..." : $"Vote {target.name} out";
            TaskText = () =>
                target == null
                    ? "You don't have a target for some reason... weird..."
                    : $"Vote {target.name} out\nFake Tasks:";
            Color = Patches.Colors.Executioner;
            RoleType = RoleEnum.Executioner;
            Faction = Faction.Neutral;
            Scale = 1.4f;
        }

        internal override bool EABBNOODFGL(ShipStatus __instance)
        {
            if (Player.Data.IsDead) return true;
            if (!TargetVotedOut || !target.Data.IsDead) return true;
            Utils.EndGame();
            return false;
        }

        public void Wins()
        {
            if (Player.Data.IsDead || Player.Data.Disconnected) return;
            TargetVotedOut = true;
        }

        public void Loses()
        {
            Player.Data.IsImpostor = true;
        }
    }
}