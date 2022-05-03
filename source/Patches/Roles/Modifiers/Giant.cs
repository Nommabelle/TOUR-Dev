using TownOfUs.Extensions;
using UnityEngine;

namespace TownOfUs.Roles.Modifiers
{
    public class Giant : Modifier, IVisualAlteration
    {
        public Giant(PlayerControl player) : base(player)
        {
            var slowText = CustomGameOptions.GiantSlow ? " and slow!" : "!";
            Name = "Giant";
            TaskText = () => "You are big" + slowText;
            Color = Patches.Colors.Giant;
            ModifierType = ModifierEnum.Giant;
        }

        public bool TryGetModifiedAppearance(out VisualAppearance appearance)
        {
            appearance = Player.GetDefaultAppearance();
            if (CustomGameOptions.GiantSlow) {
                appearance.SpeedFactor = 0.7f;
            }
            appearance.SizeFactor = new Vector3(1.0f, 1.0f, 1.0f);
            return true;
        }
    }
}