namespace TownOfUs.Roles
{
    public class Amnesiac : Role
    {
        public Amnesiac(PlayerControl player) : base(player)
        {
            Name = "Amnesiac";
            ImpostorText = () => "Remember a role of a deceased player";
            TaskText = () => "Remember who you were.\nFake Tasks:";
            Color = Patches.Colors.Amnesiac;
            RoleType = RoleEnum.Amnesiac;
            Faction = Faction.Neutral;
        }

        public DeadBody CurrentTarget;

        public void Loses()
        {
            Player.Data.IsImpostor = true;
        }

        protected override void IntroPrefix(IntroCutscene._CoBegin_d__14 __instance)
        {
            var amnesiacTeam = new Il2CppSystem.Collections.Generic.List<PlayerControl>();
            amnesiacTeam.Add(PlayerControl.LocalPlayer);
            __instance.yourTeam = amnesiacTeam;
        }
    }
}