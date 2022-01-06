using System.Linq;
using HarmonyLib;
using Il2CppSystem.Collections.Generic;
using Reactor;
using TownOfUs.Roles;
using TownOfUs.Roles.Modifiers;

namespace TownOfUs
{
    [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnGameEnd))]
    public class AmongUsClient_OnGameEnd
    {
        public static void Postfix(AmongUsClient __instance, [HarmonyArgument(0)] GameOverReason reason,
            [HarmonyArgument(0)] bool showAd)
        {
            Utils.potentialWinners.Clear();
            foreach (var player in PlayerControl.AllPlayerControls)
                Utils.potentialWinners.Add(new WinningPlayerData(player.Data));
        }
    }

    [HarmonyPatch(typeof(EndGameManager), nameof(EndGameManager.Start))]
    public class EndGameManager_SetEverythingUp
    {
        public static void Prefix()
        {
            if (Role.NobodyWins)
            {
                TempData.winners = new List<WinningPlayerData>();
                return;
            }
            foreach(var role in Role.AllRoles)
            {
                var type = role.RoleType;

                if(type == RoleEnum.Jester)
                {
                    var jester = (Jester)role;
                    if (jester.VotedOut) {
                        var winners = Utils.potentialWinners.Where(x => x.Name == jester.PlayerName).ToList();
                        TempData.winners = new List<WinningPlayerData>();
                        foreach (var win in winners)
                        {
                            win.IsDead = false;
                            TempData.winners.Add(win);
                        }

                        return;
                    }
                } 
                else if (type == RoleEnum.Executioner)
                {
                    var executioner = (Executioner) role;
                    if (executioner.TargetVotedOut)
                    {
                        var winners = Utils.potentialWinners.Where(x => x.Name == executioner.PlayerName).ToList();
                        TempData.winners = new List<WinningPlayerData>();
                        foreach (var win in winners) TempData.winners.Add(win);
                        return;
                    }
                }
                else if (type == RoleEnum.Glitch)
                {
                    var glitch = (Glitch) role;
                    if (glitch.GlitchWins)
                    {
                        var winners = Utils.potentialWinners.Where(x => x.Name == glitch.PlayerName).ToList();
                        TempData.winners = new List<WinningPlayerData>();
                        foreach (var win in winners) TempData.winners.Add(win);
                        return;
                    }
                }
                else if (type == RoleEnum.Juggernaut)
                {
                    var juggernaut = (Juggernaut)role;
                    if (juggernaut.JuggernautWins)
                    {
                        var winners = Utils.potentialWinners.Where(x => x.Name == juggernaut.PlayerName).ToList();
                        TempData.winners = new List<WinningPlayerData>();
                        foreach (var win in winners) TempData.winners.Add(win);
                        return;
                    }
                }
                else if (type == RoleEnum.Arsonist)
                {
                    var arsonist = (Arsonist)role;
                    if (arsonist.ArsonistWins)
                    {
                        var winners = Utils.potentialWinners.Where(x => x.Name == arsonist.PlayerName).ToList();
                        TempData.winners = new List<WinningPlayerData>();
                        foreach (var win in winners) TempData.winners.Add(win);
                        return;
                    }
                }
                else if (type == RoleEnum.Phantom)
                {
                    var phantom = (Phantom)role;
                    if(phantom.CompletedTasks)
                    {
                        var winners = Utils.potentialWinners.Where(x => x.Name == phantom.PlayerName).ToList();
                        TempData.winners = new List<WinningPlayerData>();
                        foreach (var win in winners) TempData.winners.Add(win);
                    }
                }
            }

            foreach(var modifier in Modifier.AllModifiers)
            {
                var type = modifier.ModifierType;

                if(type == ModifierEnum.Lover)
                {

                    var lover = (Lover)modifier;
                    if (lover.LoveCoupleWins)
                    {
                        var otherLover = lover.OtherLover;
                        List<WinningPlayerData> winners = new List<WinningPlayerData>();
                        foreach(var player in Utils.potentialWinners){
                            if (player.Name == lover.PlayerName ||
                               player.Name == otherLover.PlayerName) winners.Add(player);
                        }
                        TempData.winners = new List<WinningPlayerData>();
                        foreach (var win in winners) TempData.winners.Add(win);
                        return;
                    }

                }
            }
        }
    }
}
