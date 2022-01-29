using HarmonyLib;
using Hazel;
using TownOfUs.CrewmateRoles.InvestigatorMod;
using TownOfUs.CrewmateRoles.SnitchMod;
using TownOfUs.Roles;
using UnityEngine;
using System;
using Il2CppSystem.Collections.Generic;
using Object = UnityEngine.Object;

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
            List<PlayerTask> tasks1, tasks2;
            List<GameData.TaskInfo> taskinfos1, taskinfos2;

            var rememberImp = true;
            var rememberNeut = true;

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
                case RoleEnum.Vigilante:
                case RoleEnum.Veteran:
                case RoleEnum.Crewmate:
                case RoleEnum.Tracker:
                case RoleEnum.Haunter:
                case RoleEnum.Phantom:

                    rememberImp = false;
                    rememberNeut = false;

                    break;


                case RoleEnum.Jester:
                case RoleEnum.Executioner:
                case RoleEnum.Arsonist:
                case RoleEnum.Amnesiac:
                case RoleEnum.Glitch:
                case RoleEnum.Juggernaut:

                    rememberImp = false;

                    break;
            }

            if (role == RoleEnum.Investigator) Footprint.DestroyAll(Role.GetRole<Investigator>(other));

            newRole = Role.GetRole(other);
            newRole.Player = amnesiac;

            if (role == RoleEnum.Snitch) CompleteTask.Postfix(amnesiac);

            Role.RoleDictionary.Remove(amnesiac.PlayerId);
            if (!(role == RoleEnum.Haunter || role == RoleEnum.Phantom))
            {
                Role.RoleDictionary.Remove(other.PlayerId);
                Role.RoleDictionary.Add(amnesiac.PlayerId, newRole);
            }
            else
            {
                new Crewmate(amnesiac);
            }

            var snitch = role == RoleEnum.Snitch;
            var tracker = role == RoleEnum.Tracker;
            var seer = role == RoleEnum.Seer;
            var arso = role == RoleEnum.Arsonist;

            if (rememberImp == false && (!(role == RoleEnum.Haunter || role == RoleEnum.Phantom)))
            {
                if (rememberNeut == false)
                {
                    new Crewmate(other);
                }
                else
                {
                    new Jester(other);
                }
            }
            else if (rememberImp == true)
            {
                new Impostor(other);
                amnesiac.Data.IsImpostor = true;
                amnesiac.SetKillTimer(PlayerControl.GameOptions.KillCooldown);
                if (CustomGameOptions.AmneTurnAssassin)
                {
                    var writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId,
                        (byte)CustomRPC.SetAssassin, SendOption.Reliable, -1);
                    writer.Write(amnesiac.PlayerId);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                }
                if (amnesiac.Is(RoleEnum.Poisoner))
                {
                    if (PlayerControl.LocalPlayer == amnesiac)
                    {
                        foreach (var role2 in Role.GetRoles(RoleEnum.Poisoner))
                        {
                            var poisoner = (Poisoner)role2;
                            poisoner.LastPoisoned = DateTime.UtcNow;
                            if (PlayerControl.LocalPlayer.Is(RoleEnum.Poisoner))
                            {
                                DestroyableSingleton<HudManager>.Instance.KillButton.renderer.enabled = false;
                            }
                        }
                    }
                    else if (PlayerControl.LocalPlayer == other)
                    {
                        DestroyableSingleton<HudManager>.Instance.KillButton.enabled = true;
                        DestroyableSingleton<HudManager>.Instance.KillButton.renderer.enabled = true;
                    }
                }
            }

            tasks1 = other.myTasks;
            taskinfos1 = other.Data.Tasks;
            tasks2 = amnesiac.myTasks;
            taskinfos2 = amnesiac.Data.Tasks;

            amnesiac.myTasks = tasks1;
            amnesiac.Data.Tasks = taskinfos1;
            other.myTasks = tasks2;
            other.Data.Tasks = taskinfos2;

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

            if (tracker)
            {
                var trackerRole = Role.GetRole<Tracker>(amnesiac);
                trackerRole.Tracked.RemoveRange(0, trackerRole.Tracked.Count);
                trackerRole.TrackerArrows.DestroyAll();
                trackerRole.TrackerArrows.Clear();
                trackerRole.TrackerArrows.RemoveRange(0, trackerRole.TrackerArrows.Count);
                trackerRole.TrackerTargets.RemoveRange(0, trackerRole.TrackerTargets.Count);
            }

            if (seer)
            {
                var seerRole = Role.GetRole<Seer>(amnesiac);
                seerRole.Investigated.RemoveRange(0, seerRole.Investigated.Count);
            }

            if (arso)
            {
                var arsoRole = Role.GetRole<Arsonist>(amnesiac);
                arsoRole.DousedPlayers.RemoveRange(0, arsoRole.DousedPlayers.Count);
            }

            if (other.Is(RoleEnum.Crewmate))
            {
                var role2 = Role.GetRole<Crewmate>(other);
                role2.RegenTask();
            }
            else if (other.Is(RoleEnum.Jester))
            {
                var role2 = Role.GetRole<Jester>(other);
                role2.RegenTask();
            }
            else
            {
                var role2 = Role.GetRole<Impostor>(other);
                role2.RegenTask();
            }

            if (amnesiac.AmOwner || other.AmOwner)
            {
                foreach (var sheriffRole in Role.GetRoles(RoleEnum.Sheriff))
                {
                    var sheriff = (Sheriff)sheriffRole;
                    sheriff.LastKilled = DateTime.UtcNow;
                }

                foreach (var engiRole in Role.GetRoles(RoleEnum.Engineer))
                {
                    var engi = (Engineer)engiRole;
                    engi.UsedThisRound = false;
                }

                foreach (var medicRole in Role.GetRoles(RoleEnum.Medic))
                {
                    var medic = (Medic)medicRole;
                    medic.UsedAbility = false;
                }

                foreach (var mayorRole in Role.GetRoles(RoleEnum.Mayor))
                {
                    var mayor = (Mayor)mayorRole;
                    mayor.VoteBank = CustomGameOptions.MayorVoteBank;
                }

                foreach (var seerRole in Role.GetRoles(RoleEnum.Seer))
                {
                    var seer2 = (Seer)seerRole;
                    seer2.LastInvestigated = DateTime.UtcNow;
                }

                foreach (var trackerRole in Role.GetRoles(RoleEnum.Tracker))
                {
                    var tracker2 = (Tracker)trackerRole;
                    tracker2.LastTracked = DateTime.UtcNow;
                    tracker2.RemainingTracks = CustomGameOptions.MaxTracks;
                }

                foreach (var vetRole in Role.GetRoles(RoleEnum.Veteran))
                {
                    var veteran = (Veteran)vetRole;
                    veteran.LastAlerted = DateTime.UtcNow;
                    veteran.RemainingAlerts = CustomGameOptions.MaxAlerts;
                }

                foreach (var timelordRole in Role.GetRoles(RoleEnum.TimeLord))
                {
                    var timelord = (TimeLord)timelordRole;
                    timelord.FinishRewind = DateTime.UtcNow;
                    timelord.StartRewind = DateTime.UtcNow;
                    timelord.StartRewind = timelord.StartRewind.AddSeconds(-10.0f);
                }

                foreach (var arsoRole in Role.GetRoles(RoleEnum.Arsonist))
                {
                    var arso2 = (Arsonist)arsoRole;
                    arso2.LastDoused = DateTime.UtcNow;
                }

                foreach (var glitchRole in Role.GetRoles(RoleEnum.Glitch))
                {
                    var glitch = (Glitch)glitchRole;
                    glitch.LastHack = DateTime.UtcNow;
                    glitch.LastMimic = DateTime.UtcNow;
                    glitch.LastKill = DateTime.UtcNow;
                }

                foreach (var juggRole in Role.GetRoles(RoleEnum.Juggernaut))
                {
                    var jugg = (Juggernaut)juggRole;
                    jugg.JuggKills = 0;
                    jugg.LastKill = DateTime.UtcNow;
                }

                foreach (var camouflageRole in Role.GetRoles(RoleEnum.Camouflager))
                {
                    var camouflager = (Camouflager)camouflageRole;
                    camouflager.LastCamouflaged = DateTime.UtcNow;
                }

                foreach (var grenadierRole in Role.GetRoles(RoleEnum.Grenadier))
                {
                    var grenadier = (Grenadier)grenadierRole;
                    grenadier.LastFlashed = DateTime.UtcNow;
                }

                foreach (var morphlingRole in Role.GetRoles(RoleEnum.Morphling))
                {
                    var morphling = (Morphling)morphlingRole;
                    morphling.LastMorphed = DateTime.UtcNow;
                }

                foreach (var swooperRole in Role.GetRoles(RoleEnum.Swooper))
                {
                    var swooper = (Swooper)swooperRole;
                    swooper.LastSwooped = DateTime.UtcNow;
                }

                foreach (var undertakerRole in Role.GetRoles(RoleEnum.Undertaker))
                {
                    var undertaker = (Undertaker)undertakerRole;
                    undertaker.LastDragged = DateTime.UtcNow;
                }

                foreach (var minerRole in Role.GetRoles(RoleEnum.Miner))
                {
                    var miner = (Miner)minerRole;
                    miner.LastMined = DateTime.UtcNow;
                }

                DestroyableSingleton<HudManager>.Instance.KillButton.gameObject.SetActive(false);
                DestroyableSingleton<HudManager>.Instance.KillButton.isActive = false;
                

                Lights.SetLights();
            }
        }
    }
}
