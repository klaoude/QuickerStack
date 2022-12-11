﻿using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickerStack
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
            if (QuickerStackPlugin.GetPlayerConfig(Player.m_localPlayer.GetPlayerID()).IsMarked(item.m_shared))
            {
                stringBuilder.Append("\n<color=magenta>Will not be quick stacked</color>");
            }
            __result = stringBuilder.ToString();
        }
    }
}
