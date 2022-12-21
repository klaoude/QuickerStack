using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static ItemDrop;
using static QuickStackStore.QSSConfig;

namespace QuickStackStore
{
    internal class QuickStackRestockModule
    {
        private static bool ShouldRestockItem(ItemData item, UserConfig playerConfig)
        {
            var maxStack = item.m_shared.m_maxStackSize;
            var type = item.m_shared.m_itemType;

            return maxStack > 1 && maxStack > item.m_stack
                && (!RestockConfig.RestockOnlyAmmoAndConsumables.Value || type == ItemData.ItemType.Ammo || type == ItemData.ItemType.Consumable)
                && ((GeneralConfig.OverrideHotkeyBarBehavior.Value != OverrideHotkeyBarBehavior.NeverAffectHotkeyBar && RestockConfig.RestockIncludesHotkeyBar.Value) || item.m_gridPos.y > 0)
                && (!RestockConfig.RestockOnlyFavoritedItems.Value || playerConfig.IsItemNameOrSlotFavorited(item))
                && !CompatibilitySupport.IsEquipOrQuickSlot(item.m_gridPos, true);
        }

        private static bool ShouldQuickStackItem(ItemData item, UserConfig playerConfig)
        {
            return item.m_shared.m_maxStackSize > 1
                && ((GeneralConfig.OverrideHotkeyBarBehavior.Value != OverrideHotkeyBarBehavior.NeverAffectHotkeyBar && QuickStackConfig.QuickStackIncludesHotkeyBar.Value) || item.m_gridPos.y > 0)
                && !playerConfig.IsItemNameOrSlotFavorited(item) && !CompatibilitySupport.IsEquipOrQuickSlot(item.m_gridPos);
        }

        private static int RestockFromThisContainer(List<ItemData> firstItemList, List<ItemData> unusedList, Inventory playerInventory, Inventory container, HashSet<Vector2i> partialStackCounter)
        {
            int num = 0;

            if (firstItemList?.Count <= 0)
            {
                return num;
            }

            for (int i = firstItemList.Count - 1; i >= 0; i--)
            {
                var pItem = firstItemList[i];

                for (int j = container.m_inventory.Count - 1; j >= 0; j--)
                {
                    var cItem = container.m_inventory[j];

                    // stackables can't have quality
                    if (cItem.m_shared.m_name == pItem.m_shared.m_name)
                    {
                        int itemsToMove = Math.Min(pItem.m_shared.m_maxStackSize - pItem.m_stack, cItem.m_stack);
                        pItem.m_stack += itemsToMove;

                        partialStackCounter.Add(pItem.m_gridPos);

                        if (cItem.m_stack == itemsToMove)
                        {
                            container.m_inventory.Remove(cItem);
                        }
                        else
                        {
                            cItem.m_stack -= itemsToMove;
                        }

                        if (pItem.m_stack == pItem.m_shared.m_maxStackSize)
                        {
                            firstItemList.RemoveAt(i);
                            num++;
                            break;
                        }
                    }
                }
            }

            container.Changed();
            playerInventory.Changed();

            return num;
        }

        private static int StackItemsIntoThisContainer(List<ItemData> firstItemList, List<ItemData> secondItemList, Inventory playerInventory, Inventory container, HashSet<Vector2i> unusedCounter)
        {
            int num = 0;

            if (QuickStackConfig.QuickStackTrophiesIntoSameContainer.Value && firstItemList?.Count > 0)
            {
                for (int i = container.m_inventory.Count - 1; i >= 0; i--)
                {
                    var cItem = container.m_inventory[i];

                    if (cItem.m_shared.m_itemType != ItemData.ItemType.Trophie)
                    {
                        continue;
                    }
                    else
                    {
                        for (int j = firstItemList.Count - 1; j >= 0; j--)
                        {
                            var pItem = firstItemList[j];

                            if (container.AddItem(pItem))
                            {
                                playerInventory.m_inventory.Remove(pItem);
                                firstItemList.RemoveAt(j);
                                num++;
                            }
                        }

                        break;
                    }
                }
            }

            if (secondItemList?.Count > 0)
            {
                for (int i = container.m_inventory.Count - 1; i >= 0; i--)
                {
                    var cItem = container.m_inventory[i];

                    for (int j = secondItemList.Count - 1; j >= 0; j--)
                    {
                        var pItem = secondItemList[j];

                        // stackables can't have quality
                        if (cItem.m_shared.m_name == pItem.m_shared.m_name)
                        {
                            if (container.AddItem(pItem))
                            {
                                playerInventory.m_inventory.Remove(pItem);
                                secondItemList.RemoveAt(j);
                                num++;
                            }
                        }
                    }
                }
            }

            container.Changed();
            playerInventory.Changed();

            return num;
        }

