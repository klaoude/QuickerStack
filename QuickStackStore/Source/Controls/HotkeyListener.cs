using BepInEx.Configuration;
using HarmonyLib;
using System.Linq;
using UnityEngine;
using static QuickStackStore.QSSConfig;

namespace QuickStackStore
{
    [HarmonyPatch]
    public static class HotkeyListener
    {
        public static bool IgnoreKeyPresses()
        {
            // removed InventoryGui.IsVisible() because we specifically want to allow that
            return IgnoreKeyPressesDueToPlayer(Player.m_localPlayer)
                || !ZNetScene.instance
                || Minimap.IsOpen()
                || Menu.IsVisible()
                || Console.IsVisible()
                || StoreGui.IsVisible()
                || TextInput.IsVisible()
                || (Chat.instance && Chat.instance.HasFocus())
                || (ZNet.instance && ZNet.instance.InPasswordDialog())
                || (TextViewer.instance && TextViewer.instance.IsVisible());
        }

        private static bool IgnoreKeyPressesDueToPlayer(Player player)
        {
            return !player
                || player.InCutscene()
                || player.IsTeleporting()
                || player.IsDead()
                || player.InPlaceMode();
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

        [HarmonyPatch(typeof(Player), nameof(Player.Update))]
        public static class Player_Update_Patch
        {
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
}