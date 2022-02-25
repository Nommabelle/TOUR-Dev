using System.Linq;
using HarmonyLib;
using TownOfUs.Roles;
using Object = UnityEngine.Object;

namespace TownOfUs.CrewmateRoles.SnitchMod
{
    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.FixedUpdate))]
    public class UpdateArrows
    {
        public static void Postfix(PlayerControl __instance)
        {
            foreach (var role in Role.AllRoles.Where(x => x.RoleType == RoleEnum.Snitch))
            {
                var snitch = (Snitch)role;
                if (PlayerControl.LocalPlayer.Data.IsDead || snitch.Player.Data.IsDead)
                {
                    snitch.SnitchArrows.Values.DestroyAll();
                    snitch.SnitchArrows.Clear();
                    snitch.ImpArrows.DestroyAll();
                    snitch.ImpArrows.Clear();
                }

                foreach (var arrow in snitch.ImpArrows) arrow.target = snitch.Player.transform.position;

                foreach (var arrow in snitch.SnitchArrows)
                {
                    var player = Utils.PlayerById(arrow.Key);
                    if (player == null || player.Data == null || player.Data.IsDead || player.Data.Disconnected)
                    {
                        var arrow2 = snitch.SnitchArrows.FirstOrDefault(x => x.Key == arrow.Key);
                        if (arrow.Value != null)
                            Object.Destroy(arrow.Value);
                        if (arrow.Value.gameObject != null)
                            Object.Destroy(arrow.Value.gameObject);
                        snitch.SnitchArrows.Remove(arrow.Key);
                        continue;
                    }
                    arrow.Value.target = player.transform.position;
                }
            }
        }
    }
}