using System;
using HarmonyLib;
using Hazel;
using TownOfUs.Roles;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;
using System.Linq;

namespace TownOfUs.NeutralRoles.PhantomMod
{
    [HarmonyPatch(typeof(AirshipExileController), nameof(AirshipExileController.WrapUpAndSpawn))]
    public static class AirshipExileController_WrapUpAndSpawn
    {
        public static void Postfix(AirshipExileController __instance) => SetPhantom.ExileControllerPostfix(__instance);
    }
    
    [HarmonyPatch(typeof(ExileController), nameof(ExileController.WrapUp))]
    public class SetPhantom
    {
        public static bool PhantomSpawned = false;
        public static PlayerControl WillBePhantom;
        public static Vector2 StartPosition;

        public static void ExileControllerPostfix(ExileController __instance)
        {
            FindVent();
            var exiled = __instance.exiled?.Object;
            var possiblePhantoms = PlayerControl.AllPlayerControls.ToArray()
                    .Where(x => (x == exiled || x.Data.IsDead && !exiled.Is(RoleEnum.Jester)) && !x.Data.Disconnected &&
                    !x.Is(ModifierEnum.Lover) && !x.Is(Faction.Crewmates) && !x.Is(RoleEnum.Glitch) &&
                    !x.Is(RoleEnum.Juggernaut) && !x.Is(Faction.Impostors)).ToList();
            if (possiblePhantoms.Count == 0) return;
            if (!RpcHandling.PhantomOn) return;
            if (PhantomSpawned == true) return;

            if (AmongUsClient.Instance.AmHost)
            {
                var rand = Random.RandomRangeInt(0, possiblePhantoms.Count);
                WillBePhantom = possiblePhantoms[rand];
                PhantomSpawned = true;

                Role.RoleDictionary.Remove(WillBePhantom.PlayerId);
                var role = new Phantom(WillBePhantom);
                role.RegenTask();
                Lights.SetLights();

                RemoveTasks(WillBePhantom);
                if (!PlayerControl.LocalPlayer.Is(RoleEnum.Haunter))
                {
                    WillBePhantom.MyPhysics.ResetMoveState();
                }

                System.Console.WriteLine("Become Phantom - Phantom");

                WillBePhantom.gameObject.layer = LayerMask.NameToLayer("Players");

                var writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId,
                    (byte)CustomRPC.PhantomDied, SendOption.Reliable, -1);
                writer.Write(WillBePhantom.PlayerId);
                AmongUsClient.Instance.FinishRpcImmediately(writer);
            }
        }

        public static void Postfix(ExileController __instance) => ExileControllerPostfix(__instance);

        public static void RemoveTasks(PlayerControl player)
        {
            var totalTasks = PlayerControl.GameOptions.NumCommonTasks + PlayerControl.GameOptions.NumLongTasks +
                             PlayerControl.GameOptions.NumShortTasks;


            foreach (var task in player.myTasks)
                if (task.TryCast<NormalPlayerTask>() != null)
                {
                    var normalPlayerTask = task.Cast<NormalPlayerTask>();

                    var updateArrow = normalPlayerTask.taskStep > 0;
                    
                    normalPlayerTask.taskStep = 0;
                    normalPlayerTask.Initialize();
                    if (normalPlayerTask.TaskType == TaskTypes.PickUpTowels)
                        foreach (var console in Object.FindObjectsOfType<TowelTaskConsole>())
                            console.Image.color = Color.white;
                    normalPlayerTask.taskStep = 0;

                    if (normalPlayerTask.TaskType == TaskTypes.UploadData)
                        normalPlayerTask.taskStep = 1;
                    if (updateArrow)
                        normalPlayerTask.UpdateArrow();
                    
                    var taskInfo = player.Data.FindTaskById(task.Id);
                    taskInfo.Complete = false;
                }
        }

        /*public static void ResetTowels(NormalPlayerTask task)
        {
            var towelTask = task.Cast<TowelTask>();
            var data = new byte[8];
            var array = Enumerable.Range(0, 14).ToList();
            array.Shuffle();
            var b3 = 0;
            while (b3 < data.Length)
            {
                data[b3] = (byte)array[b3];
                b3++;
            }

            towelTask.Data = data;
            return;
        }

        public static void ResetRecords(NormalPlayerTask task)
        {
            task.Data = new 
        }*/

        public static void AddCollider(Phantom role)
        {
            var player = role.Player;
            var collider2d = player.gameObject.AddComponent<BoxCollider2D>();
            collider2d.isTrigger = true;
            var button = player.gameObject.AddComponent<PassiveButton>();
            button.OnClick = new Button.ButtonClickedEvent();
            button.OnMouseOut = new Button.ButtonClickedEvent();
            button.OnMouseOver = new Button.ButtonClickedEvent();

            button.OnClick.AddListener((Action) (() =>
            {
                if (MeetingHud.Instance) return;
                if (PlayerControl.LocalPlayer.Data.IsDead) return;
                var taskinfos = player.Data.Tasks.ToArray();
                var tasksLeft = taskinfos.Count(x => !x.Complete);
                if (tasksLeft <= CustomGameOptions.PhantomTasksRemaining)
                {
                    role.Caught = true;
                    var writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId,
                        (byte)CustomRPC.CatchPhantom, SendOption.Reliable, -1);
                    writer.Write(role.Player.PlayerId);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                }
            }));
        }
        public static void FindVent()
        {
            if (PlayerControl.LocalPlayer.Is(RoleEnum.Phantom))
            {
                if (Role.GetRole<Phantom>(PlayerControl.LocalPlayer).Caught) return;
                var startingVent =
                    ShipStatus.Instance.AllVents[Random.RandomRangeInt(0, ShipStatus.Instance.AllVents.Count)];
                PlayerControl.LocalPlayer.NetTransform.RpcSnapTo(new Vector2(startingVent.transform.position.x, startingVent.transform.position.y + 0.3636f));
                PlayerControl.LocalPlayer.MyPhysics.RpcEnterVent(startingVent.Id);
            }
        }
    }
}