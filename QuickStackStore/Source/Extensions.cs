using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace QuickStackStore
{
    public static class Extensions
    {
        public static int GetIndexFromItemData(this ItemDrop.ItemData item, int width)
        {
            return item.m_gridPos.y * width + item.m_gridPos.x;
        }

        internal static bool IsPressingFavoriteKey()
        {
            return Input.GetKey(QuickStackStorePlugin.FavoriteModifierKey1) || Input.GetKey(QuickStackStorePlugin.FavoriteModifierKey2);
        }

        public static Color GetMixedColor(Color color1, Color color2)
        {
            float r = (color1.r + color2.r) / 2f;
            float g = (color1.g + color2.g) / 2f;
            float b = (color1.b + color2.b) / 2f;
            float a = (color1.a + color2.a) / 2f;

            return new Color(r, g, b, a);
        }

        // taken from the 'Trash Items' mod, as allowed in their permission settings on nexus
        // https://www.nexusmods.com/valheim/mods/441
        // https://github.com/virtuaCode/valheim-mods/tree/main/TrashItems
        public static Sprite LoadSprite(string path, Rect size, Vector2 pivot, int units = 100)
        {
            Assembly asm = Assembly.GetExecutingAssembly();
            Stream img = asm.GetManifestResourceStream(path);

            Texture2D tex = new Texture2D((int)size.width, (int)size.height, TextureFormat.RGBA32, false, true);

            using (MemoryStream mStream = new MemoryStream())
            {
                img.CopyTo(mStream);
                tex.LoadImage(mStream.ToArray());
                tex.Apply();
                return Sprite.Create(tex, size, pivot, units);
            }
        }

        public static bool XAdd<T>(this List<T> instance, T item)
        {
            if (instance.Contains(item))
            {
                instance.Remove(item);
                return false;
            }
            else
            {
                instance.Add(item);
                return true;
            }
        }
    }
}