using HarmonyLib;
using System;
using UnityEngine;

namespace QuickerStack
{
    [HarmonyPatch(typeof(Player), "Update")]
    public static class HotkeyHook
    {
        // Token: 0x0600001F RID: 31 RVA: 0x00002A94 File Offset: 0x00000C94
        private static void Postfix(Player __instance)
        {
            if (Player.m_localPlayer != __instance)
            {
                return;
            }
            if (Chat.instance.m_input.isFocused || Minimap.InTextInput())
            {
                return;
            }
            if (Input.GetKeyDown(QuickerStackPlugin.QuickStackKey))
            {
                QuickerStackPlugin.DoQuickStack(__instance);
            }
        }
    }
}
