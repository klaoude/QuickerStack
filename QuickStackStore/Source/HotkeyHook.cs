using HarmonyLib;
using UnityEngine;

namespace QuickStackStore
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

            if (Chat.instance.m_input.isFocused || Minimap.InTextInput())
            {
                return;
            }

            if (Input.GetKeyDown(QuickStackStorePlugin.QuickStackKey))
            {
                QuickStackStorePlugin.DoQuickStack(__instance);
            }
        }
    }
}