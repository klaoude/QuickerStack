using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;

namespace QuickStackStore
{
    internal class ContainerFinder
    {
        public static List<Container> AllContainers = new List<Container>();

        public static List<Container> FindContainersInRange(Vector3 point, float range)
        {
            List<Container> list = new List<Container>();

            var sw = new System.Diagnostics.Stopwatch();
            sw.Start();

            foreach (Container container in AllContainers)
            {
                if (container != null && container.transform != null && Vector3.Distance(point, container.transform.position) < range)
                {
                    list.Add(container);
                }
            }

            sw.Stop();
            Debug.Log($"Found {list.Count} container/s out of {AllContainers.Count} in range in {sw.Elapsed} (global search)");

            return list;
        }
    }

    [HarmonyPatch(typeof(Container))]
    internal static class PatchContainer
    {
        [HarmonyPatch(nameof(Container.Awake))]
        [HarmonyPostfix]
        internal static void Awake(Container __instance, ZNetView ___m_nview)
        {
            ContainerFinder.AllContainers.Add(__instance);
        }

        [HarmonyPatch(nameof(Container.OnDestroyed))]
        [HarmonyPostfix]
        internal static void OnDestroyed(Container __instance)
        {
            Debug.Log($"Destoying container instance: {__instance.transform.position}");
            ContainerFinder.AllContainers.Remove(__instance);
        }
    }
}