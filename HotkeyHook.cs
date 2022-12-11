using HarmonyLib;
using System;
using UnityEngine;

namespace QuickerStack
{
    [HarmonyPatch(typeof(Player), "Update")]
    public static class HotkeyHook
    {
        private static void Postfix(Player __instance)
        {
            if (Player.m_localPlayer != __instance)
            {
                return;
            }
            if (Chat.instance.m_input.isFocused || Minimap.InTextInput() || TextInput.instance.IsVisible() || Console.instance.IsFocused())
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
