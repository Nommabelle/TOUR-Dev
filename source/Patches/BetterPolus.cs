using System.Collections;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Reactor;
using UnityEngine;

namespace TownOfUs
{
    [HarmonyPatch(typeof(ShipStatus))]
    public static class ShipStatusPatch
    {
        public static readonly Vector3 DvdScreenNewPos = new Vector3(26.635f, -15.92f, 1f);
        public static readonly Vector3 flippedDvdScreenNewPos = new Vector3(12.3f, -15.92f, 1f);

        public static readonly Vector3 VitalsNewPos = new Vector3(31.275f, -6.45f, 1f);

        public static readonly Vector3 WifiNewPos = new Vector3(15.975f, 0.084f, 1f);
        public static readonly Vector3 NavNewPos = new Vector3(11.07f, -15.298f, -0.015f);

        public static readonly Vector3 TempColdNewPos = new Vector3(25.4f, -6.4f, 1f);
        public static readonly Vector3 TempColdNewPosDV = new Vector3(7.772f, -17.103f, -0.017f);

        public const float DvdScreenNewScale = 0.75f;

        public static bool IsAdjustmentsDone;
        public static bool IsObjectsFetched;
        public static bool IsRoomsFetched;
        public static bool IsVentsFetched;

        public static bool flipMap => TutorialManager.InstanceExists ? false : CustomGameOptions.FlipMap;

        public static Console WifiConsole;
        public static Console NavConsole;

        public static SystemConsole Vitals;
        public static GameObject DvdScreenOffice;

        public static Vent ElectricBuildingVent;
        public static Vent ElectricalVent;
        public static Vent ScienceBuildingVent;
        public static Vent StorageVent;
        public static Vent LightCageVent;

        public static Console TempCold;

        public static GameObject Comms;
        public static GameObject DropShip;
        public static GameObject Outside;
        public static GameObject Science;

