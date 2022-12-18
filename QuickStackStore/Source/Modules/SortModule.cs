using System;
using System.Collections.Generic;
using System.Linq;
using static QuickStackStore.QSSConfig;

namespace QuickStackStore
{
    public static class SortModule
    {
        public static void DoSort(Player player)
        {
            Container container = InventoryGui.instance.m_currentContainer;

            if (container != null)
            {
                switch (SortConfig.SortHotkeyBehaviorWhenContainerOpen.Value)
                {
                    case SortBehavior.OnlySortContainer:
                        Sort(InventoryGui.instance.m_currentContainer.GetInventory());
                        break;

                    case SortBehavior.SortBoth:
                        Sort(InventoryGui.instance.m_currentContainer.GetInventory());
                        Sort(player.GetInventory(), player);
                        break;
                }
            }
            else
            {
                Sort(player.GetInventory(), player);
            }
        }

        public static IComparable SortByGetter(ItemDrop.ItemData item)
        {
            switch (SortConfig.SortCriteria.Value)
            {
                case SortCriteriaEnum.TranslatedName:
                    return Localization.instance.Localize(item.m_shared.m_name);

                case SortCriteriaEnum.Value:
                    return item.m_shared.m_value;

                case SortCriteriaEnum.Weight:
                    return item.m_shared.m_weight;

                case SortCriteriaEnum.Type:
                    return item.m_shared.m_itemType;

                case SortCriteriaEnum.InternalName:
                default:
                    return item.m_shared.m_name;
            }
        }

        private static bool IsOutsideOfOffset(Vector2i itemPos, Vector2i offset)
        {
            return itemPos.y > offset.y || (itemPos.y == offset.y && itemPos.x >= offset.x);
        }

        private static bool ShouldSortItem(ItemDrop.ItemData item, Vector2i offset, UserConfig playerConfig = null)
        {
            return (playerConfig == null || !playerConfig.IsItemNameOrSlotFavorited(item)) && IsOutsideOfOffset(item.m_gridPos, offset) && !CompatibilitySupport.IsEquipOrQuickSlot(item.m_gridPos);
        }

        public static int SortCompare(ItemDrop.ItemData a, ItemDrop.ItemData b)
        {
            int comp = SortByGetter(a).CompareTo(SortByGetter(b));

            if (!SortConfig.SortInAscendingOrder.Value)
            {
                comp *= -1;
            }

            if (comp == 0)
            {
                comp = a.m_shared.m_name.CompareTo(b.m_shared.m_name);
            }

            if (comp == 0)
            {
                comp = -a.m_quality.CompareTo(b.m_quality);
            }

            if (comp == 0)
            {
                comp = -a.m_stack.CompareTo(b.m_stack);
            }

            return comp;
        }

        public static void Sort(Inventory inventory, Player player = null)
        {
            //var sw = new System.Diagnostics.Stopwatch();
            //sw.Start();

            var blockedSlots = new List<Vector2i>();
            UserConfig playerConfig = null;

            if (player != null)
            {
                playerConfig = UserConfig.GetPlayerConfig(player.GetPlayerID());

                if (SortConfig.SortLeavesEmptyFavoritedSlotsEmpty.Value)
                {
                    foreach (var item in inventory.GetAllItems())
                    {
                        if (playerConfig.IsItemNameFavorited(item.m_shared))
                        {
                            blockedSlots.Add(item.m_gridPos);
                        }
                    }

                    blockedSlots.AddRange(playerConfig.GetFavoritedSlotsCopy());
                }
                else
                {
                    foreach (var item in inventory.GetAllItems())
                    {
                        if (playerConfig.IsItemNameOrSlotFavorited(item))
                        {
                            blockedSlots.Add(item.m_gridPos);
                        }
                    }
                }
            }

            bool ignoreFirstRow = player != null && (GeneralConfig.NeverAffectHotkeyBar.Value || !SortConfig.SortIncludesHotkeyBar.Value);

            // simple ignore hotbar
            var offset = ignoreFirstRow ? new Vector2i(0, 1) : new Vector2i(0, 0);

            var toSort = inventory.GetAllItems().Where(item => ShouldSortItem(item, offset, playerConfig)).ToList();

            toSort.Sort((a, b) => SortCompare(a, b));

            if (SortConfig.SortMergesStacks.Value)
            {
                MergeStacks(toSort, inventory);
            }

            bool td = GeneralConfig.UseTopDownLogicForEverything.Value;

            int currentIndex = 0;
            var width = inventory.GetWidth();

            int y;
            int max;

            if (td)
            {
                // top down
                y = ignoreFirstRow ? 1 : 0;
                max = inventory.GetHeight();
            }
            else
            {
                // bottom up
                y = inventory.GetHeight() - 1;
                max = ignoreFirstRow ? 1 : 0;
            }

            for (; currentIndex < toSort.Count() && ((td && y < max) || (!td && y >= max)); y += td ? 1 : -1)
            {
                for (int x = 0; x < width && currentIndex < toSort.Count(); x++)
                {
                    var pos = new Vector2i(x, y);

                    if (!blockedSlots.Contains(pos))
                    {
                        toSort.ElementAt(currentIndex).m_gridPos = pos;
                        currentIndex++;
                    }
                }
            }

            //sw.Stop();

            // Clear the cache in case anyone is using something that loads plugins at run-time.
            CompatibilitySupport.cache.Clear();

            inventory.Changed();
        }

        internal static void MergeStacks(List<ItemDrop.ItemData> toMerge, Inventory inventory)
        {
            var grouped = toMerge.Where(itm => itm.m_stack < itm.m_shared.m_maxStackSize).GroupBy(itm => itm.m_shared.m_name).Where(itm => itm.Count() > 1).Select(grouping => grouping.ToList());

            foreach (var nonFullStacks in grouped)
            {
                var maxStack = nonFullStacks.First().m_shared.m_maxStackSize;

                var numTimes = 0;
                var curStack = nonFullStacks[0];
                nonFullStacks.RemoveAt(0);

                var enumerator = nonFullStacks.GetEnumerator();

                while (nonFullStacks.Count >= 1)
                {
                    numTimes += 1;
                    enumerator.MoveNext();
                    var stack = enumerator.Current;
                    if (stack == null)
                    {
                        break;
                    }

                    if (curStack.m_stack >= maxStack)
                    {
                        curStack = stack;
                        nonFullStacks.Remove(stack);
                        enumerator = nonFullStacks.GetEnumerator();
                        continue;
                    }

                    var toStack = Math.Min(maxStack - curStack.m_stack, stack.m_stack);

                    if (toStack > 0)
                    {
                        curStack.m_stack += toStack;
                        stack.m_stack -= toStack;

                        if (stack.m_stack <= 0)
                        {
                            inventory.RemoveItem(stack);
                        }
                    }
                }
            }
        }
    }
}