using System;
using System.Collections.Generic;
using TMPro;

namespace TownOfUs.Roles
{
    public class Seer : Role
    {
        public List<byte> Investigated = new List<byte>();

        public int UsesLeft;
        public TextMeshPro UsesText;
        public bool UsedThisRound;

        public bool ButtonUsable => UsesLeft != 0 && (!UsedThisRound || !CustomGameOptions.RewindPerRound);

        public Seer(PlayerControl player) : base(player)
        {
            Name = "Seer";
            ImpostorText = () => "Investigate roles";
            TaskText = () => "Investigate roles and find the Impostor";
            Color = Patches.Colors.Seer;
            RoleType = RoleEnum.Seer;
            AddToRoleHistory(RoleType);
            UsesLeft = (int) CustomGameOptions.SeeMaxUses;
            if (UsesLeft == 0) UsesLeft = -1;
            UsedThisRound = false;
        }

        public PlayerControl ClosestPlayer;
        public DateTime LastInvestigated { get; set; }

        public float SeerTimer()
        {
            var utcNow = DateTime.UtcNow;
            var timeSpan = utcNow - LastInvestigated;
            var num = CustomGameOptions.SeerCd * 1000f;
            var flag2 = num - (float) timeSpan.TotalMilliseconds < 0f;
            if (flag2) return 0;
            return (num - (float) timeSpan.TotalMilliseconds) / 1000f;
        }
    }
}