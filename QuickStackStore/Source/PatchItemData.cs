using HarmonyLib;
using System;
using System.Text;
using UnityEngine;

namespace QuickStackStore
{
    [HarmonyPatch(typeof(ItemDrop.ItemData))]
    internal static class PatchItemData
    {
        [HarmonyPatch(nameof(ItemDrop.ItemData.GetTooltip), new Type[]
        {
            typeof(ItemDrop.ItemData),
            typeof(int),
            typeof(bool)
        })]
        [HarmonyPostfix]
        public static void GetTooltip(ItemDrop.ItemData item, int qualityLevel, bool crafting, ref string __result)
        {
            if (crafting)
            {
                return;
            }

            StringBuilder stringBuilder = new StringBuilder(256);
            stringBuilder.Append(__result);

            var conf = QuickStackStorePlugin.GetPlayerConfig(Player.m_localPlayer.GetPlayerID());

            // TODO localization
            if (conf.IsItemNameFavorited(item.m_shared))
            {
                var color = ColorUtility.ToHtmlStringRGB(QuickStackStorePlugin.BorderColorFavoritedItem);

                stringBuilder.Append($"\n<color=#{color}>Will not be quick stacked</color>");
            }
            else if (conf.IsItemNameConsideredTrashFlagged(item.m_shared))
            {
                var color = ColorUtility.ToHtmlStringRGB(QuickStackStorePlugin.BorderColorTrashFlaggedItem);

                stringBuilder.Append($"\n<color=#{color}>Can be quick trashed</color>");
            }

            __result = stringBuilder.ToString();
        }
    }
}