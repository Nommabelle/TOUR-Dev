﻿using HarmonyLib;
using TownOfUs.Roles;

namespace TownOfUs.CrewmateRoles.DetectiveMod
{
    [HarmonyPatch(typeof(HudManager))]
    public class HudExamine
    {
        [HarmonyPatch(nameof(HudManager.Update))]
        public static void Postfix(HudManager __instance)
        {
            UpdateExamineButton(__instance);
        }

        public static void UpdateExamineButton(HudManager __instance)
        {
            if (PlayerControl.AllPlayerControls.Count <= 1) return;
            if (PlayerControl.LocalPlayer == null) return;
            if (PlayerControl.LocalPlayer.Data == null) return;
            if (!PlayerControl.LocalPlayer.Is(RoleEnum.Detective)) return;
            var data = PlayerControl.LocalPlayer.Data;
            var isDead = data.IsDead;
            var examineButton = __instance.KillButton;

            var role = Role.GetRole<Detective>(PlayerControl.LocalPlayer);

            if (isDead)
            {
                examineButton.gameObject.SetActive(false);
            }
            else
            {
                examineButton.gameObject.SetActive(!MeetingHud.Instance);
                // trackButton.isActive = !MeetingHud.Instance;
                examineButton.SetCoolDown(role.ExamineTimer(), CustomGameOptions.ExamineCd);

                Utils.SetTarget(ref role.ClosestPlayer, examineButton, float.NaN);
            }

            var renderer = examineButton.graphic;
            if (role.ClosestPlayer != null)
            {
                renderer.color = Palette.EnabledColor;
                renderer.material.SetFloat("_Desat", 0f);
            }
            else
            {
                renderer.color = Palette.DisabledClear;
                renderer.material.SetFloat("_Desat", 1f);
            }
        }
    }
}