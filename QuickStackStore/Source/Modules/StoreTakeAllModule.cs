using System.Linq;
using UnityEngine;
using static ItemDrop;
using static QuickStackStore.QSSConfig;

namespace QuickStackStore
{
    internal class StoreTakeAllModule
    {
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

        internal static bool ShouldMoveItem(ItemData item, UserConfig playerConfig, bool takeAllOverride = false)
        {
            return takeAllOverride ||
                (((GeneralConfig.OverrideHotkeyBarBehavior.Value != OverrideHotkeyBarBehavior.NeverAffectHotkeyBar && StoreTakeAllConfig.StoreAllIncludesHotkeyBar.Value) || item.m_gridPos.y > 0)
                && (StoreTakeAllConfig.StoreAllIncludesEquippedItems.Value || !item.m_equiped)
                && !playerConfig.IsItemNameOrSlotFavorited(item)
                && !CompatibilitySupport.IsEquipOrQuickSlot(item.m_gridPos));
        }

        internal static void MoveAllItemsInOrder(Player player, Inventory fromInventory, Inventory toInventory, bool takeAllOverride = false)
        {
            if (player.IsTeleporting() || !InventoryGui.instance.m_container)
            {
                return;
            }

            InventoryGui.instance.SetupDragItem(null, null, 0);

            UserConfig playerConfig = UserConfig.GetPlayerConfig(player.GetPlayerID());
            var list = fromInventory.m_inventory.Where((item) => ShouldMoveItem(item, playerConfig, takeAllOverride)).ToList();

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

            Debug.Log($"Moved {num} item/s to container");

            toInventory.Changed();
            fromInventory.Changed();
        }
    }
}