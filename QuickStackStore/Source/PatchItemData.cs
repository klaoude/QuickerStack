using HarmonyLib;
using System;
using System.Text;
using UnityEngine;

namespace QuickStackStore
{
    [HarmonyPatch(typeof(ItemDrop.ItemData))]
    internal static class PatchItemData
    {
        [HarmonyPatch("GetTooltip", new Type[]
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

            if (QuickStackStorePlugin.GetPlayerConfig(Player.m_localPlayer.GetPlayerID()).IsItemMarked(item.m_shared))
            {
                var color = ColorUtility.ToHtmlStringRGB(QuickStackStorePlugin.FavoriteItemColor);

                stringBuilder.Append($"\n<color=#{color}>Will not be quick stacked</color>");
            }

            __result = stringBuilder.ToString();
        }
    }
}