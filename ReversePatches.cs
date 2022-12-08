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
        // Token: 0x06000020 RID: 32 RVA: 0x00002ACF File Offset: 0x00000CCF
        [HarmonyReversePatch(0)]
        [HarmonyPatch(typeof(Container), "CheckAccess")]
        public static bool CheckAccess(this Container instance, long playerID)
        {
            throw new NotImplementedException();
        }

        // Token: 0x06000021 RID: 33 RVA: 0x00002AD6 File Offset: 0x00000CD6
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

        // Token: 0x06000022 RID: 34 RVA: 0x00002ADD File Offset: 0x00000CDD
        [HarmonyReversePatch(0)]
        [HarmonyPatch(typeof(Inventory), "FindFreeStackItem")]
        public static ItemDrop.ItemData FindFreeStackItem(this Inventory instance, string name, int quality)
        {
            throw new NotImplementedException();
        }

        // Token: 0x06000023 RID: 35 RVA: 0x00002AE4 File Offset: 0x00000CE4
        [HarmonyReversePatch(0)]
        [HarmonyPatch(typeof(Inventory), "Changed")]
        public static void Changed(this Inventory instance)
        {
            throw new NotImplementedException();
        }

        // Token: 0x06000024 RID: 36 RVA: 0x00002AEB File Offset: 0x00000CEB
        [HarmonyReversePatch(0)]
        [HarmonyPatch(typeof(Container), "Save")]
        public static void Save(this Container instance)
        {
            throw new NotImplementedException();
        }

        // Token: 0x06000025 RID: 37 RVA: 0x00002AF2 File Offset: 0x00000CF2
        [HarmonyReversePatch(0)]
        [HarmonyPatch(typeof(InventoryGrid), "GetButtonPos")]
        public static Vector2i GetButtonPos(this InventoryGrid instance, GameObject go)
        {
            throw new NotImplementedException();
        }
    }
}
