using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace QuickStackStore
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
            UserConfig playerConfig = QuickStackStorePlugin.GetPlayerConfig(player.GetPlayerID());

            for (int y = 0; y < ___m_inventory.GetHeight(); y++)
            {
                for (int x = 0; x < ___m_inventory.GetWidth(); x++)
                {
                    int index = y * width + x;

                    if (___m_elements[index].m_queued.transform.childCount > 0)
                    {
                        var child = ___m_elements[index].m_queued.transform.GetChild(0).GetComponent<Image>();
                        child.enabled = playerConfig.IsSlotMarked(new Vector2i(x, y));
                    }
                    else
                    {
                        // set m_queued parent as parent first, so the position is correct
                        var obj = GameObject.Instantiate<Image>(___m_elements[index].m_queued, ___m_elements[index].m_queued.transform.parent);
                        // change the parent to the m_queued image so we can access the new image without a loop
                        obj.transform.SetParent(___m_elements[index].m_queued.transform);
                        // set the new border image
                        obj.sprite = QuickStackStorePlugin.border;
                        // activate it if needed
                        obj.enabled = playerConfig.IsSlotMarked(new Vector2i(x, y));
                    }
                }
            }

            // TODO add colors to config, change color depending on if both favorites are on

            foreach (ItemDrop.ItemData itemData in ___m_inventory.GetAllItems())
            {
                int index = itemData.m_gridPos.y * width + itemData.m_gridPos.x;

                if (___m_elements[index].m_queued.transform.childCount > 0)
                {
                    var child = ___m_elements[index].m_queued.transform.GetChild(0).GetComponent<Image>();
                    child.enabled |= playerConfig.IsItemMarked(itemData.m_shared);
                }
                else
                {
                    // set m_queued parent as parent first, so the position is correct
                    var obj = GameObject.Instantiate<Image>(___m_elements[index].m_queued, ___m_elements[index].m_queued.transform.parent);
                    // change the parent to the m_queued image so we can access the new image without a loop
                    obj.transform.SetParent(___m_elements[index].m_queued.transform);
                    // set the new border image
                    obj.sprite = QuickStackStorePlugin.border;
                    // activate it if needed
                    obj.enabled |= playerConfig.IsItemMarked(itemData.m_shared);
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
            QuickStackStorePlugin.GetPlayerConfig(localPlayer.GetPlayerID()).Toggle(itemAt.m_shared);

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
            QuickStackStorePlugin.GetPlayerConfig(localPlayer.GetPlayerID()).Toggle(buttonPos);

            return false;
        }
    }
}