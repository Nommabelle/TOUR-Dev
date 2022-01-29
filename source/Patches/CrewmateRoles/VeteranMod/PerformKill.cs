using HarmonyLib;
using Hazel;
using TownOfUs.Roles;

namespace TownOfUs.CrewmateRoles.VeteranMod
{
    [HarmonyPatch(typeof(KillButtonManager), nameof(KillButtonManager.PerformKill))]
    public class Alert
    {
        public static bool Prefix(KillButtonManager __instance)
        {
            var flag = PlayerControl.LocalPlayer.Is(RoleEnum.Veteran);
            if (!flag) return true;
            if (!PlayerControl.LocalPlayer.CanMove) return false;
            if (PlayerControl.LocalPlayer.Data.IsDead) return false;
            var role = Role.GetRole<Veteran>(PlayerControl.LocalPlayer);
            if (role.RemainingAlerts == 0) return false;
            var alertButton = DestroyableSingleton<HudManager>.Instance.KillButton;
            if (__instance == alertButton)
            {
                if (__instance.isCoolingDown) return false;
                if (!__instance.isActiveAndEnabled) return false;
                if (role.AlertTimer() != 0) return false;
                role.TimeRemaining = CustomGameOptions.AlertDuration;
                role.RemainingAlerts--;
                role.RegenTask();
                role.Alert();
                var writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId,
                    (byte)CustomRPC.Alert, SendOption.Reliable, -1);
                writer.Write(PlayerControl.LocalPlayer.PlayerId);
                AmongUsClient.Instance.FinishRpcImmediately(writer);
                return false;
            }

            return true;
        }
    }
}