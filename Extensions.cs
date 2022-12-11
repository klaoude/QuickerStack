using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace QuickerStack
{
    public static class Extensions
    {
        public unsafe static InventoryGrid GetPlayerGrid(this InventoryGui instance)
        {
            return Extensions._playerGrid.Invoke(instance);
        }

        public unsafe static GameObject GetDragGo(this InventoryGui instance)
        {
            return Extensions._m_dragGo.Invoke(instance);
        }

        public unsafe static ZNetView GetNView(this Container instance)
        {
            return Extensions._m_nview.Invoke(instance);
        }
        public unsafe static Container GetCurrentContainer(this InventoryGui instance)
        {
            return Extensions._m_currentContainer.Invoke(instance);
        }

        public unsafe static bool IsVisible(this TextInput instance)
        {
            return instance && Extensions._m_visibleFrame.Invoke(instance);
        }

        public unsafe static bool IsFocused(this Terminal instance)
        {
            return instance && Extensions._m_focused.Invoke(instance);
        }

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

        internal static readonly AccessTools.FieldRef<InventoryGui, InventoryGrid> _playerGrid = AccessTools.FieldRefAccess<InventoryGui, InventoryGrid>("m_playerGrid");
        internal static readonly AccessTools.FieldRef<InventoryGui, GameObject> _m_dragGo = AccessTools.FieldRefAccess<InventoryGui, GameObject>("m_dragGo");
        internal static readonly AccessTools.FieldRef<Container, ZNetView> _m_nview = AccessTools.FieldRefAccess<Container, ZNetView>("m_nview");

        internal static readonly AccessTools.FieldRef<TextInput, bool> _m_visibleFrame = AccessTools.FieldRefAccess<TextInput, bool>("m_visibleFrame");
        internal static readonly AccessTools.FieldRef<InventoryGui, Container> _m_currentContainer = AccessTools.FieldRefAccess<InventoryGui, Container>("m_currentContainer");
        internal static readonly AccessTools.FieldRef<Terminal, bool> _m_focused = AccessTools.FieldRefAccess<Terminal, bool>("m_focused");
    }
}
