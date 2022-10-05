using HarmonyLib;
using UnityEngine;

namespace TownOfUs.Modifiers
{
    [HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
    public static class Multitasker
    {
        public static void Postfix(HudManager __instance)
        {
            if (!PlayerControl.LocalPlayer.Is(ModifierEnum.Multitasker)) return;
            if (PlayerControl.LocalPlayer.Data.IsDead || PlayerControl.LocalPlayer.Data.Disconnected) return;
            if (!Minigame.Instance) return;
            var Base = Minigame.Instance as MonoBehaviour;
            SpriteRenderer[] rends = Base.GetComponentsInChildren<SpriteRenderer>();
            for (int i = 0; i < rends.Length; i++)
            {
                rends[i].color = new Color(1.0f, 1.0f, 1.0f, 0.3f);
            }
        }
    }
}