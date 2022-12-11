using HarmonyLib;
using System;
using System.Collections.Generic;
using UnityEngine.Events;

namespace QuickStackStore
{
    internal class RepairPatch
    {
        [HarmonyPatch(typeof(InventoryGui), "Awake")]
        public static class RepairAllItems
        {
            private static void Postfix(ref InventoryGui __instance)
            {
                __instance.m_repairButton.onClick.AddListener(new UnityAction(RepairAll));
            }
        }

        public static void RepairAll()
        {
            var inv = InventoryGui.instance;

            while (inv.HaveRepairableItems())
            {
                inv.RepairOneItem();
            }
        }
    }

    internal class InventoryPatches
    {
        // TODO
        /// <summary>
        /// Makes all items fill inventories top to bottom instead of just tools and weapons
        /// </summary>
        //[HarmonyPatch(typeof(Inventory), "TopFirst")]
        //public static class Inventory_TopFirst_Patch
        //{
        //    public static bool Prefix(ref bool __result)
        //    {
        //        __result = true;
        //        return false;
        //    }
        //}

        /// <summary>
        /// When merging another inventory, try to merge items with existing stacks.
        /// </summary>
        [HarmonyPatch(typeof(Inventory), "MoveAll")]
        public static class Inventory_MoveAll_Patch
        {
            private static void Prefix(ref Inventory __instance, ref Inventory fromInventory)
            {
                List<ItemDrop.ItemData> list = new List<ItemDrop.ItemData>(fromInventory.GetAllItems());
                foreach (ItemDrop.ItemData otherItem in list)
                {
                    if (otherItem.m_shared.m_maxStackSize > 1)
                    {
                        foreach (ItemDrop.ItemData myItem in __instance.m_inventory)
                        {
                            if (myItem.m_shared.m_name == otherItem.m_shared.m_name && myItem.m_quality == otherItem.m_quality)
                            {
                                int itemsToMove = Math.Min(myItem.m_shared.m_maxStackSize - myItem.m_stack, otherItem.m_stack);
                                myItem.m_stack += itemsToMove;
                                if (otherItem.m_stack == itemsToMove)
                                {
                                    fromInventory.RemoveItem(otherItem);
                                    break;
                                }
                                else
                                {
                                    otherItem.m_stack -= itemsToMove;
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    internal class RestedPatches
    {
        [HarmonyPatch(typeof(Player), "Save")]
        public static class PatchPlayerSave
        {
            private static void Prefix(Player __instance)
            {
                if (ZNet.instance)
                {
                    var aliveAndNotRested = !__instance.IsDead() && (__instance.m_seman.GetStatusEffect("Rested") != null);
                    __instance.m_knownStations["comfortTweaksRested_" + ZNet.m_world.m_uid] = aliveAndNotRested ? (int)__instance.m_seman.GetStatusEffect("Rested").GetRemaningTime() : 0;
                }
            }
        }

        [HarmonyPatch(typeof(Player), "Load")]
        public static class PatchPlayerLoad
        {
            private static void Postfix(Player __instance)
            {
                if (ZNet.instance != null && __instance == Player.m_localPlayer && __instance.m_knownStations.TryGetValue("comfortTweaksRested_" + ZNet.m_world.m_uid, out var value) && value > 0)
                {
                    __instance.GetSEMan().AddStatusEffect("Rested", true).m_ttl = value;
                    __instance.m_knownStations["comfortTweaksRested_" + ZNet.m_world.m_uid] = 0;
                }
            }
        }
    }

    internal class BoatWagonPatches
    {
        [HarmonyPatch(typeof(Piece), nameof(Piece.Awake))]
        public static class Awake_Patch
        {
            private static void Postfix(Piece __instance)
            {
                Ship ship = __instance.GetComponentInChildren<Ship>();
                Vagon vagon = __instance.GetComponentInChildren<Vagon>();

                if (ship || vagon)
                {
                    __instance.m_canBeRemoved = true;
                }
            }
        }

        [HarmonyPatch(typeof(Piece), "CanBeRemoved")]
        public static class CanBeRemoved_Patch
        {
            private static bool Prefix(Piece __instance, ref bool __result)
            {
                __result = true;
                // TODO config setting for chest content left

                Container container = __instance.GetComponentInChildren<Container>();
                if (container != null)
                {
                    __result &= container.CanBeRemoved();
                }

                Ship ship = __instance.GetComponentInChildren<Ship>();
                if (ship != null)
                {
                    Container container2 = __instance.GetComponentInChildren<Container>();
                    if (container2 != null)
                    {
                        __result &= container2.GetInventory().NrOfItems() <= 0;
                    }

                    __result &= ship.CanBeRemoved();
                }

                Vagon vagon = __instance.GetComponentInChildren<Vagon>();
                if (vagon != null)
                {
                    __result &= !vagon.InUse() && vagon.m_container.GetInventory().NrOfItems() <= 0;
                }

                return false;
            }
        }
    }

    internal class TombStonePatches
    {
        public static ItemDrop.ItemData GetBelt(Inventory inv)
        {
            return inv.GetAllItems().Find((ItemDrop.ItemData x) => x.m_shared.m_name.Equals("$item_beltstrength"));
        }

        [HarmonyPatch(typeof(TombStone), "EasyFitInInventory")]
        public static class EasyFitInInventory_Patch
        {
            private static void Postfix(TombStone __instance, Player player, ref bool __result)
            {
                if (player == Player.m_localPlayer && !__result)
                {
                    Container m_container = Traverse.Create(__instance).Field<Container>("m_container").Value;

                    int freeSlots = player.GetInventory().GetEmptySlots() - m_container.GetInventory().NrOfItems();

                    if (freeSlots < 0)
                    {
                        foreach (ItemDrop.ItemData itemData in m_container.GetInventory().GetAllItems())
                        {
                            if (player.GetInventory().FindFreeStackSpace(itemData.m_shared.m_name) >= itemData.m_stack)
                            {
                                freeSlots++;
                            }
                        }

                        if (freeSlots < 0)
                        {
                            return;
                        }
                    }

                    float carryWeight = player.GetMaxCarryWeight();

                    bool containerHasBelt = GetBelt(m_container.GetInventory()) != null;
                    var playerBelt = GetBelt(player.GetInventory());

                    if (containerHasBelt || (playerBelt != null && !player.GetInventory().GetEquipedtems().Contains(playerBelt)))
                    {
                        // Recalculating max player carry weight including Megingjord
                        carryWeight += 150f;
                    }

                    if (player.GetInventory().GetTotalWeight() + m_container.GetInventory().GetTotalWeight() <= carryWeight)
                    {
                        __result = true;
                        return;
                    }
                }
            }
        }

        [HarmonyPatch(typeof(TombStone), "GiveBoost")]
        public static class GiveBoost_Patch
        {
            private static void Postfix(TombStone __instance)
            {
                if (__instance.m_lootStatusEffect is SE_Stats corpse_run)
                {
                    Player player = __instance.FindOwner();

                    if (player && GetBelt(player.GetInventory()) == null)
                    {
                        player.GetSEMan().RemoveStatusEffect(__instance.m_lootStatusEffect.name, true);

                        var badCorpseRun = __instance.m_lootStatusEffect.Clone() as SE_Stats;
                        badCorpseRun.name += "NoCarryWeight";
                        badCorpseRun.m_addMaxCarryWeight = 0;

                        player.GetSEMan().AddStatusEffect(badCorpseRun, true, 0, 0f);
                    }
                }
            }
        }
    }

    internal class WaterReequipPatch
    {
        /// <summary>
        /// Re-equip items when leaving the water.
        /// </summary>
        public static class UpdateEquipmentState
        {
            public static bool shouldReequipItemsAfterSwimming = false;
        }

        [HarmonyPatch(typeof(Humanoid), "UpdateEquipment")]
        public static class Humanoid_UpdateEquipment_Patch
        {
            private static bool Prefix(Humanoid __instance)
            {
                if (__instance.IsPlayer() && __instance.IsSwiming() && !__instance.IsOnGround())
                {
                    // The above is only enough to know we will eventually exit swimming, but we still don't know if the items were visible prior or not.
                    // We only want to re-show them if they were shown to begin with, so we need to check.
                    // This is also why this must be a prefix patch; in a postfix patch, the items are already hidden, and we don't know
                    // if they were hidden by UpdateEquipment or by the user far earlier.
                    if (__instance.m_leftItem != null || __instance.m_rightItem != null)
                    {
                        UpdateEquipmentState.shouldReequipItemsAfterSwimming = true;
                    }
                }
                else if (__instance.IsPlayer() && !__instance.IsSwiming() && __instance.IsOnGround() && UpdateEquipmentState.shouldReequipItemsAfterSwimming)
                {
                    __instance.ShowHandItems();
                    UpdateEquipmentState.shouldReequipItemsAfterSwimming = false;
                }

                return true;
            }
        }
    }
}