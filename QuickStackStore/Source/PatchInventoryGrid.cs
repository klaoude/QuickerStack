using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace QuickStackStore
{
    [HarmonyPatch(typeof(InventoryGrid))]
    internal static class PatchInventoryGrid
    {
        [HarmonyPatch(nameof(InventoryGrid.UpdateGui))]
        [HarmonyPostfix]
        internal static void UpdateGui(Player player, Inventory ___m_inventory, List<InventoryGrid.Element> ___m_elements)
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

                    Image img;

                    if (___m_elements[index].m_queued.transform.childCount > 0)
                    {
                        img = ___m_elements[index].m_queued.transform.GetChild(0).GetComponent<Image>();
                    }
                    else
                    {
                        img = CreateBorderImage(___m_elements[index].m_queued);
                    }

                    img.color = QuickStackStorePlugin.BorderColorFavoriteSlot;
                    img.enabled = playerConfig.IsSlotFavorited(new Vector2i(x, y));
                }
            }

            foreach (ItemDrop.ItemData itemData in ___m_inventory.GetAllItems())
            {
                int index = itemData.GetIndexFromItemData(width);

                Image img;

                if (___m_elements[index].m_queued.transform.childCount > 0)
                {
                    img = ___m_elements[index].m_queued.transform.GetChild(0).GetComponent<Image>();
                }
                else
                {
                    img = CreateBorderImage(___m_elements[index].m_queued);
                }

                if (playerConfig.IsItemNameFavorited(itemData.m_shared))
                {
                    if (img.enabled)
                    {
                        img.color = QuickStackStorePlugin.BorderColorFavoriteBoth;
                    }
                    else
                    {
                        img.color = QuickStackStorePlugin.BorderColorFavoriteItem;
                    }
                }

                // do this after the IsItemFavorited if statement, so we can use img.enabled to deduce the slot favoriting
                img.enabled |= playerConfig.IsItemNameFavorited(itemData.m_shared);
            }
        }

        private static Image CreateBorderImage(Image baseImg)
        {
            // set m_queued parent as parent first, so the position is correct
            var obj = GameObject.Instantiate<Image>(baseImg, baseImg.transform.parent);
            // change the parent to the m_queued image so we can access the new image without a loop
            obj.transform.SetParent(baseImg.transform);
            // set the new border image
            obj.sprite = QuickStackStorePlugin.border;

            return obj;
        }

        private static bool IsPressingFavoriteKey()
        {
            return Input.GetKey(QuickStackStorePlugin.FavoriteModifierKey1) || Input.GetKey(QuickStackStorePlugin.FavoriteModifierKey2);
        }

        [HarmonyPatch(nameof(InventoryGrid.OnRightClick))]
        [HarmonyPrefix]
        internal static bool OnRightClick(InventoryGrid __instance, UIInputHandler element)
        {
            Player localPlayer = Player.m_localPlayer;
            if (localPlayer.IsTeleporting())
            {
                return true;
            }

            if (InventoryGui.instance.m_dragGo)
            {
                return true;
            }

            if (!IsPressingFavoriteKey())
            {
                return true;
            }

            GameObject gameObject = element.gameObject;
            Vector2i buttonPos = __instance.GetButtonPos(gameObject);
            ItemDrop.ItemData itemAt = __instance.GetInventory().GetItemAt(buttonPos.x, buttonPos.y);
            QuickStackStorePlugin.GetPlayerConfig(localPlayer.GetPlayerID()).Toggle(itemAt.m_shared);

            return false;
        }

        [HarmonyPatch(nameof(InventoryGrid.OnLeftClick))]
        [HarmonyPrefix]
        internal static bool OnLeftClick(InventoryGrid __instance, UIInputHandler clickHandler)
        {
            if (InventoryGui.instance.m_playerGrid != __instance)
            {
                return true;
            }

            Player localPlayer = Player.m_localPlayer;

            if (localPlayer.IsTeleporting())
            {
                return true;
            }

            if (InventoryGui.instance.m_dragGo)
            {
                return true;
            }

            if (!IsPressingFavoriteKey())
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