using System;
using UnityEngine;
using TMPro;

namespace TownOfUs.Roles
{
    public class Survivor : Role
    {
        public bool Enabled;
        public DateTime LastVested;
        public float TimeRemaining;

        public int UsesLeft;
        public TextMeshPro UsesText;

        public bool ButtonUsable => UsesLeft != 0;


        public Survivor(PlayerControl player) : base(player)
        {
            Name = "Survivor";
            ImpostorText = () => "Do whatever it takes to live";
            TaskText = () => "Stay alive to win";
            Color = Patches.Colors.Survivor;
            LastVested = DateTime.UtcNow;
            RoleType = RoleEnum.Survivor;
            Faction = Faction.Neutral;
            AddToRoleHistory(RoleType);

            UsesLeft = CustomGameOptions.MaxVests;
        }

        public bool Vesting => TimeRemaining > 0f;

        public float VestTimer()
        {
            var utcNow = DateTime.UtcNow;
            var timeSpan = utcNow - LastVested;
            var num = CustomGameOptions.VestCd * 1000f;
            var flag2 = num - (float) timeSpan.TotalMilliseconds < 0f;
            if (flag2) return 0;
            return (num - (float) timeSpan.TotalMilliseconds) / 1000f;
        }

        public void Vest()
        {
            Enabled = true;
            TimeRemaining -= Time.deltaTime;
        }


        public void UnVest()
        {
            Enabled = false;
            LastVested = DateTime.UtcNow;
        }

        protected override void IntroPrefix(IntroCutscene._CoBegin_d__19 __instance, ref Il2CppSystem.Collections.Generic.List<PlayerControl> yourTeam)
        {
            var survTeam = new Il2CppSystem.Collections.Generic.List<PlayerControl>();
            survTeam.Add(PlayerControl.LocalPlayer);
            yourTeam = survTeam;
        }

        public void AliveImpWin()
        {
            WinByRPC = true;
        }

        public void DeadCrewWin()
        {
            LostByRPC = true;
        }
    }
}