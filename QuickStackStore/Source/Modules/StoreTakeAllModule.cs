using System.Collections.Generic;
using System.Linq;
using static ItemDrop;
using static QuickStackStore.QSSConfig;

namespace QuickStackStore
{
    internal class StoreTakeAllModule
    {
        private static bool ShouldStoreItem(ItemData item, UserConfig playerConfig, int inventoryHeight, bool includeHotbar)
        {
            return (item.m_gridPos.y > 0 || includeHotbar)
                && (StoreTakeAllConfig.StoreAllIncludesEquippedItems.Value || !item.m_equiped)
                && !playerConfig.IsItemNameOrSlotFavorited(item)
                && !CompatibilitySupport.IsEquipOrQuickSlot(inventoryHeight, item.m_gridPos);
        }

        public static void ContextSensitiveTakeAll(InventoryGui instance)
        {
            if (instance.m_currentContainer)
            {
                if (!StoreTakeAllConfig.ChestsUseImprovedTakeAllLogic.Value || instance.m_currentContainer.GetComponent<TombStone>())
                {
                    instance.OnTakeAll();
                }
                else
                {
                    TakeAllItemsInOrder(Player.m_localPlayer);
                }
            }
        }

        private static void TakeAllItemsInOrder(Player player)
        {
            Inventory fromInventory = InventoryGui.instance.m_currentContainer.m_inventory;
            Inventory toInventory = player.m_inventory;

            MoveAllItemsInOrder(player, fromInventory, toInventory, true);
        }

        internal static void StoreAllItemsInOrder(Player player)
        {
            Inventory fromInventory = player.m_inventory;
            Inventory toInventory = InventoryGui.instance.m_currentContainer.m_inventory;

            MoveAllItemsInOrder(player, fromInventory, toInventory);
        }

        internal static void MoveAllItemsInOrder(Player player, Inventory fromInventory, Inventory toInventory, bool takeAllOverride = false)
        {
            if (player.IsTeleporting() || !InventoryGui.instance.m_container)
            {
                return;
            }

            InventoryGui.instance.SetupDragItem(null, null, 0);

            List<ItemData> list;

            if (takeAllOverride)
            {
                list = new List<ItemData>(fromInventory.m_inventory);
            }
            else
            {
                UserConfig playerConfig = UserConfig.GetPlayerConfig(player.GetPlayerID());
                var includeHotbar = GeneralConfig.OverrideHotkeyBarBehavior.Value != OverrideHotkeyBarBehavior.NeverAffectHotkeyBar && StoreTakeAllConfig.StoreAllIncludesHotkeyBar.Value;

                list = fromInventory.m_inventory.Where((item) => ShouldStoreItem(item, playerConfig, fromInventory.GetHeight(), includeHotbar)).ToList();
            }

            list.Sort((ItemData a, ItemData b) => Helper.CompareSlotOrder(a.m_gridPos, b.m_gridPos));

            int num = 0;

            foreach (ItemData itemData in list)
            {
                if (toInventory.AddItem(itemData))
                {
                    fromInventory.RemoveItem(itemData);
                    num++;

                    if (itemData.m_equiped)
                    {
                        Player.m_localPlayer.RemoveEquipAction(itemData);
                        Player.m_localPlayer.UnequipItem(itemData, false);
                    }
                }
            }

            if (takeAllOverride)
            {
                Helper.Log($"Moved {num} item/s from container to player inventory");
            }
            else
            {
                Helper.Log($"Moved {num} item/s from player inventory to container");
            }

            toInventory.Changed();
            fromInventory.Changed();
        }
    }
}