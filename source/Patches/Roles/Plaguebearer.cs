using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Hazel;
using Reactor;

namespace TownOfUs.Roles
{
    public class Plaguebearer : Role
    {
        public PlayerControl ClosestPlayer;
        public List<byte> InfectedPlayers = new List<byte>();
        public DateTime LastInfected;

        public int InfectedAlive => InfectedPlayers.Count(x => Utils.PlayerById(x) != null && Utils.PlayerById(x).Data != null && !Utils.PlayerById(x).Data.IsDead);
        public bool CanTransform => PlayerControl.AllPlayerControls.ToArray().Count(x => x != null && x != null && !x.Data.IsDead) <= InfectedAlive;

        public Plaguebearer(PlayerControl player) : base(player)
        {
            Name = "Plaguebearer";
            ImpostorText = () => "Infect everyone to become Pestilence";
            TaskText = () => "Infect everyone to become Pestilence\nFake Tasks:";
            Color = Patches.Colors.Plaguebearer;
            RoleType = RoleEnum.Plaguebearer;
            AddToRoleHistory(RoleType);
            Faction = Faction.Neutral;
            InfectedPlayers.Add(player.PlayerId);
        }

        public void Loses()
        {
            LostByRPC = true;
        }

        protected override void IntroPrefix(IntroCutscene._ShowTeam_d__21 __instance)
        {
            var plaguebearerTeam = new Il2CppSystem.Collections.Generic.List<PlayerControl>();
            plaguebearerTeam.Add(PlayerControl.LocalPlayer);
            __instance.teamToShow = plaguebearerTeam;
        }

        public float InfectTimer()
        {
            var utcNow = DateTime.UtcNow;
            var timeSpan = utcNow - LastInfected;
            var num = CustomGameOptions.InfectCd * 1000f;
            var flag2 = num - (float)timeSpan.TotalMilliseconds < 0f;
            if (flag2) return 0;
            return (num - (float)timeSpan.TotalMilliseconds) / 1000f;
        }

        public void RpcSpreadInfection(PlayerControl source, PlayerControl target)
        {
            if (InfectedPlayers.Contains(source.PlayerId))
            {
                InfectedPlayers.Add(target.PlayerId);
                var writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId,
                    (byte)CustomRPC.Infect, SendOption.Reliable, -1);
                writer.Write(Player.PlayerId);
                writer.Write(target.PlayerId);
                AmongUsClient.Instance.FinishRpcImmediately(writer);
            }
            else if (InfectedPlayers.Contains(target.PlayerId))
            {
                InfectedPlayers.Add(source.PlayerId);
                var writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId,
                    (byte)CustomRPC.Infect, SendOption.Reliable, -1);
                writer.Write(Player.PlayerId);
                writer.Write(source.PlayerId);
                AmongUsClient.Instance.FinishRpcImmediately(writer);
            }
        }

        public void TurnPestilence()
        {
            RoleDictionary.Remove(Player.PlayerId);
            var role = new Pestilence(Player);
            if (Player == PlayerControl.LocalPlayer)
            {
                Coroutines.Start(Utils.FlashCoroutine(Patches.Colors.Pestilence));
                role.RegenTask();
            }
        }
    }
}