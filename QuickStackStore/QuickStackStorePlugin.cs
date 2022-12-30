using BepInEx;
using HarmonyLib;
using System.Reflection;
using UnityEngine;

namespace QuickStackStore
{
    [BepInIncompatibility("com.maxsch.valheim.MultiUserChest")]
    [BepInIncompatibility("virtuacode.valheim.trashitems")]
    [BepInPlugin("goldenrevolver.quick_stack_store", NAME, VERSION)]
    public class QuickStackStorePlugin : BaseUnityPlugin
    {
        public const string NAME = "Quick Stack - Store - Sort - Trash - Restock";
        public const string VERSION = "1.2.1";

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