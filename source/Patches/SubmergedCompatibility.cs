using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BepInEx;
using BepInEx.IL2CPP;
using HarmonyLib;
using UnhollowerRuntimeLib;
using UnityEngine;
using Reactor;
using UnhollowerBaseLib;
using TownOfUs.Roles;

namespace TownOfUs.Patches
{


    [HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
    public static class SubmergedHudPatch
    {
        public static void Postfix(HudManager __instance)
        {
            if (SubmergedCompatibility.isSubmerged())
            {
                if (PlayerControl.LocalPlayer.Data.IsDead && PlayerControl.LocalPlayer.Is(RoleEnum.Haunter))
                {
                    if (!Role.GetRole<Haunter>(PlayerControl.LocalPlayer).Caught) __instance.MapButton.transform.parent.Find(__instance.MapButton.name + "(Clone)").gameObject.SetActive(false);
                    else __instance.MapButton.transform.parent.Find(__instance.MapButton.name + "(Clone)").gameObject.SetActive(true);
                }
                if (PlayerControl.LocalPlayer.Data.IsDead && PlayerControl.LocalPlayer.Is(RoleEnum.Phantom))
                {
                    if (!Role.GetRole<Phantom>(PlayerControl.LocalPlayer).Caught) __instance.MapButton.transform.parent.Find(__instance.MapButton.name + "(Clone)").gameObject.SetActive(false);
                    else  __instance.MapButton.transform.parent.Find(__instance.MapButton.name + "(Clone)").gameObject.SetActive(true);
                }
            }
                
        }
    }

    [HarmonyPatch(typeof(PlayerPhysics), nameof(PlayerPhysics.HandleAnimation))]
    [HarmonyPriority(Priority.Low)] //make sure it occurs after other patches
    public static class SubmergedPhysicsPatch
    {
        public static void Postfix(PlayerPhysics __instance)
        {
            if (SubmergedCompatibility.Loaded && __instance.myPlayer.Data.IsDead)
            {
                if (__instance.myPlayer.Is(RoleEnum.Phantom))
                {
                    if (!Role.GetRole<Phantom>(PlayerControl.LocalPlayer).Caught)
                    {
                        SubmergedCompatibility.MoveDeadPlayerElevator(__instance.myPlayer);
                        Transform transform = __instance.transform;
                        Vector3 position = transform.position;
                        position.z = 0f;
                        transform.position = position;
                        __instance.myPlayer.gameObject.layer = 8;
                    }
                }
                if (__instance.myPlayer.Is(RoleEnum.Haunter))
                {
                    if (!Role.GetRole<Haunter>(PlayerControl.LocalPlayer).Caught)
                    {
                        SubmergedCompatibility.MoveDeadPlayerElevator(__instance.myPlayer);
                        Transform transform = __instance.transform;
                        Vector3 position = transform.position;
                        position.z = 0f;
                        transform.position = position;
                        __instance.myPlayer.gameObject.layer = 8;
                    }
                }
            }

            
        }
    }



    public static class SubmergedCompatibility
    {
        public static class Classes
        {
            public const string ElevatorMover = "ElevatorMover";
        }

        public const string SUBMERGED_GUID = "Submerged";
        public const ShipStatus.MapType SUBMERGED_MAP_TYPE = (ShipStatus.MapType)5;

        public static SemanticVersioning.Version Version { get; private set; }
        public static bool Loaded { get; private set; }
        public static BasePlugin Plugin { get; private set; }
        public static Assembly Assembly { get; private set; }
        public static Type[] Types { get; private set; }
        public static Dictionary<string, Type> InjectedTypes { get; private set; }

        private static MonoBehaviour _submarineStatus;
        public static MonoBehaviour SubmarineStatus
        {
            get
            {
                if (!Loaded) return null;

                if (_submarineStatus is null || _submarineStatus.WasCollected || !_submarineStatus || _submarineStatus == null)
                {
                    if (ShipStatus.Instance is null || ShipStatus.Instance.WasCollected || !ShipStatus.Instance || ShipStatus.Instance == null)
                    {
                        return _submarineStatus = null;
                    }
                    else
                    {
                        if (ShipStatus.Instance.Type == SUBMERGED_MAP_TYPE)
                        {
                            return _submarineStatus = ShipStatus.Instance.GetComponent(Il2CppType.From(SubmarineStatusType))?.TryCast(SubmarineStatusType) as MonoBehaviour;
                        }
                        else
                        {
                            return _submarineStatus = null;
                        }
                    }
                }
                else
                {
                    return _submarineStatus;
                }
            }
        }

        public static bool DisableO2MaskCheckForEmergency
        {
            set
            {
                if (!Loaded) return;
                DisableO2MaskCheckField.SetValue(null, value);
            }
        }

        private static Type SubmarineStatusType;
        private static MethodInfo CalculateLightRadiusMethod;

        private static Type TaskIsEmergencyPatchType;
        private static FieldInfo DisableO2MaskCheckField;

        private static MethodInfo RpcRequestChangeFloorMethod;
        private static Type FloorHandlerType;
        private static MethodInfo GetFloorHandlerMethod;

        private static Type Vent_MoveToVent_PatchType;
        private static FieldInfo InTransitionField;

        private static Type CustomTaskTypesType;
        private static FieldInfo RetrieveOxigenMaskField;
        public static TaskTypes RetrieveOxygenMask;
        private static Type SubmarineOxygenSystemType;
        private static FieldInfo SubmarineOxygenSystemInstanceField;
        private static MethodInfo RepairDamageMethod;

        private static Type SubmergedExileController;
        private static MethodInfo SubmergedExileWrapUpMethod;

        private static Type SubmarineElevator;
        private static MethodInfo GetInElevator;
        private static MethodInfo GetMovementStageFromTime;
        private static FieldInfo getSubElevatorSystem;

        private static Type SubmarineElevatorSystem;
        private static FieldInfo UpperDeckIsTargetFloor; 

        private static FieldInfo SubmergedInstance;
        private static FieldInfo SubmergedElevators;

        public static void Initialize()
        {
            Loaded = IL2CPPChainloader.Instance.Plugins.TryGetValue(SUBMERGED_GUID, out PluginInfo plugin);
            if (!Loaded) return;

            Plugin = plugin!.Instance as BasePlugin;
            Version = plugin.Metadata.Version;

            Assembly = Plugin!.GetType().Assembly;
            Types = AccessTools.GetTypesFromAssembly(Assembly);

            InjectedTypes = (Dictionary<string, Type>)AccessTools.PropertyGetter(Types.FirstOrDefault(t => t.Name == "RegisterInIl2CppAttribute"), "RegisteredTypes")
                .Invoke(null, Array.Empty<object>());

            SubmarineStatusType = Types.First(t => t.Name == "SubmarineStatus");
            SubmergedInstance = AccessTools.Field(SubmarineStatusType, "Instance");
            SubmergedElevators = AccessTools.Field(SubmarineStatusType, "Elevators");

            CalculateLightRadiusMethod = AccessTools.Method(SubmarineStatusType, "CalculateLightRadius");

            TaskIsEmergencyPatchType = Types.First(t => t.Name == "PlayerTask_TaskIsEmergency_Patch");
            DisableO2MaskCheckField = AccessTools.Field(TaskIsEmergencyPatchType, "DisableO2MaskCheck");

            FloorHandlerType = Types.First(t => t.Name == "FloorHandler");
            GetFloorHandlerMethod = AccessTools.Method(FloorHandlerType, "GetFloorHandler", new Type[] { typeof(PlayerControl) });
            RpcRequestChangeFloorMethod = AccessTools.Method(FloorHandlerType, "RpcRequestChangeFloor");

            Vent_MoveToVent_PatchType = Types.First(t => t.Name == "Vent_MoveToVent_Patch");
            InTransitionField = AccessTools.Field(Vent_MoveToVent_PatchType, "InTransition");

            CustomTaskTypesType = Types.First(t => t.Name == "CustomTaskTypes");
            RetrieveOxigenMaskField = AccessTools.Field(CustomTaskTypesType, "RetrieveOxygenMask");
            RetrieveOxygenMask = (TaskTypes)RetrieveOxigenMaskField.GetValue(null);

            SubmarineOxygenSystemType = Types.First(t => t.Name == "SubmarineOxygenSystem");
            SubmarineOxygenSystemInstanceField = AccessTools.Field(SubmarineOxygenSystemType, "Instance");
            RepairDamageMethod = AccessTools.Method(SubmarineOxygenSystemType, "RepairDamage");
            SubmergedExileController = Types.First(t => t.Name == "SubmergedExileController");
            SubmergedExileWrapUpMethod = AccessTools.Method(SubmergedExileController, "WrapUpAndSpawn");

            SubmarineElevator = Types.First(t => t.Name == "SubmarineElevator");
            GetInElevator = AccessTools.Method(SubmarineElevator, "GetInElevator");
            GetMovementStageFromTime = AccessTools.Method(SubmarineElevator, "GetMovementStageFromTime");
            getSubElevatorSystem = AccessTools.Field(SubmarineElevator, "System");

            SubmarineElevatorSystem = Types.First(t => t.Name == "SubmarineElevatorSystem");
            UpperDeckIsTargetFloor = AccessTools.Field(SubmarineElevatorSystem, "UpperDeckIsTargetFloor");
            //I tried patching normally but it would never work
            Harmony _harmony = new Harmony("tou.submerged.patch");
            var exilerolechangePostfix = SymbolExtensions.GetMethodInfo(() => ExileRoleChangePostfix());
            _harmony.Patch(SubmergedExileWrapUpMethod, null, new HarmonyMethod(exilerolechangePostfix));
        }

        public static void CheckOutOfBoundsElevator(PlayerControl player)
        {
            if (!Loaded) return;
            if (!isSubmerged()) return;

            Tuple<bool, object> elevator = GetPlayerElevator(player);
            if (!elevator.Item1) return;
            bool CurrentFloor = (bool)UpperDeckIsTargetFloor.GetValue(getSubElevatorSystem.GetValue(elevator.Item2)); //true is top, false is bottom
            bool PlayerFloor = player.transform.position.y > -7f; //true is top, false is bottom
            
            if (CurrentFloor != PlayerFloor)
            {
                ChangeFloor(CurrentFloor);
            }
        }

        public static void MoveDeadPlayerElevator(PlayerControl player)
        {
            if (!Loaded) return;
            Tuple<bool, object> elevator = GetPlayerElevator(player);
            if (!elevator.Item1) return;

            int MovementStage = (int)GetMovementStageFromTime.Invoke(elevator.Item2, null);
            if (MovementStage >= 5)
            {
                //Fade to clear
                bool topfloortarget = (bool)UpperDeckIsTargetFloor.GetValue(getSubElevatorSystem.GetValue(elevator.Item2)); //true is top, false is bottom
                bool topintendedtarget = player.transform.position.y > -7f; //true is top, false is bottom
                if (topfloortarget != topintendedtarget)
                {
                    ChangeFloor(!topintendedtarget);
                }
            }
        }

        public static Tuple<bool, object> GetPlayerElevator(PlayerControl player)
        {
            if (!Loaded) return Tuple.Create(false, (object)null);
            IList elevatorlist = Utils.createList(SubmarineElevator);
            elevatorlist = (IList)SubmergedElevators.GetValue(SubmergedInstance.GetValue(null));
            foreach (object elevator in elevatorlist)
            {
                if ((bool)GetInElevator.Invoke(elevator, new object[] { player })) return Tuple.Create(true, elevator);
            }

            return Tuple.Create(false, (object)null);
        }

        public static void ExileRoleChangePostfix()
        {
            NeutralRoles.PhantomMod.SetPhantom.ExileControllerPostfix(ExileController.Instance);
            ImpostorRoles.TraitorMod.SetTraitor.ExileControllerPostfix(ExileController.Instance);
            CrewmateRoles.HaunterMod.SetHaunter.ExileControllerPostfix(ExileController.Instance);

            Coroutines.Start(resetTimers());
            Coroutines.Start(GhostRoleBegin());
            
        }

        public static IEnumerator resetTimers()
        {
            if (PlayerControl.LocalPlayer.Data.IsDead) yield break;
            while (!PlayerControl.LocalPlayer.moveable)
            {
                yield return null;
            }
            yield return new WaitForSeconds(0.5f);
            while (DestroyableSingleton<HudManager>.Instance.PlayerCam.transform.Find("SpawnInMinigame(Clone)") != null)
            {
                yield return null;
            }
            Utils.ResetCustomTimers();
        }


        public static IEnumerator GhostRoleBegin()
        {
            if (!PlayerControl.LocalPlayer.Data.IsDead) yield break;
            while (!PlayerControl.LocalPlayer.moveable)
            {
                yield return null;
            }
            yield return new WaitForSeconds(0.5f);
            while (DestroyableSingleton<HudManager>.Instance.PlayerCam.transform.Find("SpawnInMinigame(Clone)") != null)
            {
                yield return null;
            }
            
            if (PlayerControl.LocalPlayer.Is(RoleEnum.Haunter))
            {
                if (!Role.GetRole<Haunter>(PlayerControl.LocalPlayer).Caught)
                {
                    var startingVent =
                        ShipStatus.Instance.AllVents[UnityEngine.Random.RandomRangeInt(0, ShipStatus.Instance.AllVents.Count)];
                    ChangeFloor(startingVent.transform.position.y > -7f);
                    PlayerControl.LocalPlayer.NetTransform.RpcSnapTo(new Vector2(startingVent.transform.position.x, startingVent.transform.position.y + 0.3636f));
                    PlayerControl.LocalPlayer.MyPhysics.RpcEnterVent(startingVent.Id);
                }
            }
            if (PlayerControl.LocalPlayer.Is(RoleEnum.Phantom))
            {
                if (!Role.GetRole<Phantom>(PlayerControl.LocalPlayer).Caught)
                {
                    var startingVent =
                        ShipStatus.Instance.AllVents[UnityEngine.Random.RandomRangeInt(0, ShipStatus.Instance.AllVents.Count)];
                    ChangeFloor(startingVent.transform.position.y > -7f);
                    PlayerControl.LocalPlayer.NetTransform.RpcSnapTo(new Vector2(startingVent.transform.position.x, startingVent.transform.position.y + 0.3636f));
                    PlayerControl.LocalPlayer.MyPhysics.RpcEnterVent(startingVent.Id);
                }
            }
        }


        public static MonoBehaviour AddSubmergedComponent(this GameObject obj, string typeName)
        {
            if (!Loaded) return obj.AddComponent<MissingSubmergedBehaviour>();
            bool validType = InjectedTypes.TryGetValue(typeName, out Type type);
            return validType ? obj.AddComponent(Il2CppType.From(type)).TryCast<MonoBehaviour>() : obj.AddComponent<MissingSubmergedBehaviour>();
        }

        public static float GetSubmergedNeutralLightRadius(bool isImpostor)
        {
            if (!Loaded) return 0;
            return (float)CalculateLightRadiusMethod.Invoke(SubmarineStatus, new object[] { null, true, isImpostor });
        }

        public static void ChangeFloor(bool toUpper)
        {
            if (!Loaded) return;
            MonoBehaviour _floorHandler = ((Component)GetFloorHandlerMethod.Invoke(null, new object[] { PlayerControl.LocalPlayer })).TryCast(FloorHandlerType) as MonoBehaviour;
            RpcRequestChangeFloorMethod.Invoke(_floorHandler, new object[] { toUpper });
        }

        public static bool getInTransition()
        {
            if (!Loaded) return false;
            return (bool)InTransitionField.GetValue(null);
        }


        public static void RepairOxygen()
        {
            if (!Loaded) return;
            try
            {
                ShipStatus.Instance.RpcRepairSystem((SystemTypes)130, 64);
                RepairDamageMethod.Invoke(SubmarineOxygenSystemInstanceField.GetValue(null), new object[] { PlayerControl.LocalPlayer, 64 });
            }
            catch (System.NullReferenceException)
            {
                
            }

        }

        public static bool isSubmerged()
        {
            return Loaded && ShipStatus.Instance && ShipStatus.Instance.Type == SUBMERGED_MAP_TYPE;
        }
    }

    public class MissingSubmergedBehaviour : MonoBehaviour
    {
        static MissingSubmergedBehaviour() => ClassInjector.RegisterTypeInIl2Cpp<MissingSubmergedBehaviour>();
        public MissingSubmergedBehaviour(IntPtr ptr) : base(ptr) { }
    }
}
