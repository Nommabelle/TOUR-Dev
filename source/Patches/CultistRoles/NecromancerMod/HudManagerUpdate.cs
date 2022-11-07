using HarmonyLib;
using TownOfUs.Roles;
using TownOfUs.Roles.Cultist;
using UnityEngine;

namespace TownOfUs.CultistRoles.NecromancerMod
{
    [HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
    public class HudManagerUpdate
    {
        public static Sprite ReviveSprite => TownOfUs.Revive2Sprite;
        public static byte DontRevive = byte.MaxValue;

        public static void Postfix(HudManager __instance)
        {
            if (PlayerControl.AllPlayerControls.Count <= 1) return;
            if (PlayerControl.LocalPlayer == null) return;
            if (PlayerControl.LocalPlayer.Data == null) return;
            if (!PlayerControl.LocalPlayer.Is(RoleEnum.Necromancer)) return;
            var role = Role.GetRole<Necromancer>(PlayerControl.LocalPlayer);
            if (role.ReviveButton == null)
            {
                role.ReviveButton = Object.Instantiate(__instance.KillButton, __instance.KillButton.transform.parent);
                role.ReviveButton.graphic.enabled = true;
                role.ReviveButton.GetComponent<AspectPosition>().DistanceFromEdge = TownOfUs.ButtonPosition;
                role.ReviveButton.gameObject.SetActive(false);
            }

            role.ReviveButton.GetComponent<AspectPosition>().Update();
            role.ReviveButton.graphic.sprite = ReviveSprite;
            role.ReviveButton.gameObject.SetActive(!PlayerControl.LocalPlayer.Data.IsDead && !MeetingHud.Instance);

            role.ReviveButton.SetCoolDown(role.ReviveTimer(), CustomGameOptions.ReviveCooldown);

            var data = PlayerControl.LocalPlayer.Data;
            var isDead = data.IsDead;
            var truePosition = PlayerControl.LocalPlayer.GetTruePosition();
            var maxDistance = GameOptionsData.KillDistances[PlayerControl.GameOptions.KillDistance];
            var flag = (PlayerControl.GameOptions.GhostsDoTasks || !data.IsDead) &&
                       (!AmongUsClient.Instance || !AmongUsClient.Instance.IsGameOver) &&
                       PlayerControl.LocalPlayer.CanMove;
            var allocs = Physics2D.OverlapCircleAll(truePosition, maxDistance,
                LayerMask.GetMask(new[] { "Players", "Ghost" }));

            DeadBody closestBody = null;
            var closestDistance = float.MaxValue;

            foreach (var collider2D in allocs)
            {
                if (!flag || isDead || collider2D.tag != "DeadBody") continue;
                var component = collider2D.GetComponent<DeadBody>();


                if (!(Vector2.Distance(truePosition, component.TruePosition) <=
                      maxDistance)) continue;

                var distance = Vector2.Distance(truePosition, component.TruePosition);
                if (!(distance < closestDistance)) continue;
                closestBody = component;
                closestDistance = distance;
            }

            if (isDead)
            {
                role.ReviveButton.gameObject.SetActive(false);
            }
            else
            {
                role.ReviveButton.gameObject.SetActive(!MeetingHud.Instance);
            }

            if (role.CurrentTarget && role.CurrentTarget != closestBody)
                role.CurrentTarget.bodyRenderer.material.SetFloat("_Outline", 0f);


            if (closestBody != null && closestBody.ParentId == DontRevive) closestBody = null;
            role.CurrentTarget = closestBody;
            if (role.CurrentTarget && __instance.enabled)
            {
                var player = Utils.PlayerById(role.CurrentTarget.ParentId);
                if (player.Is(RoleEnum.Sheriff) || player.Is(RoleEnum.CultistSeer) || player.Is(RoleEnum.Survivor) || player.Is(RoleEnum.Mayor)) return;
                var component = role.CurrentTarget.bodyRenderer;
                component.material.SetFloat("_Outline", 1f);
                component.material.SetColor("_OutlineColor", Color.red);
                role.ReviveButton.graphic.color = Palette.EnabledColor;
                role.ReviveButton.graphic.material.SetFloat("_Desat", 0f);
                return;
            }

            role.ReviveButton.graphic.color = Palette.DisabledClear;
            role.ReviveButton.graphic.material.SetFloat("_Desat", 1f);
        }
    }
}