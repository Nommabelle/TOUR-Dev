using HarmonyLib;
using Hazel;
using TownOfUs.CrewmateRoles.InvestigatorMod;
using TownOfUs.CrewmateRoles.SnitchMod;
using TownOfUs.Roles;
using UnityEngine;

namespace TownOfUs.NeutralRoles.AmnesiacMod
{
    [HarmonyPatch(typeof(KillButtonManager), nameof(KillButtonManager.PerformKill))]
    [HarmonyPriority(Priority.Last)]
    public class PerformKillButton

    {
        public static bool Prefix(KillButtonManager __instance)
        {
            if (__instance != DestroyableSingleton<HudManager>.Instance.KillButton) return true;
            var flag = PlayerControl.LocalPlayer.Is(RoleEnum.Amnesiac);
            if (!flag) return true;
            if (!PlayerControl.LocalPlayer.CanMove) return false;
            if (PlayerControl.LocalPlayer.Data.IsDead) return false;
            var role = Role.GetRole<Amnesiac>(PlayerControl.LocalPlayer);

            var flag2 = __instance.isCoolingDown;
            if (flag2) return false;
            if (!__instance.enabled) return false;
            var maxDistance = GameOptionsData.KillDistances[PlayerControl.GameOptions.KillDistance];
            if (role == null)
                return false;
            if (role.CurrentTarget == null)
                return false;
            if (Vector2.Distance(role.CurrentTarget.TruePosition,
                PlayerControl.LocalPlayer.GetTruePosition()) > maxDistance) return false;
            var playerId = role.CurrentTarget.ParentId;
            var player = Utils.PlayerById(playerId);

            var writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId,
                (byte) CustomRPC.Remember, SendOption.Reliable, -1);
            writer.Write(PlayerControl.LocalPlayer.PlayerId);
            writer.Write(playerId);
            AmongUsClient.Instance.FinishRpcImmediately(writer);

            Remember(role, player);
            return false;
        }

        public static void Remember(Amnesiac amneRole, PlayerControl other)
        {
            var role = Utils.GetRole(other);
            var amnesiac = amneRole.Player;

            var rememberImp = true;

            Role newRole;

            switch (role)
            {
                case RoleEnum.Sheriff:
                case RoleEnum.Engineer:
                case RoleEnum.Mayor:
                case RoleEnum.Swapper:
                case RoleEnum.Investigator:
                case RoleEnum.TimeLord:
                case RoleEnum.Medic:
                case RoleEnum.Seer:
                case RoleEnum.Spy:
                case RoleEnum.Snitch:
                case RoleEnum.Altruist:
                case RoleEnum.Retributionist:
                case RoleEnum.Veteran:
                case RoleEnum.Crewmate:
                case RoleEnum.Jester:
                case RoleEnum.Executioner:
                case RoleEnum.Arsonist:
                case RoleEnum.Amnesiac:
                case RoleEnum.Glitch:
                case RoleEnum.Juggernaut:
                case RoleEnum.Tracker:

                    rememberImp = false;

                    break;
            }

            if (role == RoleEnum.Investigator) Footprint.DestroyAll(Role.GetRole<Investigator>(other));


            newRole = Role.GetRole(other);
            newRole.Player = amnesiac;

            if (role == RoleEnum.Snitch) CompleteTask.Postfix(amnesiac);

            Role.RoleDictionary.Remove(amnesiac.PlayerId);
            Role.RoleDictionary.Remove(other.PlayerId);
            Role.RoleDictionary.Add(amnesiac.PlayerId, newRole);

            var snitch = role == RoleEnum.Snitch;

            if (rememberImp == false)
            {
                new Crewmate(other);
            }
            else
            {
                new Impostor(other);
                amnesiac.Data.IsImpostor = true;
            }

            if (snitch)
            {
                var snitchRole = Role.GetRole<Snitch>(amnesiac);
                snitchRole.ImpArrows.DestroyAll();
                snitchRole.SnitchArrows.DestroyAll();
                snitchRole.SnitchTargets.Clear();
                CompleteTask.Postfix(amnesiac);
                if (other.AmOwner)
                    foreach (var player in PlayerControl.AllPlayerControls)
                        player.nameText.color = Color.white;
            }

            amneRole.RegenTask();

            if (amnesiac.AmOwner || other.AmOwner)
            {
                DestroyableSingleton<HudManager>.Instance.KillButton.gameObject.SetActive(false);
                DestroyableSingleton<HudManager>.Instance.KillButton.isActive = false;

                Lights.SetLights();
            }
        }
    }
}
