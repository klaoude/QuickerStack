﻿using HarmonyLib;
using System;
using UnityEngine;

namespace QuickerStack
{
    [HarmonyPatch(typeof(Container))]
    internal static class PatchContainer
    {
        [HarmonyPatch("Awake")]
        [HarmonyPostfix]
        internal static void Awake(Container __instance, ZNetView ___m_nview)
        {
            QuickerStackPlugin.AllContainers.Add(__instance);
        }
        [HarmonyPatch("OnDestroyed")]
        [HarmonyPostfix]
        internal static void OnDestroyed(Container __instance)
        {
            Debug.Log("Destoying container instance");
            Debug.Log(__instance.transform.position);
            QuickerStackPlugin.AllContainers.Remove(__instance);
        }
    }
}
