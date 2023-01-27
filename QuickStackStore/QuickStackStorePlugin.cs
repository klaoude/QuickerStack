using BepInEx;
using HarmonyLib;
using System.Reflection;
using UnityEngine;

namespace QuickStackStore
{
    [BepInIncompatibility("virtuacode.valheim.trashitems")]
    [BepInDependency(CompatibilitySupport.multiUserChest, BepInDependency.DependencyFlags.SoftDependency)]
    [BepInPlugin(GUID, NAME, VERSION)]
    public class QuickStackStorePlugin : BaseUnityPlugin
    {
        public const string GUID = "goldenrevolver.quick_stack_store";
        public const string NAME = "Quick Stack - Store - Sort - Trash - Restock";
        public const string VERSION = "1.3.1";

        // TODO controller support
        protected void Awake()
        {
            if (CompatibilitySupport.HasOutdatedMUCPlugin())
            {
                Helper.LogO("This mod is not compatible with versions of Multi User Chest earlier than 0.4.0, aborting start", QSSConfig.DebugLevel.Warning);
                return;
            }

            var path = "QuickStackStore.Resources";

            BorderRenderer.border = Helper.LoadSprite($"{path}.border.png", new Rect(0, 0, 1024, 1024), new Vector2(512, 512));
            TrashModule.trashSprite = Helper.LoadSprite($"{path}.trash.png", new Rect(0, 0, 64, 64), new Vector2(32, 32));
            TrashModule.bgSprite = Helper.LoadSprite($"{path}.trashmask.png", new Rect(0, 0, 96, 112), new Vector2(48, 56));

            QSSConfig.LoadConfig(this);

            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());
        }
    }
}