        internal static void DoRestock(Player player, bool RestockOnlyFromCurrentContainerOverride = false)
        {
            if (player.IsTeleporting() || !InventoryGui.instance.m_container)
            {
                return;
            }

            InventoryGui.instance.SetupDragItem(null, null, 0);

            UserConfig playerConfig = UserConfig.GetPlayerConfig(player.GetPlayerID());

            List<ItemData> restockables = player.m_inventory.m_inventory.Where((itm) => ShouldRestockItem(itm, playerConfig)).ToList();

            int totalCount = restockables.Count;

            if (totalCount == 0 && RestockConfig.ShowRestockResultMessage.Value)
            {
                player.Message(MessageHud.MessageType.Center, LocalizationConfig.RestockResultMessageNothing.Value, 0, null);
                return;
            }

            // sort in reverse, because we iterate in reverse
            restockables.Sort((ItemData a, ItemData b) => -1 * Helper.CompareSlotOrder(a.m_gridPos, b.m_gridPos));

            int completelyFilledStacks = 0;
            var partiallyFilledStacks = new HashSet<Vector2i>();
            Container container = InventoryGui.instance.m_currentContainer;

            if (container != null)
            {
                completelyFilledStacks = RestockFromThisContainer(restockables, null, player.m_inventory, container.m_inventory, partiallyFilledStacks);

                if (RestockConfig.RestockHotkeyBehaviorWhenContainerOpen.Value == RestockBehavior.RestockOnlyFromCurrentContainer || RestockOnlyFromCurrentContainerOverride)
                {
                    ReportRestockResult(player, completelyFilledStacks, partiallyFilledStacks.Count, totalCount);
                    return;
                }
            }

            List<Container> containers = ContainerFinder.FindContainersInRange(player.transform.position, RestockConfig.RestockFromNearbyRange.Value);

            if (containers.Count > 0)
            {
                completelyFilledStacks += ApplyToMultipleContainers(RestockFromThisContainer, restockables, null, player, containers, partiallyFilledStacks);
            }

            ReportRestockResult(player, completelyFilledStacks, partiallyFilledStacks.Count, totalCount);
        }

        internal static void DoQuickStack(Player player, bool QuickStackOnlyToCurrentContainerOverride = false)
        {
            if (player.IsTeleporting() || !InventoryGui.instance.m_container)
            {
                return;
            }

            InventoryGui.instance.SetupDragItem(null, null, 0);

            UserConfig playerConfig = UserConfig.GetPlayerConfig(player.GetPlayerID());

            List<ItemData> quickStackables = player.m_inventory.m_inventory.Where((itm) => ShouldQuickStackItem(itm, playerConfig)).ToList();

            if (quickStackables.Count == 0 && QuickStackConfig.ShowQuickStackResultMessage.Value)
            {
                player.Message(MessageHud.MessageType.Center, LocalizationConfig.QuickStackResultMessageNothing.Value, 0, null);
                return;
            }

            // sort in reverse, because we iterate in reverse
            quickStackables.Sort((ItemData a, ItemData b) => -1 * Helper.CompareSlotOrder(a.m_gridPos, b.m_gridPos));

            List<ItemData> trophies = null;

            if (QuickStackConfig.QuickStackTrophiesIntoSameContainer.Value)
            {
                trophies = new List<ItemData>();

                for (int i = quickStackables.Count - 1; i >= 0; i--)
                {
                    var item = quickStackables[i];

                    if (item.m_shared.m_itemType == ItemData.ItemType.Trophie)
                    {
                        quickStackables.RemoveAt(i);
                        // add at beginning to keep the same order of the already sorted list
                        trophies.Insert(0, item);
                    }
                }
            }

            int movedCount = 0;
            Container container = InventoryGui.instance.m_currentContainer;

            if (container != null)
            {
                movedCount = StackItemsIntoThisContainer(trophies, quickStackables, player.m_inventory, container.m_inventory, null);

                if (QuickStackConfig.QuickStackHotkeyBehaviorWhenContainerOpen.Value == QuickStackBehavior.QuickStackOnlyToCurrentContainer || QuickStackOnlyToCurrentContainerOverride)
                {
                    ReportQuickStackResult(player, movedCount);
                    return;
                }
            }

            List<Container> containers = ContainerFinder.FindContainersInRange(player.transform.position, QuickStackConfig.QuickStackToNearbyRange.Value);

            if (containers.Count > 0)
            {
                movedCount += ApplyToMultipleContainers(StackItemsIntoThisContainer, trophies, quickStackables, player, containers, null);
            }

            ReportQuickStackResult(player, movedCount);
        }

