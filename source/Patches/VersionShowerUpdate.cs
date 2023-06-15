using HarmonyLib;
using System.Linq;
using UnityEngine;

namespace TownOfUs
{
    [HarmonyPriority(Priority.VeryHigh)] // to show this message first, or be overrided if any plugins do
    [HarmonyPatch(typeof(VersionShower), nameof(VersionShower.Start))]
    public static class VersionShowerUpdate
    {
        public static void Postfix(VersionShower __instance)
        {
            var text = __instance.text;
            text.text += " - <color=#00FF00FF>TownOfUs v" + TownOfUs.VersionString + "</color>";
            text.transform.localPosition += new Vector3(-1.2f, 0f, 0f);

            if (GameObject.Find("RightPanel"))
            {
                text.transform.SetParent(GameObject.Find("RightPanel").transform);

                var aspect = text.gameObject.AddComponent<AspectPosition>();
                aspect.Alignment = AspectPosition.EdgeAlignments.Top;
                aspect.DistanceFromEdge = new Vector3(-0.2f, 2f, 10f);

                aspect.StartCoroutine(Effects.Lerp(0.1f, new System.Action<float>((p) =>
                {
                    aspect.AdjustPosition();
                })));


                var reactor = GameObject.Find("LeftPanel").transform.GetChild(3);
                reactor.SetParent(GameObject.Find("RightPanel").transform);
                reactor.localPosition = new Vector3(7.5887f, -3f, 0f);

                var rpos = reactor.GetComponent<AspectPosition>();
                rpos.Alignment = AspectPosition.EdgeAlignments.LeftBottom;
                rpos.DistanceFromEdge = new Vector3(13f, 0f, 10f);


                rpos.StartCoroutine(Effects.Lerp(0.1f, new System.Action<float>((p) => {
                    rpos.AdjustPosition();
                })));

                return;
            }


            var aspect2 = text.gameObject.AddComponent<AspectPosition>();
            aspect2.Alignment = AspectPosition.EdgeAlignments.Bottom;
            aspect2.DistanceFromEdge = new Vector3(0f, 3f, 10f);

            aspect2.StartCoroutine(Effects.Lerp(0.1f, new System.Action<float>((p) =>
            {
                aspect2.AdjustPosition();
            })));

            var react = UnityEngine.Object.FindObjectsOfType<GameObject>().First(x => x.name.Contains("Reactor"));
            if (react)
            {
                var rpos2 = react.GetComponent<AspectPosition>();
                rpos2.Alignment = AspectPosition.EdgeAlignments.Left;
                rpos2.DistanceFromEdge = new Vector3(10f, 0f, 10f);


                rpos2.StartCoroutine(Effects.Lerp(0.1f, new System.Action<float>((p) => {
                    rpos2.AdjustPosition();
                })));
            }
        }
    }
}
