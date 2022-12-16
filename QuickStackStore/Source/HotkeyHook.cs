using HarmonyLib;
using UnityEngine;

namespace QuickStackStore
{
    [HarmonyPatch(typeof(Player), "Update")]
    public static class HotkeyHook
    {
        private static void Postfix(Player __instance)
        {
            if (QuickStackStorePlugin.DisableAllNewKeybinds)
            {
                return;
            }

            if (Player.m_localPlayer != __instance)
            {
                return;
            }

            if (Chat.instance.m_input.isFocused || Minimap.InTextInput() || TextInput.instance.m_visibleFrame)
            {
                return;
            }

            if (Input.GetKeyDown(QuickStackStorePlugin.QuickStackKey))
            {
                QuickStackStorePlugin.DoQuickStack(__instance);
            }

            if (Input.GetKeyDown(QuickStackStorePlugin.SortKey))
            {
                QuickStackStorePlugin.DoSort(__instance);
            }

            if (Input.GetKeyDown(TrashItems.TrashHotkey))
            {
                TrashItems.TrashItem();
            }
        }
    }
}