using BepInEx;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace QuickStackStore
{
    public static class CompatibilitySupport
    {
        private static MethodInfo IsComfyArmorSlot;

        public static Dictionary<string, bool> cache = null;

        public const string aeden = "aedenthorn.ExtendedPlayerInventory";
        public const string comfy = "com.bruce.valheim.comfyquickslots";
        public const string odinPlus = "com.odinplusqol.mod";
        public const string odinExInv = "odinplusqol.OdinsExtendedInventory";
        public const string randy = "randyknapp.mods.equipmentandquickslots";

        public static string[] supportedPlugins = new string[]
        {
            aeden,
            comfy,
            odinPlus,
            odinExInv,
            randy,
        };

        public static void RegenerateCache()
        {
            cache = new Dictionary<string, bool>();

            var plugins = UnityEngine.Object.FindObjectsOfType<BaseUnityPlugin>();

            foreach (var guid in supportedPlugins)
            {
                cache[guid] = plugins.Any(plugin => plugin.Info.Metadata.GUID == guid);
            }
        }

        public static bool HasPlugin(string guid)
        {
            if (cache == null)
            {
                RegenerateCache();
            }

            if (!cache.ContainsKey(guid))
            {
                var plugins = UnityEngine.Object.FindObjectsOfType<BaseUnityPlugin>();
                cache[guid] = plugins.Any(plugin => plugin.Info.Metadata.GUID == guid);
            }

            return cache[guid];
        }

        public static bool HasPluginThatRequiresMiniButtonVMove()
        {
            return HasPlugin(aeden) || HasPlugin(odinExInv) || HasPlugin(odinPlus);
        }

        public static bool IsEquipOrQuickSlot(Vector2i itemPos, bool onlyCheckEquipSlot = false)
        {
            //if (HasPlugin("randyknapp.mods.equipmentandquickslots"))
            //{
            //    // randyknapps mod ignores everything this mod does anyway, so no need for specific compatibility
            //}

            //if (HasPlugin(aeden) || HasPlugin(odinExInv) || HasPlugin(odinPlus))
            //{
            //    // nothing to do here, these mods makes sure armor stays where it should, and we can't detect the quickslots. at least they are not affected by 'take all'
            //}

            if (HasPlugin("com.bruce.valheim.comfyquickslots"))
            {
                if (IsComfyArmorSlot == null)
                {
                    var ass = Assembly.Load("ComfyQuickSlots");

                    if (ass != null)
                    {
                        var type = ass.GetTypes().First(a => a.IsClass && a.Name == "ComfyQuickSlots");
                        var pubstatic = type.GetMethods(BindingFlags.Public | BindingFlags.Static);
                        IsComfyArmorSlot = pubstatic.First(t => t.Name == "IsArmorSlot" && t.GetParameters().Length == 1);
                    }
                }

                if ((bool)IsComfyArmorSlot?.Invoke(null, new object[] { itemPos }))
                {
                    return true;
                }

                if (!onlyCheckEquipSlot)
                {
                    // check for quickslot (could also be armor slot)
                    if ((bool)IsComfyArmorSlot?.Invoke(null, new object[] { new Vector2i(itemPos.x - 3, itemPos.y) }))
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}