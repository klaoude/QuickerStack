using HarmonyLib;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace QuickStackStore
{
    internal class TrashItems
    {
        public static bool ShowConfirmationDialog = true;
        public static KeyCode TrashHotkey = KeyCode.Delete;
        public static Color TrashLabelColor = new Color(1f, 0.8482759f, 0);
        public static string TrashLabel = "Trash";
        public static bool DisplayTrashCanUI = true;

        // TODO
        public static bool TrophiesIgnoreConfirmDialog = false;

        private static bool _clickedTrash = false;
        private static bool _confirmed = false;

        public static Sprite trashSprite;
        public static Sprite bgSprite;
        public static GameObject dialog;
        public static Transform trashRoot;
        public static TrashButton trashButton;

        [HarmonyPatch(typeof(InventoryGui))]
        internal static class TrashItemsPatches
        {
            [HarmonyPatch(nameof(InventoryGui.Show))]
            [HarmonyPostfix]
            public static void Show_Postfix(InventoryGui __instance)
            {
                if (__instance != InventoryGui.instance)
                {
                    return;
                }

                if (trashRoot != null || QuickStackStorePlugin.DisableAllNewButtons || !DisplayTrashCanUI)
                {
                    return;
                }

                Transform playerInventory = __instance.m_player.transform;

                trashRoot = Object.Instantiate(playerInventory.Find("Armor"), playerInventory);
                trashButton = trashRoot.gameObject.AddComponent<TrashButton>();
            }

            [HarmonyPatch(nameof(InventoryGui.Hide))]
            [HarmonyPostfix]
            public static void Postfix()
            {
                OnCancel();
            }

            [HarmonyPostfix]
            [HarmonyPatch(nameof(InventoryGui.UpdateItemDrag))]
            public static void UpdateItemDrag_Postfix(InventoryGui __instance, ItemDrop.ItemData ___m_dragItem, Inventory ___m_dragInventory, int ___m_dragAmount)
            {
                if (_clickedTrash && ___m_dragItem != null && ___m_dragInventory.ContainsItem(___m_dragItem))
                {
                    var playerConfig = QuickStackStorePlugin.GetPlayerConfig(Player.m_localPlayer.GetPlayerID());

                    if (playerConfig.IsItemNameOrSlotFavorited(___m_dragItem))
                    {
                        Player.m_localPlayer.Message(MessageHud.MessageType.Center, "Can't trash favorited item!", 0, null);
                        _clickedTrash = false;
                        return;
                    }

                    // TODO add other junk to the exception list
                    if (ShowConfirmationDialog && (TrophiesIgnoreConfirmDialog || ___m_dragItem.m_shared.m_itemType != ItemDrop.ItemData.ItemType.Trophie))
                    {
                        if (_confirmed)
                        {
                            _confirmed = false;
                        }
                        else
                        {
                            ShowConfirmDialog(___m_dragItem, ___m_dragAmount);
                            _clickedTrash = false;
                            return;
                        }
                    }

                    if (___m_dragAmount == ___m_dragItem.m_stack)
                    {
                        Player.m_localPlayer.RemoveEquipAction(___m_dragItem);
                        Player.m_localPlayer.UnequipItem(___m_dragItem, false);
                        ___m_dragInventory.RemoveItem(___m_dragItem);
                    }
                    else
                    {
                        ___m_dragInventory.RemoveItem(___m_dragItem, ___m_dragAmount);
                    }

                    __instance.SetupDragItem(null, null, 0);
                    __instance.UpdateCraftingPanel(false);
                }

                _clickedTrash = false;
            }
        }

        public class TrashButton : MonoBehaviour
        {
            private Canvas canvas;
            private RectTransform rectTransform;
            private GameObject buttonGo;

            protected void Awake()
            {
                if (InventoryGui.instance == null)
                {
                    return;
                }

                var playerInventory = InventoryGui.instance.m_player.transform;
                RectTransform rect = GetComponent<RectTransform>();
                rect.anchoredPosition -= new Vector2(0, 78);

                SetText(TrashLabel);
                SetColor(TrashLabelColor);

                // Replace armor with trash icon
                Transform tArmor = transform.Find("armor_icon");

                if (!tArmor)
                {
                    Debug.LogError("armor_icon not found!");
                }

                tArmor.GetComponent<Image>().sprite = trashSprite;

                transform.SetSiblingIndex(0);
                transform.gameObject.name = "Trash";

                buttonGo = new GameObject("ButtonCanvas");
                rectTransform = buttonGo.AddComponent<RectTransform>();
                rectTransform.transform.SetParent(transform.transform, true);
                rectTransform.anchoredPosition = Vector2.zero;
                rectTransform.sizeDelta = new Vector2(70, 74);
                canvas = buttonGo.AddComponent<Canvas>();
                buttonGo.AddComponent<GraphicRaycaster>();

                // Add trash ui button
                Button button = buttonGo.AddComponent<Button>();
                button.onClick.AddListener(new UnityAction(TrashItem));
                var image = buttonGo.AddComponent<Image>();
                image.color = new Color(0, 0, 0, 0);

                // Add border background
                Transform frames = playerInventory.Find("selected_frame");
                GameObject newFrame = Instantiate(frames.GetChild(0).gameObject, transform);
                newFrame.GetComponent<Image>().sprite = bgSprite;
                newFrame.transform.SetAsFirstSibling();
                newFrame.GetComponent<RectTransform>().sizeDelta = new Vector2(-8, 22);
                newFrame.GetComponent<RectTransform>().anchoredPosition = new Vector2(6, 7.5f);

                // Add inventory screen tab
                UIGroupHandler handler = gameObject.AddComponent<UIGroupHandler>();
                handler.m_groupPriority = 1;
                handler.m_enableWhenActiveAndGamepad = newFrame;
                InventoryGui.instance.m_uiGroups = InventoryGui.instance.m_uiGroups.AddToArray(handler);

                gameObject.AddComponent<TrashHandler>();
            }

            protected void Start()
            {
                StartCoroutine(DelayedOverrideSorting());
            }

            private IEnumerator DelayedOverrideSorting()
            {
                yield return null;

                if (canvas == null)
                {
                    yield break;
                }

                canvas.overrideSorting = true;
                canvas.sortingOrder = 1;
            }

            public void SetText(string text)
            {
                Transform tText = transform.Find("ac_text");

                if (!tText)
                {
                    Debug.LogError("ac_text not found!");
                    return;
                }

                tText.GetComponent<Text>().text = text;
            }

            public void SetColor(Color color)
            {
                Transform tText = transform.Find("ac_text");

                if (!tText)
                {
                    Debug.LogError("ac_text not found!");
                    return;
                }

                tText.GetComponent<Text>().color = color;
            }
        }

        public static void ShowConfirmDialog(ItemDrop.ItemData item, int itemAmount)
        {
            if (InventoryGui.instance == null || dialog != null)
            {
                return;
            }

            dialog = Object.Instantiate(InventoryGui.instance.m_splitPanel.gameObject, InventoryGui.instance.transform);

            var okButton = dialog.transform.Find("win_bkg/Button_ok").GetComponent<Button>();
            okButton.onClick.RemoveAllListeners();
            okButton.onClick.AddListener(new UnityAction(OnConfirm));
            okButton.GetComponentInChildren<Text>().text = "Trash";
            okButton.GetComponentInChildren<Text>().color = new Color(1, 0.2f, 0.1f);

            var cancelButton = dialog.transform.Find("win_bkg/Button_cancel").GetComponent<Button>();
            cancelButton.onClick.RemoveAllListeners();
            cancelButton.onClick.AddListener(new UnityAction(OnCancel));

            dialog.transform.Find("win_bkg/Slider").gameObject.SetActive(false);

            var text = dialog.transform.Find("win_bkg/Text").GetComponent<Text>();
            text.text = Localization.instance.Localize(item.m_shared.m_name);

            var icon = dialog.transform.Find("win_bkg/Icon_bkg/Icon").GetComponent<Image>();
            icon.sprite = item.GetIcon();

            var amount = dialog.transform.Find("win_bkg/amount").GetComponent<Text>();

            amount.text = itemAmount + "/" + item.m_shared.m_maxStackSize;

            dialog.gameObject.SetActive(true);
        }

        public static void OnConfirm()
        {
            _confirmed = true;

            if (dialog != null)
            {
                Object.Destroy(dialog);
                dialog = null;
            }

            TrashItem();
        }

        public static void OnCancel()
        {
            _confirmed = false;

            if (dialog != null)
            {
                Object.Destroy(dialog);
                dialog = null;
            }
        }

        public static void TrashItem()
        {
            Debug.Log("Trash Item clicked!");

            // TODO add pointing support
            if (InventoryGui.instance != null && InventoryGui.instance.m_dragGo != null)
            {
                _clickedTrash = true;

                InventoryGui.instance.UpdateItemDrag();
            }
        }
    }

    public class TrashHandler : MonoBehaviour
    {
        private UIGroupHandler handler;

        protected void Awake()
        {
            handler = this.GetComponent<UIGroupHandler>();
        }

        protected void Update()
        {
            if (ZInput.GetButtonDown("JoyButtonA") && handler.IsActive())
            {
                TrashItems.TrashItem();
                // Switch back to inventory tab
                InventoryGui.instance.SetActiveGroup(1);
            }
        }
    }
}