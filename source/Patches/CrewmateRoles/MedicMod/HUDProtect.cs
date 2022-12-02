using HarmonyLib;
using TownOfUs.Roles;

namespace TownOfUs.CrewmateRoles.MedicMod
{
    [HarmonyPatch(typeof(HudManager))]
    public class HUDProtect
    {
        [HarmonyPatch(nameof(HudManager.Update))]
        public static void Postfix(HudManager __instance)
        {
            UpdateProtectButton(__instance);
        }

        public static void UpdateProtectButton(HudManager __instance)
        {
            if (PlayerControl.AllPlayerControls.Count <= 1) return;
            if (PlayerControl.LocalPlayer == null) return;
            if (PlayerControl.LocalPlayer.Data == null) return;
            if (!PlayerControl.LocalPlayer.Is(RoleEnum.Medic)) return;
            var data = PlayerControl.LocalPlayer.Data;
            var isDead = data.IsDead;
            var protectButton = __instance.KillButton;

            var role = Role.GetRole<Medic>(PlayerControl.LocalPlayer);


            if (isDead)
            {
                protectButton.gameObject.SetActive(false);
             //   protectButton.isActive = false;
            }
            else
            {
                protectButton.gameObject.SetActive(!MeetingHud.Instance);
                //protectButton.isActive = !MeetingHud.Instance;
                protectButton.SetCoolDown(0f, 1f);
                if (role.UsedAbility) return;
                Utils.SetTarget(ref role.ClosestPlayer, protectButton);
            }
        }
    }
}
