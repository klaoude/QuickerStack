using System;
using System.Collections.Generic;
using System.Linq;
using static QuickStackStore.QSSConfig;

namespace QuickStackStore
{
    public static class SortModule
    {
        /* Categories:
            0 = None, Customization, Misc
            1 = Trophie
            2 = Material
            3 = Fish
            4 = Consumable
            5 = AmmoNonEquipable
            6 = Ammo
            7 = Bow, Tool, OneHandedWeapon, TwoHandedWeapon, TwoHandedWeaponLeft, Attach_Atgeir, Torch
            8 = Shield
            9 = Utility
            10 = Helmet, Shoulder, Chest, Hands, Legs
        */

        // convert the type enum to custom categories
        public static int[] TypeToCategory = new int[] { 0, 2, 4, 7, 7, 8, 10, 10, 0, 6, 0, 10, 10, 1, 7, 7, 0, 10, 9, 7, 7, 3, 7, 5 };

        public static void DoSort(Player player)
        {
            Container container = InventoryGui.instance.m_currentContainer;

            if (container != null)
            {
                switch (SortConfig.SortHotkeyBehaviorWhenContainerOpen.Value)
                {
                    case SortBehavior.OnlySortContainer:
                        Sort(InventoryGui.instance.m_currentContainer.m_inventory);
                        break;

                    case SortBehavior.SortBoth:
                        Sort(InventoryGui.instance.m_currentContainer.m_inventory);
                        Sort(player.m_inventory, player);
                        break;
                }
            }
            else
            {
                Sort(player.m_inventory, player);
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
                    var typeNum = (int)item.m_shared.m_itemType;

                    if (typeNum < 0 || typeNum > 23)
                    {
                        return typeNum;
                    }
                    else
                    {
                        return TypeToCategory[(int)item.m_shared.m_itemType];
                    }

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

            UserConfig playerConfig = null;

            if (player != null)
            {
                playerConfig = UserConfig.GetPlayerConfig(player.GetPlayerID());
            }

            bool ignoreFirstRow = player != null && (GeneralConfig.OverrideHotkeyBarBehavior.Value == OverrideHotkeyBarBehavior.NeverAffectHotkeyBar || !SortConfig.SortIncludesHotkeyBar.Value);

            // simple ignore hotbar
            var offset = ignoreFirstRow ? new Vector2i(0, 1) : new Vector2i(0, 0);

            var allowedSlots = GetAllowedSlots(inventory, ignoreFirstRow, playerConfig);

            var toSort = inventory.m_inventory.Where(item => ShouldSortItem(item, offset, playerConfig)).ToList();

            if (SortConfig.SortMergesStacks.Value)
            {
                MergeStacks(toSort, inventory);
            }

            toSort.Sort((a, b) => SortCompare(a, b));

            for (int i = 0; i < toSort.Count; i++)
            {
                toSort[i].m_gridPos = allowedSlots[i];
            }

            //sw.Stop();
            //UnityEngine.Debug.LogWarning(sw.Elapsed);

            inventory.Changed();
        }

        private static List<Vector2i> GetAllowedSlots(Inventory inventory, bool ignoreFirstRow, UserConfig playerConfig = null)
        {
            var allowedSlots = new List<Vector2i>();

            int y;
            int max;

            if (GeneralConfig.UseTopDownLogicForEverything.Value)
            {
                y = ignoreFirstRow ? 1 : 0;
                max = inventory.GetHeight();
            }
            else
            {
                // this simulates iterating backwards, when you negate y
                y = -inventory.GetHeight() + 1;
                max = ignoreFirstRow ? 0 : 1;
            }

            for (; y < max; y++)
            {
                for (int x = 0; x < inventory.GetWidth(); x++)
                {
                    var pos = new Vector2i(x, GeneralConfig.UseTopDownLogicForEverything.Value ? y : -y);

                    if (playerConfig != null)
                    {
                        if (CompatibilitySupport.IsEquipOrQuickSlot(pos) || (SortConfig.SortLeavesEmptyFavoritedSlotsEmpty.Value && playerConfig.IsSlotFavorited(pos)))
                        {
                            continue;
                        }
                    }

                    allowedSlots.Add(pos);
                }
            }

            if (playerConfig != null)
            {
                foreach (var item in inventory.m_inventory)
                {
                    if (playerConfig.IsItemNameOrSlotFavorited(item))
                    {
                        allowedSlots.Remove(item.m_gridPos);
                    }
                }
            }

            return allowedSlots;
        }

        internal static void MergeStacks(List<ItemDrop.ItemData> toMerge, Inventory inventory)
        {
            var grouped = toMerge.Where(itm => itm.m_stack < itm.m_shared.m_maxStackSize).GroupBy(itm => itm.m_shared.m_name).Select(grouping => grouping.ToList()).ToList();

            foreach (var nonFullStacks in grouped)
            {
                if (nonFullStacks.Count <= 1)
                {
                    continue;
                }

                var totalItemCount = 0;

                foreach (var item in nonFullStacks)
                {
                    totalItemCount += item.m_stack;
                }

                var maxStack = nonFullStacks.First().m_shared.m_maxStackSize;

                var remainingItemCount = totalItemCount;

                foreach (var item in nonFullStacks)
                {
                    if (remainingItemCount <= 0)
                    {
                        item.m_stack = 0;
                        inventory.RemoveItem(item);
                        toMerge.Remove(item);
                    }
                    else
                    {
                        item.m_stack = Math.Min(maxStack, remainingItemCount);

                        remainingItemCount -= item.m_stack;
                    }
                }
            }
        }
    }
}