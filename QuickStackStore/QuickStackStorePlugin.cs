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
        // TODO visual bug when opening inventory without container after opening container beforehand
        // TODO sort and equals checks should also check quality for non stackables
        // TODO maybe mark performance hit config values in yellow
        // TODO make sure quick stack to ships and carts works
        // TODO make sure hotkeys don't work while typing in chat
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