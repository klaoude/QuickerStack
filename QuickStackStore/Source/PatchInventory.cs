using HarmonyLib;

namespace QuickStackStore
{
    [HarmonyPatch(typeof(Inventory))]
    internal class PatchInventory
    {
        [HarmonyPatch(nameof(Inventory.TopFirst))]
        [HarmonyPrefix]
        [HarmonyPriority(Priority.HigherThanNormal)]
        public static bool TopFirstPatch(ref bool __result)
        {
            if (QuickStackStorePlugin.UseTopDownLogicForEverything)
            {
                __result = true;
                return false;
            }
            else
            {
                return true;
            }
        }
    }
}