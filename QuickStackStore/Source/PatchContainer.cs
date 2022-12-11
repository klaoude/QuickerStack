using HarmonyLib;
using UnityEngine;

namespace QuickStackStore
{
    [HarmonyPatch(typeof(Container))]
    internal static class PatchContainer
    {
        [HarmonyPatch("Awake")]
        [HarmonyPostfix]
        internal static void Awake(Container __instance, ZNetView ___m_nview)
        {
            QuickStackStorePlugin.AllContainers.Add(__instance);
        }

        [HarmonyPatch("OnDestroyed")]
        [HarmonyPostfix]
        internal static void OnDestroyed(Container __instance)
        {
            Debug.Log($"Destoying container instance: {__instance.transform.position}");
            QuickStackStorePlugin.AllContainers.Remove(__instance);
        }
    }
}