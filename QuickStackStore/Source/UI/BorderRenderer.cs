using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static QuickStackStore.QSSConfig;

namespace QuickStackStore
{
    [HarmonyPatch(typeof(InventoryGrid))]
    internal static class BorderRenderer
    {
        public static Sprite border;

        [HarmonyPatch(nameof(InventoryGrid.UpdateGui))]
        [HarmonyPostfix]
        internal static void UpdateGui(Player player, Inventory ___m_inventory, List<InventoryGrid.Element> ___m_elements)
        {
            if (player == null || player.GetInventory() != ___m_inventory)
            {
                return;
            }

            int width = ___m_inventory.GetWidth();
            UserConfig playerConfig = UserConfig.GetPlayerConfig(player.GetPlayerID());

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

                    img.color = FavoriteConfig.BorderColorFavoritedSlot.Value;
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

                var isItemFavorited = playerConfig.IsItemNameFavorited(itemData.m_shared);
                if (isItemFavorited)
                {
                    if (img.enabled)
                    {
                        img.color = FavoriteConfig.BorderColorFavoritedItemOnFavoritedSlot.Value;
                    }
                    else
                    {
                        img.color = FavoriteConfig.BorderColorFavoritedItem.Value;
                    }

                    // do this at the end of the if statement, so we can use img.enabled to deduce the slot favoriting
                    img.enabled |= isItemFavorited;
                }
                else
                {
                    var isItemTrashFlagged = playerConfig.IsItemNameConsideredTrashFlagged(itemData.m_shared);

                    if (isItemTrashFlagged)
                    {
                        if (img.enabled)
                        {
                            img.color = FavoriteConfig.BorderColorTrashFlaggedItemOnFavoritedSlot.Value;
                        }
                        else
                        {
                            img.color = FavoriteConfig.BorderColorTrashFlaggedItem.Value;
                        }

                        // do this at the end of the if statement, so we can use img.enabled to deduce the slot favoriting
                        img.enabled |= isItemTrashFlagged;
                    }
                }
            }
        }

        private static Image CreateBorderImage(Image baseImg)
        {
            // set m_queued parent as parent first, so the position is correct
            var obj = GameObject.Instantiate<Image>(baseImg, baseImg.transform.parent);
            // change the parent to the m_queued image so we can access the new image without a loop
            obj.transform.SetParent(baseImg.transform);
            // set the new border image
            obj.sprite = border;

            return obj;
        }

        [HarmonyPatch(nameof(InventoryGrid.OnRightClick))]
        [HarmonyPrefix]
        internal static bool OnRightClick(InventoryGrid __instance, UIInputHandler element)
        {
            return HandleClick(__instance, element, false);
        }

        [HarmonyPatch(nameof(InventoryGrid.OnLeftClick))]
        [HarmonyPrefix]
        internal static bool OnLeftClick(InventoryGrid __instance, UIInputHandler clickHandler)
        {
            return HandleClick(__instance, clickHandler, true);
        }

        internal static bool HandleClick(InventoryGrid __instance, UIInputHandler clickHandler, bool isLeftClick)
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

            if (!Helper.IsPressingFavoriteKey())
            {
                return true;
            }

            GameObject gameObject = clickHandler.gameObject;
            Vector2i buttonPos = __instance.GetButtonPos(gameObject);

            if (buttonPos == new Vector2i(-1, -1))
            {
                return true;
            }

            if (!isLeftClick)
            {
                UserConfig.GetPlayerConfig(localPlayer.GetPlayerID()).ToggleSlotFavoriting(buttonPos);
            }
            else
            {
                ItemDrop.ItemData itemAt = __instance.GetInventory().GetItemAt(buttonPos.x, buttonPos.y);

                if (itemAt == null)
                {
                    return true;
                }

                bool wasToggleSuccessful = UserConfig.GetPlayerConfig(localPlayer.GetPlayerID()).ToggleItemNameFavoriting(itemAt.m_shared);

                if (!wasToggleSuccessful)
                {
                    localPlayer.Message(MessageHud.MessageType.Center, LocalizationConfig.CantFavoriteTrashFlaggedItemWarning.Value, 0, null);
                }
            }

            return false;
        }
    }

    [HarmonyPatch(typeof(Inventory))]
    internal class PatchInventory
    {
        [HarmonyPatch(nameof(Inventory.TopFirst))]
        [HarmonyPrefix]
        [HarmonyPriority(Priority.HigherThanNormal)]
        public static bool TopFirstPatch(ref bool __result)
        {
            if (GeneralConfig.UseTopDownLogicForEverything.Value)
            {
                __result = true;
                return false;
            }
            else
            {
                return true;
            }
        }
    }
}