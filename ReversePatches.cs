using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace QuickerStack
{
    [HarmonyPatch]
    public static class ReversePatches
    {
        [HarmonyReversePatch(0)]
        [HarmonyPatch(typeof(Container), "CheckAccess")]
        public static bool CheckAccess(this Container instance, long playerID)
        {
            throw new NotImplementedException();
        }
        [HarmonyReversePatch(0)]
        [HarmonyPatch(typeof(Inventory), "AddItem", new Type[]
        {
            typeof(ItemDrop.ItemData),
            typeof(int),
            typeof(int),
            typeof(int)
        })]
        public static bool AddItem(this Inventory instance, ItemDrop.ItemData item, int amount, int x, int y)
        {
            throw new NotImplementedException();
        }
        [HarmonyReversePatch(0)]
        [HarmonyPatch(typeof(Inventory), "FindFreeStackItem")]
        public static ItemDrop.ItemData FindFreeStackItem(this Inventory instance, string name, int quality)
        {
            throw new NotImplementedException();
        }
        [HarmonyReversePatch(0)]
        [HarmonyPatch(typeof(Inventory), "Changed")]
        public static void Changed(this Inventory instance)
        {
            throw new NotImplementedException();
        }
        [HarmonyReversePatch(0)]
        [HarmonyPatch(typeof(Container), "Save")]
        public static void Save(this Container instance)
        {
            throw new NotImplementedException();
        }
        [HarmonyReversePatch(0)]
        [HarmonyPatch(typeof(InventoryGrid), "GetButtonPos")]
        public static Vector2i GetButtonPos(this InventoryGrid instance, GameObject go)
        {
            throw new NotImplementedException();
        }
    }
}
