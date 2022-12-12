using System.Collections.Generic;
using UnityEngine;

namespace QuickStackStore
{
    public static class Extensions
    {
        public static int GetIndexFromItemData(this ItemDrop.ItemData item, int width)
        {
            return item.m_gridPos.y * width + item.m_gridPos.x;
        }

        public static Color GetMixedColor(Color color1, Color color2)
        {
            float r = (color1.r + color2.r) / 2f;
            float g = (color1.g + color2.g) / 2f;
            float b = (color1.b + color2.b) / 2f;
            float a = (color1.a + color2.a) / 2f;

            return new Color(r, g, b, a);
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
    }
}