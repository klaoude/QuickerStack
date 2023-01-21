using HarmonyLib;
using UnityEngine;
using static QuickStackStore.QSSConfig;

namespace QuickStackStore
{
    [HarmonyPatch(typeof(Player), nameof(Player.Update))]
    public static class HotkeyListener
    {
        // taken from https://github.com/aedenthorn/ValheimMods AedenthornUtils.IgnoreKeyPresses, public domain
        public static bool IgnoreKeyPresses()
        {
            // removed InventoryGui.IsVisible() because we specifically want that to be the case
            return ZNetScene.instance == null || Player.m_localPlayer == null || Minimap.IsOpen() || Console.IsVisible() || TextInput.IsVisible() || ZNet.instance.InPasswordDialog() || Chat.instance == null || Chat.instance.HasFocus() || StoreGui.IsVisible() || Menu.IsVisible() || TextViewer.instance == null || TextViewer.instance.IsVisible();
        }

        public static void Postfix(Player __instance)
        {
            if (GeneralConfig.OverrideKeybindBehavior.Value == OverrideKeybindBehavior.DisableAllNewHotkeys)
            {
                return;
            }

            if (Player.m_localPlayer != __instance)
            {
                return;
            }

            if (IgnoreKeyPresses())
            {
                return;
            }

            if (Input.GetKeyDown(QuickStackConfig.QuickStackKey.Value))
            {
                QuickStackRestockModule.DoQuickStack(__instance);
            }
            else if (Input.GetKeyDown(RestockConfig.RestockKey.Value))
            {
                QuickStackRestockModule.DoRestock(__instance);
            }

            if (!InventoryGui.IsVisible())
            {
                return;
            }

            if (Input.GetKeyDown(SortConfig.SortKey.Value))
            {
                SortModule.DoSort(__instance);
            }
            else if (Input.GetKeyDown(TrashConfig.TrashHotkey.Value))
            {
                TrashModule.TrashOrTrashFlagItem(true);
            }
            else if (Input.GetKeyDown(TrashConfig.QuickTrashHotkey.Value))
            {
                TrashModule.AttemptQuickTrash();
            }
        }
    }
}