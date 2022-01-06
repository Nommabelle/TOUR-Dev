using HarmonyLib;
using Il2CppSystem.Collections.Generic;
using TMPro;
using TownOfUs.Roles;
using TownOfUs.Roles.Modifiers;
using UnityEngine;

namespace TownOfUs.Patches
{
    public static class IntroCutScenePatch
    {
        public static TextMeshPro ModifierText;

        public static float Scale;

        [HarmonyPatch(typeof(IntroCutscene), nameof(IntroCutscene.BeginCrewmate))]
        public static class IntroCutscene_BeginCrewmate
        {
            public static void Postfix(IntroCutscene __instance)
            {
                var modifier = Modifier.GetModifier(PlayerControl.LocalPlayer);
                if (modifier != null)
                    ModifierText = Object.Instantiate(__instance.Title, __instance.Title.transform.parent, false);
                else
                    ModifierText = null;
                Lights.SetLights();
            }
        }

        [HarmonyPatch(typeof(IntroCutscene), nameof(IntroCutscene.BeginImpostor))]
        public static class IntroCutscene_BeginImpostor
        {
            public static void Postfix(IntroCutscene __instance)
            {
                var modifier = Modifier.GetModifier(PlayerControl.LocalPlayer);
                if (modifier != null)
                    ModifierText = Object.Instantiate(__instance.Title, __instance.Title.transform.parent, false);
                else
                    ModifierText = null;
                Lights.SetLights();
            }
        }

        [HarmonyPatch(typeof(IntroCutscene._CoBegin_d__14), nameof(IntroCutscene._CoBegin_d__14.MoveNext))]
        public static class IntroCutscene_CoBegin__d_MoveNext
        {
            
            public static void Postfix(IntroCutscene._CoBegin_d__14 __instance)
            {
                var role = Role.GetRole(PlayerControl.LocalPlayer);

                var alpha = __instance.__4__this.Title.color.a;
                if (role != null && !role.Hidden)
                {
                    __instance.__4__this.Title.text = role.Name;
                    __instance.__4__this.Title.color = role.Color;
                    __instance.__4__this.ImpostorText.text = role.ImpostorText();
                    __instance.__4__this.ImpostorText.gameObject.SetActive(true);
                    __instance.__4__this.BackgroundBar.material.color = role.Color;
                }

                if (ModifierText != null)
                {
                    var modifier = Modifier.GetModifier(PlayerControl.LocalPlayer);
                    if(modifier.GetType() == typeof(Lover))
                    {
                        ModifierText.text = $"<size=3>{modifier.TaskText()}</size>";
                    } 
                    else
                    {
                        ModifierText.text = "<size=4>Modifier: " + modifier.Name + "</size>";
                    }

                    ModifierText.color = modifier.Color;
                    ModifierText.transform.position =
                        __instance.__4__this.transform.position - new Vector3(0f, 2.0f, 0f);

                    ModifierText.gameObject.SetActive(true);
                }
            }
        }
    }
}
