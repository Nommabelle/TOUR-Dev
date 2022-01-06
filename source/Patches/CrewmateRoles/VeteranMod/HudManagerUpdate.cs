using HarmonyLib;
using TownOfUs.Roles;
using UnityEngine;

namespace TownOfUs.CrewmateRoles.VeteranMod
{
    [HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
    public class HudManagerUpdate
    {
        public static void Postfix(PlayerControl __instance)
        {
            UpdateAlertButton(__instance);
        }

        public static void UpdateAlertButton(PlayerControl __instance)
        {
            if (PlayerControl.AllPlayerControls.Count <= 1) return;
            if (PlayerControl.LocalPlayer == null) return;
            if (PlayerControl.LocalPlayer.Data == null) return;
            if (!PlayerControl.LocalPlayer.Is(RoleEnum.Veteran)) return;
            var data = PlayerControl.LocalPlayer.Data;
            var isDead = data.IsDead;
            var alertButton = DestroyableSingleton<HudManager>.Instance.KillButton;

            var role = Role.GetRole<Veteran>(PlayerControl.LocalPlayer);
            if (role.RemainingAlerts == 0) return;


            if (isDead)
            {
                alertButton.gameObject.SetActive(false);
                alertButton.isActive = false;
            }
            else
            {
                alertButton.gameObject.SetActive(!MeetingHud.Instance);
                alertButton.isActive = !MeetingHud.Instance;
                alertButton.SetCoolDown(role.AlertTimer(), CustomGameOptions.AlertCd);
            }

            if (role.OnAlert)
            {
                alertButton.SetCoolDown(role.TimeRemaining, CustomGameOptions.AlertDuration);
                return;
            }

            alertButton.SetCoolDown(role.AlertTimer(), CustomGameOptions.AlertCd);


            alertButton.renderer.color = Palette.EnabledColor;
            alertButton.renderer.material.SetFloat("_Desat", 0f);
        }
    }
}