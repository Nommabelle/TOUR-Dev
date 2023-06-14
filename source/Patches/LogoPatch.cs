using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;

namespace TownOfUs {
    [HarmonyPatch(typeof(MainMenuManager), nameof(MainMenuManager.Start))]
    public static class LogoPatch
    {
        private static Sprite Sprite => TownOfUs.ToUBanner;
        static void Postfix(PingTracker __instance) {
            var touLogo = new GameObject("bannerLogo_TownOfUs");
            touLogo.transform.localScale = new Vector3(0.8f, 0.8f, 1f);

            var renderer = touLogo.AddComponent<SpriteRenderer>();
            renderer.sprite = Sprite;


            var position = touLogo.AddComponent<AspectPosition>();
            position.DistanceFromEdge = new Vector3(-0.2f, 1f, 10f);
            position.Alignment = AspectPosition.EdgeAlignments.Top;

            position.StartCoroutine(Effects.Lerp(0.1f, new System.Action<float>((p) =>
            {
                position.AdjustPosition();
            })));


            var scaler = touLogo.AddComponent<AspectScaledAsset>();
            var renderers = new Il2CppSystem.Collections.Generic.List<SpriteRenderer>();
            renderers.Add(renderer);

            scaler.spritesToScale = renderers;
            scaler.aspectPosition = position;

            touLogo.transform.SetParent(GameObject.Find("RightPanel").transform);


            var reactor = GameObject.Find("LeftPanel").transform.GetChild(3);
            reactor.SetParent(GameObject.Find("RightPanel").transform);
            reactor.localPosition = new Vector3(7.5887f, - 3f, 0f);

            var rpos = reactor.GetComponent<AspectPosition>();
            rpos.Alignment = AspectPosition.EdgeAlignments.LeftBottom;
            rpos.DistanceFromEdge = new Vector3(13f, 0f, 10f);


            rpos.StartCoroutine(Effects.Lerp(0.1f, new System.Action<float>((p) => {
                rpos.AdjustPosition();
            })));
        }
    }
}