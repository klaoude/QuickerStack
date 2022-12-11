using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace QuickerStack
{
    [HarmonyPatch(typeof(InventoryGrid))]
    internal static class PatchInventoryGrid
    {
        [HarmonyPatch("UpdateGui")]
        [HarmonyPostfix]
        internal static void UpdateGui(InventoryGrid __instance, Player player, ItemDrop.ItemData dragItem, Inventory ___m_inventory, List<Element> ___m_elements)
        {
            if (player == null || player.GetInventory() != ___m_inventory)
            {
                return;
            }
            int width = ___m_inventory.GetWidth();
            UserConfig playerConfig = QuickerStackPlugin.GetPlayerConfig(player.GetPlayerID());
            foreach (ItemDrop.ItemData itemData in ___m_inventory.GetAllItems())
            {
                if (playerConfig.IsMarked(itemData.m_gridPos))
                {
                    int index = itemData.m_gridPos.y * width + itemData.m_gridPos.x;
                    ___m_elements[index].m_queued.enabled = true;
                }
            }
        }
        [HarmonyPatch("OnRightClick")]
        [HarmonyPrefix]
        internal static bool OnRightClick(InventoryGrid __instance, UIInputHandler element)
        {
            Player localPlayer = Player.m_localPlayer;
            if (localPlayer.IsTeleporting())
            {
                return true;
            }
            if (InventoryGui.instance.GetDragGo())
            {
                return true;
            }
            if (!Input.GetKey(KeyCode.LeftAlt) && !Input.GetKey(KeyCode.RightAlt))
            {
                return true;
            }
            GameObject gameObject = element.gameObject;
            Vector2i buttonPos = __instance.GetButtonPos(gameObject);
            ItemDrop.ItemData itemAt = __instance.GetInventory().GetItemAt(buttonPos.x, buttonPos.y);
            QuickerStackPlugin.GetPlayerConfig(localPlayer.GetPlayerID()).Toggle(itemAt.m_shared);
            return false;
        }
        [HarmonyPatch("OnLeftClick")]
        [HarmonyPrefix]
        internal static bool OnLeftClick(InventoryGrid __instance, UIInputHandler clickHandler)
        {
            if (InventoryGui.instance.GetPlayerGrid() != __instance)
            {
                return true;
            }
            Player localPlayer = Player.m_localPlayer;
            if (localPlayer.IsTeleporting())
            {
                return true;
            }
            if (InventoryGui.instance.GetDragGo())
            {
                return true;
            }
            if (!Input.GetKey(KeyCode.LeftAlt) && !Input.GetKey(KeyCode.RightAlt))
            {
                return true;
            }
            GameObject gameObject = clickHandler.gameObject;
            Vector2i buttonPos = __instance.GetButtonPos(gameObject);
            QuickerStackPlugin.GetPlayerConfig(localPlayer.GetPlayerID()).Toggle(buttonPos);
            return false;
        }
    }
}
