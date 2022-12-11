using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace QuickerStack
{
    public static class Extensions
    {
        // Token: 0x06000026 RID: 38 RVA: 0x00002AF9 File Offset: 0x00000CF9
        /*public static ref TR PrivateRef<T, TR>(this T instance, string fieldName)
        {
            return AccessTools.FieldRefAccess<T, TR>(fieldName).Invoke(instance);
        }*/

        // Token: 0x06000027 RID: 39 RVA: 0x00002B07 File Offset: 0x00000D07
        public unsafe static InventoryGrid GetPlayerGrid(this InventoryGui instance)
        {
            return Extensions._playerGrid.Invoke(instance);
        }

        // Token: 0x06000028 RID: 40 RVA: 0x00002B15 File Offset: 0x00000D15
        public unsafe static GameObject GetDragGo(this InventoryGui instance)
        {
            return Extensions._m_dragGo.Invoke(instance);
        }

        // Token: 0x06000029 RID: 41 RVA: 0x00002B23 File Offset: 0x00000D23
        public unsafe static ZNetView GetNView(this Container instance)
        {
            return Extensions._m_nview.Invoke(instance);
        }

        public unsafe static Container GetCurrentContainer(this InventoryGui instance)
        {
            return Extensions._m_currentContainer.Invoke(instance);
        }

        // Token: 0x0600002A RID: 42 RVA: 0x00002B31 File Offset: 0x00000D31
        public static bool XAdd<T>(this List<T> instance, T item)
        {
            if (instance.Contains(item))
            {
                instance.Remove(item);
                return false;
            }
            instance.Add(item);
            return true;
        }

        // Token: 0x0400001C RID: 28
        internal static readonly AccessTools.FieldRef<InventoryGui, InventoryGrid> _playerGrid = AccessTools.FieldRefAccess<InventoryGui, InventoryGrid>("m_playerGrid");

        // Token: 0x0400001D RID: 29
        internal static readonly AccessTools.FieldRef<InventoryGui, GameObject> _m_dragGo = AccessTools.FieldRefAccess<InventoryGui, GameObject>("m_dragGo");

        // Token: 0x0400001E RID: 30
        internal static readonly AccessTools.FieldRef<Container, ZNetView> _m_nview = AccessTools.FieldRefAccess<Container, ZNetView>("m_nview");

        internal static readonly AccessTools.FieldRef<InventoryGui, Container> _m_currentContainer = AccessTools.FieldRefAccess<InventoryGui, Container>("m_currentContainer");
    }
}