        [HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.Begin))]
        public static class ShipStatusBeginPatch
        {
            [HarmonyPrefix]
            [HarmonyPatch]
            public static void Prefix(ShipStatus __instance)
            {
                ApplyChanges(__instance);
            }
        }

        [HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.Awake))]
        public static class ShipStatusAwakePatch
        {
            [HarmonyPrefix]
            [HarmonyPatch]
            public static void Prefix(ShipStatus __instance)
            {
                ApplyChanges(__instance);
                if (__instance.Type != ShipStatus.MapType.Pb) if (flipMap) FlipMap(__instance);
            }
        }

        [HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.FixedUpdate))]
        public static class ShipStatusFixedUpdatePatch
        {
            [HarmonyPrefix]
            [HarmonyPatch]
            public static void Prefix(ShipStatus __instance)
            {
                if (!IsObjectsFetched || !IsAdjustmentsDone)
                {
                    ApplyChanges(__instance);
                } 
            }
        }

        private static void ApplyChanges(ShipStatus instance)
        {
            if (instance.Type == ShipStatus.MapType.Pb)
            {
                FindPolusObjects();
                AdjustPolus(instance);
            }
            if (instance.Type == (ShipStatus.MapType)3)
            {
                if (flipMap) FlipMap(instance);
            }
            
        }

        public static void FindPolusObjects()
        {
            FindVents();
            FindRooms();
            FindObjects();
        }


        // these are going to be painful to update :(
        public static Vector3 skeldSize = new Vector3(-1.2f, 1.2f, 1.2f);
        public static Vector3 miraSize = new Vector3(-1f, 1f, 1f);
        public static Vector3 polusSize = new Vector3(-1f, 1f, 1f);
        public static Vector3 airshipSize = new Vector3(-0.7f, 0.7f, 1f);
        public static Vector3 submergedSize = new Vector3(-0.8f, 0.8f, 0.9412f);

        public static Vector3 offsetSkeld = new Vector3(0f, 0f, 1f);
        public static Vector3 offsetMira = new Vector3(-9f, 0f, 1f);
        public static Vector3 offsetPolus = new Vector3(34f, 0f, 1f);
        public static Vector3 offsetAirship = Vector3.zero;
        public static Vector3 offsetSubmerged = new Vector3(7f, 0, 0);

        public static Vector3[] offsetMap = new Vector3[] {offsetSkeld, offsetMira, offsetPolus, offsetAirship, offsetAirship, offsetSubmerged };
        public static Vector3[] sizeMap = new Vector3[] { skeldSize, miraSize, polusSize, airshipSize, airshipSize, submergedSize };

        // The best method for a new era of superior amongus gameplay
        public static void FlipMap(ShipStatus instance)
        {
            instance.gameObject.transform.localScale = sizeMap[(int)instance.Type];
            instance.gameObject.transform.position = offsetMap[(int)instance.Type];

            if (instance.Type == (ShipStatus.MapType)5)
            {
                instance.gameObject.transform.FindChild("TopFloor/UpperCentral/GlassFloorPlayer/Floor").localScale = new Vector3(-10, 10, 1);
                instance.gameObject.transform.FindChild("BottomFloor/LowerCentral/ShadowStuff/ShadowLayer").localScale = new Vector3(-1, 1, 1);
            }
            
        }

        public static void AdjustPolus(ShipStatus instance)
        {
            if (IsObjectsFetched && IsRoomsFetched)
            {
                if (CustomGameOptions.VitalsLab) MoveVitals();
                if (!CustomGameOptions.ColdTempDeathValley && CustomGameOptions.VitalsLab) MoveTempCold();
                if (CustomGameOptions.ColdTempDeathValley) MoveTempColdDV();
                if (CustomGameOptions.WifiChartCourseSwap) SwitchNavWifi();
                if (flipMap) Coroutines.Start(delayedFlip(instance));
            }

            if (CustomGameOptions.VentImprovements) AdjustVents();

            IsAdjustmentsDone = true;
        }

        public static void FindVents()
        {
            var ventsList = Object.FindObjectsOfType<Vent>().ToList();

            if (ElectricBuildingVent == null)
            {
                ElectricBuildingVent = ventsList.Find(vent => vent.gameObject.name == "ElectricBuildingVent");
            }

            if (ElectricalVent == null)
            {
                ElectricalVent = ventsList.Find(vent => vent.gameObject.name == "ElectricalVent");
            }

            if (ScienceBuildingVent == null)
            {
                ScienceBuildingVent = ventsList.Find(vent => vent.gameObject.name == "ScienceBuildingVent");
            }

            if (StorageVent == null)
            {
                StorageVent = ventsList.Find(vent => vent.gameObject.name == "StorageVent");
            }

            if (LightCageVent == null)
            {
                LightCageVent = ventsList.Find(vent => vent.gameObject.name == "ElecFenceVent");
            }

            IsVentsFetched = ElectricBuildingVent != null && ElectricalVent != null && ScienceBuildingVent != null &&
                              StorageVent != null && LightCageVent != null;
        }

        public static void FindRooms()
        {
            if (Comms == null)
            {
                Comms = Object.FindObjectsOfType<GameObject>().ToList().Find(o => o.name == "Comms");
            }

            if (DropShip == null)
            {
                DropShip = Object.FindObjectsOfType<GameObject>().ToList().Find(o => o.name == "Dropship");
            }

            if (Outside == null)
            {
                Outside = Object.FindObjectsOfType<GameObject>().ToList().Find(o => o.name == "Outside");
            }

            if (Science == null)
            {
                Science = Object.FindObjectsOfType<GameObject>().ToList().Find(o => o.name == "Science");
            }

            IsRoomsFetched = Comms != null && DropShip != null && Outside != null && Science != null;
        }

        public static void FindObjects()
        {
            if (WifiConsole == null)
            {
                WifiConsole = Object.FindObjectsOfType<Console>().ToList()
                    .Find(console => console.name == "panel_wifi");
            }

            if (NavConsole == null)
            {
                NavConsole = Object.FindObjectsOfType<Console>().ToList()
                    .Find(console => console.name == "panel_nav");
            }

            if (Vitals == null)
            {
                Vitals = Object.FindObjectsOfType<SystemConsole>().ToList()
                    .Find(console => console.name == "panel_vitals");
            }

            if (DvdScreenOffice == null)
            {
                GameObject DvdScreenAdmin = Object.FindObjectsOfType<GameObject>().ToList()
                    .Find(o => o.name == "dvdscreen");

                if (DvdScreenAdmin != null)
                {
                    DvdScreenOffice = Object.Instantiate(DvdScreenAdmin);
                }
            }

            if (TempCold == null)
            {
                TempCold = Object.FindObjectsOfType<Console>().ToList()
                    .Find(console => console.name == "panel_tempcold");
            }

            IsObjectsFetched = WifiConsole != null && NavConsole != null && Vitals != null &&
                               DvdScreenOffice != null && TempCold != null;
        }

        public static void AdjustVents()
        {
            if (IsVentsFetched)
            {
                ElectricBuildingVent.Left = ElectricalVent;
                ElectricalVent.Center = ElectricBuildingVent;
                ElectricBuildingVent.Center = LightCageVent;
                LightCageVent.Center = ElectricBuildingVent;

                ScienceBuildingVent.Left = StorageVent;
                StorageVent.Center = ScienceBuildingVent;
            }
        }

        public static void MoveTempCold()
        {
            if (TempCold.transform.position != TempColdNewPos)
            {
                Transform tempColdTransform = TempCold.transform;
                tempColdTransform.parent = Outside.transform;
                tempColdTransform.position = TempColdNewPos;

                BoxCollider2D collider = TempCold.GetComponent<BoxCollider2D>();
                collider.isTrigger = false;
                collider.size += new Vector2(0f, -0.3f);
            }
        }

        public static void MoveTempColdDV()
        {
            if (TempCold.transform.position != TempColdNewPosDV)
            {
                Transform tempColdTransform = TempCold.transform;
                tempColdTransform.parent = Outside.transform;
                tempColdTransform.position = TempColdNewPosDV;

                BoxCollider2D collider = TempCold.GetComponent<BoxCollider2D>();
                collider.isTrigger = false;
                collider.size += new Vector2(0f, -0.3f);
            }
        }

        public static void SwitchNavWifi()
        {
            if (WifiConsole.transform.position != WifiNewPos)
            {
                Transform wifiTransform = WifiConsole.transform;
                wifiTransform.parent = DropShip.transform;
                wifiTransform.position = WifiNewPos;
            }

            if (NavConsole.transform.position != NavNewPos)
            {
                Transform navTransform = NavConsole.transform;
                navTransform.parent = Comms.transform;
                navTransform.position = NavNewPos;

                NavConsole.checkWalls = true;
            }
        }

        public static void MoveVitals()
        {
            if (Vitals.transform.position != VitalsNewPos)
            {
                Transform vitalsTransform = Vitals.gameObject.transform;
                vitalsTransform.parent = Science.transform;
                vitalsTransform.position = VitalsNewPos;
            }

            if (DvdScreenOffice.transform.position != DvdScreenNewPos)
            {
                Transform dvdScreenTransform = DvdScreenOffice.transform;
                dvdScreenTransform.position = DvdScreenNewPos;
                if (flipMap) dvdScreenTransform.position = flippedDvdScreenNewPos;

                var localScale = dvdScreenTransform.localScale;
                localScale =
                    new Vector3(DvdScreenNewScale, localScale.y,
                        localScale.z);
                dvdScreenTransform.localScale = localScale;
            }
        }


        public static IEnumerator delayedFlip(ShipStatus __instance)
        {
            yield return new WaitForSeconds(0.1f);
            FlipMap(__instance);
        }
    }



    [HarmonyPatch(typeof(MapBehaviour), nameof(MapBehaviour.GenericShow))]
    public static class MapBehaviourGenericShowUpdate
    {
        [HarmonyPostfix]
        [HarmonyPatch]
        public static void Postfix(MapBehaviour __instance)
        {
            if (!ShipStatusPatch.flipMap) return;
            __instance.gameObject.transform.localScale = new Vector3(-1, 1, 1);
            if (ShipStatus.Instance.Type == (ShipStatus.MapType)5)
            {

                __instance.gameObject.transform.FindChild("MapHud/RoomNames/Upper").GetComponentsInChildren<Transform>().ToList().ForEach(x => x.localScale = new Vector3(-1, 1, 1));
                __instance.gameObject.transform.FindChild("MapHud/RoomNames/Upper").transform.localScale = Vector3.one;

                __instance.gameObject.transform.FindChild("MapHud/RoomNames/Lower").GetComponentsInChildren<Transform>().ToList().ForEach(x => x.localScale = new Vector3(-1, 1, 1));
                __instance.gameObject.transform.FindChild("MapHud/RoomNames/Lower").transform.localScale = Vector3.one;

                for (int i = 0; i < __instance.infectedOverlay.transform.childCount; i++)
                {
                    for (int e = 0; e < __instance.infectedOverlay.transform.GetChild(i).childCount; e ++)
                    {
                        for (int o = 0; o < __instance.infectedOverlay.transform.GetChild(i).GetChild(e).childCount; o ++)
                        {
                            __instance.infectedOverlay.transform.GetChild(i).GetChild(e).GetChild(o).localScale = new Vector3(-1, 1, 1);
                        }
                    }
                }
            }else if (ShipStatus.Instance.Type == (ShipStatus.MapType)1 )
            {
                __instance.gameObject.transform.GetChild(__instance.gameObject.transform.GetChildCount() - 1).GetComponentsInChildren<Transform>().ToList().ForEach(x => x.localScale = new Vector3(-1, 1, 1));
                __instance.gameObject.transform.GetChild(__instance.gameObject.transform.GetChildCount() - 1).transform.localScale = Vector3.one;

                for (int i = 0; i < __instance.infectedOverlay.transform.childCount; i++)
                {
                    for (int e = 0; e < __instance.infectedOverlay.transform.GetChild(i).childCount; e++)
                    {
                        __instance.infectedOverlay.transform.GetChild(i).GetChild(e).localScale = new Vector3(-0.8f, 0.8f, 1);
                    }
                }
            }
            else
            {
                __instance.gameObject.transform.GetChild(__instance.gameObject.transform.GetChildCount() - 1).GetComponentsInChildren<Transform>().ToList().ForEach(x => x.localScale = new Vector3(-1, 1, 1));
                __instance.gameObject.transform.GetChild(__instance.gameObject.transform.GetChildCount() - 1).transform.localScale = Vector3.one;
                for (int i = 0; i < __instance.infectedOverlay.transform.childCount; i++) __instance.infectedOverlay.transform.GetChild(i).localScale = new Vector3(-1, 1, 1);
            }
        }
    }

    [HarmonyPatch(typeof(MapBehaviour), nameof(MapBehaviour.Awake))]
    public static class MapBehaviourStartClass
    {
        [HarmonyPostfix]
        public static void Postfix(MapBehaviour __instance)
        {
            if (!ShipStatusPatch.flipMap) return;
            switch ((int)ShipStatus.Instance.Type)
            {
                case 1:
                    __instance.HerePoint.gameObject.transform.parent.localPosition = new Vector3(-3.2384f, -2.05f, 0f);
                    break;
                case 2:
                    __instance.HerePoint.gameObject.transform.parent.localPosition = new Vector3(2.711f, 2.4508f, - 0.1f);
                    break;
                case 5:
                    __instance.HerePoint.gameObject.transform.parent.localPosition = new Vector3(1.4f, - 3.45f, 0f);
                    break;
            }
            
        }
    }

    [HarmonyPatch(typeof(SpawnInMinigame),nameof(SpawnInMinigame.SpawnAt))]
    public static class ArishipSpawnLoc
    {
        [HarmonyPrefix]
        public static void Prefix(SpawnInMinigame __instance, ref Vector3 spawnAt)
        {
            if (!ShipStatusPatch.flipMap) return;
            Vector3 spawnNew = new Vector3(-spawnAt.x, spawnAt.y, spawnAt.z);
            spawnAt = spawnNew;
        }
    }

}