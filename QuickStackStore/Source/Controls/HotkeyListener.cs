using BepInEx.Configuration;
using HarmonyLib;
using System.Linq;
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

        // thank you to 'Margmas' for giving me this snippet from VNEI https://github.com/MSchmoecker/VNEI/blob/master/VNEI/Logic/BepInExExtensions.cs#L21
        // since KeyboardShortcut.IsPressed and KeyboardShortcut.IsDown behave unintuitively
        public static bool IsKeyDown(KeyboardShortcut shortcut)
        {
            return shortcut.MainKey != KeyCode.None && Input.GetKeyDown(shortcut.MainKey) && shortcut.Modifiers.All(Input.GetKey);
        }

        public static bool IsKeyHeld(KeyboardShortcut shortcut)
        {
            return shortcut.MainKey != KeyCode.None && Input.GetKey(shortcut.MainKey) && shortcut.Modifiers.All(Input.GetKey);
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

            if (IsKeyDown(QuickStackConfig.QuickStackKeybind.Value))
            {
                QuickStackModule.DoQuickStack(__instance);
            }
            else if (IsKeyDown(RestockConfig.RestockKeybind.Value))
            {
                RestockModule.DoRestock(__instance);
            }

            if (!InventoryGui.IsVisible())
            {
                return;
            }

            if (IsKeyDown(SortConfig.SortKeybind.Value))
            {
                SortModule.DoSort(__instance);
            }
            else if (IsKeyDown(TrashConfig.QuickTrashKeybind.Value))
            {
                TrashModule.TrashOrTrashFlagItem(true);
            }
            else if (IsKeyDown(TrashConfig.TrashKeybind.Value))
            {
                TrashModule.AttemptQuickTrash();
            }
        }
    }
}