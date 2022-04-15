using System;
using UnityEngine;
using TMPro;
using TownOfUs.Extensions;

namespace TownOfUs.Roles
{
    public class GuardianAngel : Role
    {
        public bool Enabled;
        public DateTime LastProtected;
        public float TimeRemaining;

        public int UsesLeft;
        public TextMeshPro UsesText;

        public bool ButtonUsable => UsesLeft != 0;

        public PlayerControl target;

        public GuardianAngel(PlayerControl player) : base(player)
        {
            Name = "Guardian Angel";
            ImpostorText = () =>
                target == null ? "You don't have a target for some reason... weird..." : $"Protect {target.name} with your life!";
            TaskText = () =>
                target == null
                    ? "You don't have a target for some reason... weird..."
                    : $"Protect {target.name}!";
            Color = Patches.Colors.GuardianAngel;
            LastProtected = DateTime.UtcNow;
            RoleType = RoleEnum.GuardianAngel;
            AddToRoleHistory(RoleType);
            Faction = Faction.Neutral;
            Scale = 1.4f;

            UsesLeft = CustomGameOptions.MaxProtects;
        }

        public bool Protecting => TimeRemaining > 0f;

        public float ProtectTimer()
        {
            var utcNow = DateTime.UtcNow;
            var timeSpan = utcNow - LastProtected;
            var num = CustomGameOptions.ProtectCd * 1000f;
            var flag2 = num - (float)timeSpan.TotalMilliseconds < 0f;
            if (flag2) return 0;
            return (num - (float)timeSpan.TotalMilliseconds) / 1000f;
        }

        public void Protect()
        {
            Enabled = true;
            TimeRemaining -= Time.deltaTime;
        }


        public void UnProtect()
        {
            Enabled = false;
            LastProtected = DateTime.UtcNow;
        }

        public void ImpTargetWin()
        {
            WinByRPC = true;
        }

        public void ImpTargetLose()
        {
            LostByRPC = true;
        }

        protected override void IntroPrefix(IntroCutscene._CoBegin_d__19 __instance, ref Il2CppSystem.Collections.Generic.List<PlayerControl> yourTeam)
        {
            var gaTeam = new Il2CppSystem.Collections.Generic.List<PlayerControl>();
            gaTeam.Add(PlayerControl.LocalPlayer);
            gaTeam.Add(target);
            yourTeam = gaTeam;
        }
    }
}