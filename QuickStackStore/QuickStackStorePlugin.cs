using BepInEx;
using HarmonyLib;
using System.Reflection;
using UnityEngine;

namespace QuickStackStore
{
    [BepInPlugin("goldenrevolver.quick_stack_store", NAME, VERSION)]
    public class QuickStackStorePlugin : BaseUnityPlugin
    {
        private const string NAME = "Quick Stack - Store - Sort - Trash - Restock";
        private const string VERSION = "0.9";

        // TODO chest in use can still get quick stacked to!!!! applied first fix, check if working

        // TODO config option to switch left and right click for favoriting?
        // TODO maybe sort and equals checks should also check quality for non stackables (the base game doesn't though)
        // TODO controller support
        protected void Awake()
        {
            var path = "QuickStackStore.Resources";
            BorderRenderer.border = Helper.LoadSprite($"{path}.border.png", new Rect(0, 0, 1024, 1024), new Vector2(512, 512));
            TrashModule.trashSprite = Helper.LoadSprite($"{path}.trash.png", new Rect(0, 0, 64, 64), new Vector2(32, 32));
            TrashModule.bgSprite = Helper.LoadSprite($"{path}.trashmask.png", new Rect(0, 0, 96, 112), new Vector2(48, 56));

            QSSConfig.LoadConfig(this);

            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());
        }
    }
}