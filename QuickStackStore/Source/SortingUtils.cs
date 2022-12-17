using System;
using System.Collections.Generic;
using System.Linq;

namespace QuickStackStore
{
    public static class SortingUtilts
    {
        // TODO sort by item category?
        //private static void Test(ItemDrop.ItemData item)
        //{
        //    switch (item.m_shared.m_itemType)
        //    {
        //        case ItemDrop.ItemData.ItemType.None:
        //            break;
        //        case ItemDrop.ItemData.ItemType.Material:
        //            break;
        //        case ItemDrop.ItemData.ItemType.Consumable:
        //            break;
        //        case ItemDrop.ItemData.ItemType.OneHandedWeapon:
        //            break;
        //        case ItemDrop.ItemData.ItemType.Bow:
        //            break;
        //        case ItemDrop.ItemData.ItemType.Shield:
        //            break;
        //        case ItemDrop.ItemData.ItemType.Helmet:
        //            break;
        //        case ItemDrop.ItemData.ItemType.Chest:
        //            break;
        //        case ItemDrop.ItemData.ItemType.Ammo:
        //            break;
        //        case ItemDrop.ItemData.ItemType.Customization:
        //            break;
        //        case ItemDrop.ItemData.ItemType.Legs:
        //            break;
        //        case ItemDrop.ItemData.ItemType.Hands:
        //            break;
        //        case ItemDrop.ItemData.ItemType.Trophie:
        //            break;
        //        case ItemDrop.ItemData.ItemType.TwoHandedWeapon:
        //            break;
        //        case ItemDrop.ItemData.ItemType.Torch:
        //            break;
        //        case ItemDrop.ItemData.ItemType.Misc:
        //            break;
        //        case ItemDrop.ItemData.ItemType.Shoulder:
        //            break;
        //        case ItemDrop.ItemData.ItemType.Utility:
        //            break;
        //        case ItemDrop.ItemData.ItemType.Tool:
        //            break;
        //        case ItemDrop.ItemData.ItemType.Attach_Atgeir:
        //            break;
        //        case ItemDrop.ItemData.ItemType.Fish:
        //            break;
        //        case ItemDrop.ItemData.ItemType.TwoHandedWeaponLeft:
        //            break;
        //        case ItemDrop.ItemData.ItemType.AmmoNonEquipable:
        //            break;
        //        default:
        //            break;
        //    }
        //}

        // sorting by locale without setting it to english for everyone is dangerous due to desyncs
        public static IComparable SortByGetter(ItemDrop.ItemData item)
        {
            switch (QuickStackStorePlugin.SortCriteria)
            {
                case QuickStackStorePlugin.SortCriteriaEnum.TranslatedName:
                    return Localization.instance.Localize(item.m_shared.m_name);

                case QuickStackStorePlugin.SortCriteriaEnum.Value:
                    return item.m_shared.m_value;

                case QuickStackStorePlugin.SortCriteriaEnum.Weight:
                    return item.m_shared.m_weight;

                case QuickStackStorePlugin.SortCriteriaEnum.InternalName:
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

            if (!QuickStackStorePlugin.SortInAscendingOrder)
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
                playerConfig = QuickStackStorePlugin.GetPlayerConfig(player.GetPlayerID());

                if (QuickStackStorePlugin.SortLeavesEmptyFavoritedSlotsEmpty)
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

            //var offset = new Vector2i(offset % inventory.GetWidth(), offset / inventory.GetWidth());
            bool ignoreFirstRow = player != null && !QuickStackStorePlugin.SortIncludesHotkeyBar;

            // simple ignore hotbar
            var offset = ignoreFirstRow ? new Vector2i(0, 1) : new Vector2i(0, 0);

            var toSort = inventory.GetAllItems().Where(item => ShouldSortItem(item, offset, playerConfig)).ToList();

            toSort.Sort((a, b) => SortCompare(a, b));

            if (QuickStackStorePlugin.SortMergesStacks)
            {
                var grouped = toSort.Where(itm => itm.m_stack < itm.m_shared.m_maxStackSize).GroupBy(itm => itm.m_shared.m_name).Where(itm => itm.Count() > 1).Select(grouping => grouping.ToList());
                //Plugin.instance.GetLogger().LogInfo($"There are {grouped.Count()} groups of stackable items");
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
                    //Plugin.instance.GetLogger().LogDebug($"Auto-Stacked in {numTimes} iterations");
                }
            }

            bool td = QuickStackStorePlugin.UseTopDownLogicForEverything;

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
            //Plugin.instance.GetLogger().LogDebug($"Sorting inventory took {sw.Elapsed}");

            // Clear the cache in case anyone is using something that loads plugins at run-time.
            CompatibilitySupport.cache.Clear();

            inventory.Changed();
        }
    }
}