        public static void ReportRestockResult(Player player, int movedCount, int partiallyFilledCount, int totalCount)
        {
            if (!RestockConfig.ShowRestockResultMessage.Value)
            {
                return;
            }

            string message;

            if (movedCount == 0 && partiallyFilledCount == 0)
            {
                message = string.Format(LocalizationConfig.RestockResultMessageNone.Value, totalCount);
            }
            else if (movedCount < totalCount)
            {
                message = string.Format(LocalizationConfig.RestockResultMessagePartial.Value, partiallyFilledCount, totalCount);
            }
            else if (movedCount == totalCount)
            {
                message = string.Format(LocalizationConfig.RestockResultMessageFull.Value, totalCount);
            }
            else
            {
                message = $"Invalid restock: Restocked more items than we originally had ({movedCount}/{totalCount})";
                Debug.Log(message);
            }

            player.Message(MessageHud.MessageType.Center, message, 0, null);
        }

        public static void ReportQuickStackResult(Player player, int movedCount)
        {
            if (!QuickStackConfig.ShowQuickStackResultMessage.Value)
            {
                return;
            }

            string message;

            if (movedCount == 0)
            {
                message = LocalizationConfig.QuickStackResultMessageNone.Value;
            }
            else if (movedCount == 1)
            {
                message = LocalizationConfig.QuickStackResultMessageOne.Value;
            }
            else
            {
                message = string.Format(LocalizationConfig.QuickStackResultMessageMore.Value, movedCount);
            }

            player.Message(MessageHud.MessageType.Center, message, 0, null);
        }

        private static int ApplyToMultipleContainers(Func<List<ItemData>, List<ItemData>, Inventory, Inventory, HashSet<Vector2i>, int> method, List<ItemData> firstList, List<ItemData> secondList, Player player, List<Container> containers, HashSet<Vector2i> differenceCounter)
        {
            int num = 0;

            foreach (Container container in containers)
            {
                ZNetView nview = container.m_nview;

                // this is mostly used to sync the animation but its the best we got
                var inUse = nview.GetZDO().GetInt("InUse", 0) == 1;
                // container.IsInUse is only client side
                var inUseClientSide = container.IsInUse() || (container.m_wagon && container.m_wagon.InUse());

                // ownership is useless since every container always has an owner (its last user)

                // based on Container.Interact
                if (!inUse && !inUseClientSide && container.CheckAccess(player.GetPlayerID())
                    && (!container.m_checkGuardStone || PrivateArea.CheckAccess(container.transform.position, 0f, true, false)))
                {
                    nview.ClaimOwnership();

                    if (GeneralConfig.SuppressContainerSoundAndVisuals.Value)
                    {
                        container.m_inUse = true;
                        nview.GetZDO().Set("InUse", container.m_inUse ? 1 : 0);
                        ZDOMan.instance.ForceSendZDO(ZNet.instance.GetUID(), nview.GetZDO().m_uid);

                        num += method(firstList, secondList, player.m_inventory, container.m_inventory, differenceCounter);

                        container.m_inUse = false;
                        nview.GetZDO().Set("InUse", container.m_inUse ? 1 : 0);
                        ZDOMan.instance.ForceSendZDO(ZNet.instance.GetUID(), nview.GetZDO().m_uid);
                    }
                    else
                    {
                        container.SetInUse(true);
                        ZDOMan.instance.ForceSendZDO(ZNet.instance.GetUID(), nview.GetZDO().m_uid);

                        num += method(firstList, secondList, player.m_inventory, container.m_inventory, differenceCounter);

                        container.SetInUse(false);
                        ZDOMan.instance.ForceSendZDO(ZNet.instance.GetUID(), nview.GetZDO().m_uid);
                    }
                }
            }

            return num;
        }
    }
}