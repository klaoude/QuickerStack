using HarmonyLib;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;
using static QuickStackStore.QSSConfig;

namespace QuickStackStore
{
    public static class Helper
    {
        internal static int CompareSlotOrder(Vector2i a, Vector2i b)
        {
            // Bottom left to top right
            var yPosCompare = -a.y.CompareTo(b.y);

            if (GeneralConfig.UseTopDownLogicForEverything.Value)
            {
                // Top left to bottom right
                yPosCompare *= -1;
            }

            return yPosCompare != 0 ? yPosCompare : a.x.CompareTo(b.x);
        }

        internal static bool IsPressingFavoriteKey()
        {
            return Input.GetKey(FavoriteConfig.FavoritingModifierKey1.Value) || Input.GetKey(FavoriteConfig.FavoritingModifierKey2.Value);
        }

        internal static Color GetMixedColor(Color color1, Color color2)
        {
            float r = (color1.r + color2.r) / 2f;
            float g = (color1.g + color2.g) / 2f;
            float b = (color1.b + color2.b) / 2f;
            float a = (color1.a + color2.a) / 2f;

            return new Color(r, g, b, a);
        }

        // originally from 'Trash Items' mod, as allowed in their permission settings on nexus
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

        // taken from https://github.com/aedenthorn/ValheimMods AedenthornUtils.IgnoreKeyPresses, public domain
        public static bool IgnoreKeyPresses()
        {
            // removed InventoryGui.IsVisible() because we specifically want that to be the case
            return ZNetScene.instance == null || Player.m_localPlayer == null || Minimap.IsOpen() || Console.IsVisible() || TextInput.IsVisible() || ZNet.instance.InPasswordDialog() || Chat.instance?.HasFocus() == true || StoreGui.IsVisible() || Menu.IsVisible() || TextViewer.instance?.IsVisible() == true;
        }
    }

    [HarmonyPatch(typeof(Player), nameof(Player.Update))]
    public static class HotkeyListener
    {
        public static void Postfix(Player __instance)
        {
            if (GeneralConfig.OverrideKeybindBehavior.Value == OverrideKeybindBehavior.DisableAllNewHotkeys)
            {
                return;
            }

            if (Player.m_localPlayer != __instance)
            {
                return;
            }

            if (!InventoryGui.IsVisible())
            {
                return;
            }

            if (Helper.IgnoreKeyPresses())
            {
                return;
            }

            if (Input.GetKeyDown(QuickStackConfig.QuickStackKey.Value))
            {
                QuickStackRestockModule.DoQuickStack(__instance);
            }
            else if (Input.GetKeyDown(RestockConfig.RestockKey.Value))
            {
                QuickStackRestockModule.DoRestock(__instance);
            }
            else if (Input.GetKeyDown(SortConfig.SortKey.Value))
            {
                SortModule.DoSort(__instance);
            }
            else if (Input.GetKeyDown(TrashConfig.TrashHotkey.Value))
            {
                TrashModule.TrashOrTrashFlagItem(true);
            }
            else if (Input.GetKeyDown(TrashConfig.QuickTrashHotkey.Value))
            {
                TrashModule.AttemptQuickTrash();
            }
        }
    }

    public static class Extensions
    {
        public static int GetIndexFromItemData(this ItemDrop.ItemData item, int width)
        {
            return item.m_gridPos.y * width + item.m_gridPos.x;
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