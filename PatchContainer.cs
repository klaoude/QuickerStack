using HarmonyLib;
using System;

namespace QuickerStack
{
    [HarmonyPatch(typeof(Container))]
    internal static class PatchContainer
    {
        // Token: 0x06000018 RID: 24 RVA: 0x0000285D File Offset: 0x00000A5D
        [HarmonyPatch("Awake")]
        [HarmonyPostfix]
        internal static void Awake(Container __instance, ZNetView ___m_nview)
        {
            QuickerStackPlugin.AllContainers.Add(__instance);
        }

        // Token: 0x06000019 RID: 25 RVA: 0x0000286A File Offset: 0x00000A6A
        [HarmonyPatch("OnDestroyed")]
        [HarmonyPostfix]
        internal static void OnDestroyed(Container __instance)
        {
            QuickerStackPlugin.AllContainers.Remove(__instance);
        }
    }
}
