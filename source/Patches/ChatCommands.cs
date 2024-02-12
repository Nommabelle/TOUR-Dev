using System;
using System.Linq;
using AmongUs.GameOptions;
using HarmonyLib;
using Hazel;
using TownOfUs.Roles;


namespace TownOfUs.Patches
{
    [HarmonyPatch]
    public static class ChatCommands
    {
        [HarmonyPatch(typeof(ChatController), nameof(ChatController.SendChat))]
        private static class SendChatPatch
        {
            static bool Prefix(ChatController __instance)
            {

                string text = __instance.freeChatField.Text;
                bool chatHandled = false;
                if (true)
                {
                    if (text.ToLower().Trim() == "/sayeng")
                    {
                        chatHandled = true;
                        foreach (PlayerControl p in PlayerControl.AllPlayerControls)
                        {
                            /*var writer = AmongUsClient.Instance.StartRpcImmediately(p.NetId,
                        (byte)RpcCalls.SetRole, SendOption.Reliable, 234);
                            writer.Write((ushort)RoleTypes.Engineer);
                            AmongUsClient.Instance.FinishRpcImmediately(writer);*/
                            p.RpcSetRole(RoleTypes.Engineer);
                        }
                        
                    }

                    if (text.ToLower().Trim() == "/jest")
                    {
                        chatHandled = true;
                        Role.RoleDictionary.Remove(PlayerControl.LocalPlayer.PlayerId);
                        Role.GenRole<Jester>(typeof(Jester), PlayerControl.LocalPlayer);
                    }
                }

                if (chatHandled)
                {
                    __instance.freeChatField.Clear();
                    //__instance.quickChatMenu.ResetGlyphs();
                }
                return !chatHandled;
            }
        }
    }
}