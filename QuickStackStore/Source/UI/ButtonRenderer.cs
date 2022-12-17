using HarmonyLib;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using static QuickStackStore.QSSConfig;

namespace QuickStackStore
{
    internal class ButtonRenderer
    {
        private static float origButtonLength = -1;
        private static Vector3 origButtonPosition;

        private static Button quickStackAreaButton;
        private static Button sortInventoryButton;
        private static Button restockAreaButton;

        private static Button quickStackToChestButton;
        private static Button depositAllButton;
        private static Button sortChestButton;
        private static Button restockChestButton;

        private const float shrinkFactor = 0.9f;
        private const int vPadding = 8;

        [HarmonyPatch(typeof(InventoryGui))]
        internal static class PatchGuiShow
        {
            [HarmonyPatch(nameof(InventoryGui.Show))]
            [HarmonyPostfix]
            public static void Show_Postfix(InventoryGui __instance)

            {
                if (__instance != InventoryGui.instance)
                {
                    return;
                }

                var buttonRect = __instance.m_takeAllButton.GetComponent<RectTransform>();

                if (origButtonLength == -1)
                {
                    origButtonLength = buttonRect.sizeDelta.x;
                    origButtonPosition = buttonRect.localPosition;

                    buttonRect.GetComponent<Button>().onClick.RemoveAllListeners();
                    buttonRect.GetComponent<Button>().onClick.AddListener(new UnityAction(() => StoreTakeAllModule.ContextSensitiveTakeAll(__instance)));
                }

                if (buttonRect.sizeDelta.x == origButtonLength)
                {
                    buttonRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, origButtonLength * shrinkFactor);
                }

                float vOffset = buttonRect.sizeDelta.y + vPadding;

                int extraContainerButtons = 0;

                if (!GeneralConfig.DisableAllNewButtons.Value)
                {
                    if (StoreTakeAllConfig.DisplayStoreAllButton.Value)
                    {
                        extraContainerButtons++;
                    }

                    if (QuickStackConfig.DisplayQuickStackButtons.Value != ShowTwoButtons.OnlyInventoryButton)
                    {
                        extraContainerButtons++;
                    }

                    if (RestockConfig.DisplayRestockButtons.Value != ShowTwoButtons.OnlyInventoryButton)
                    {
                        extraContainerButtons++;
                    }

                    if (SortConfig.DisplaySortButtons.Value != ShowTwoButtons.OnlyInventoryButton)
                    {
                        extraContainerButtons++;
                    }
                }

                if (buttonRect.localPosition == origButtonPosition)
                {
                    if (extraContainerButtons <= 1)
                    {
                        // move the button to the left by half of its removed length
                        buttonRect.localPosition -= new Vector3((origButtonLength / 2) * (1 - shrinkFactor), 0);
                    }
                    else
                    {
                        buttonRect.localPosition = OppositePositionOfTakeAllButton();
                        buttonRect.localPosition += new Vector3(origButtonLength, QuickStackConfig.DisplayQuickStackButtons.Value == ShowTwoButtons.OnlyInventoryButton ? 0 : -vOffset);
                    }
                }

                if (GeneralConfig.DisableAllNewButtons.Value)
                {
                    return;
                }

                int miniButtons = 0;

                if (QuickStackConfig.DisplayQuickStackButtons.Value != ShowTwoButtons.OnlyContainerButton)
                {
                    if (quickStackAreaButton == null)
                    {
                        quickStackAreaButton = CreateSmallButton(__instance, nameof(quickStackAreaButton), LocalizationConfig.QuickStackLabelCharacter.Value, ++miniButtons);

                        quickStackAreaButton.onClick.RemoveAllListeners();
                        quickStackAreaButton.onClick.AddListener(new UnityAction(() => QuickStackRestockModule.DoQuickStack(Player.m_localPlayer)));
                    }
                }

                if (RestockConfig.DisplayRestockButtons.Value != ShowTwoButtons.OnlyContainerButton)
                {
                    if (restockAreaButton == null)
                    {
                        restockAreaButton = CreateSmallButton(__instance, nameof(restockAreaButton), LocalizationConfig.RestockLabelCharacter.Value, ++miniButtons);

                        restockAreaButton.onClick.RemoveAllListeners();
                        restockAreaButton.onClick.AddListener(new UnityAction(() => QuickStackRestockModule.DoRestock(Player.m_localPlayer)));
                    }
                }

                if (SortConfig.DisplaySortButtons.Value != ShowTwoButtons.OnlyContainerButton)
                {
                    if (sortInventoryButton == null)
                    {
                        sortInventoryButton = CreateSmallButton(__instance, nameof(sortInventoryButton), LocalizationConfig.SortLabelCharacter.Value, ++miniButtons);

                        sortInventoryButton.onClick.RemoveAllListeners();
                        sortInventoryButton.onClick.AddListener(new UnityAction(() => SortModule.Sort(Player.m_localPlayer.GetInventory(), Player.m_localPlayer)));
                    }
                }

                Vector2 startOffset = buttonRect.localPosition;
                int buttonsBelowTakeAll = 0;

                if (QuickStackConfig.DisplayQuickStackButtons.Value != ShowTwoButtons.OnlyInventoryButton)
                {
                    if (quickStackToChestButton == null)
                    {
                        quickStackToChestButton = Object.Instantiate(__instance.m_takeAllButton, buttonRect.parent);

                        // revert the moment from the take all button
                        MoveButtonToIndex(ref quickStackToChestButton, startOffset, -vOffset, extraContainerButtons, 1);

                        quickStackToChestButton.onClick.RemoveAllListeners();
                        quickStackToChestButton.onClick.AddListener(new UnityAction(() => QuickStackRestockModule.DoQuickStack(Player.m_localPlayer, true)));

                        quickStackToChestButton.GetComponentInChildren<Text>().text = LocalizationConfig.QuickStackLabel.Value;
                    }
                }

                if (StoreTakeAllConfig.DisplayStoreAllButton.Value)
                {
                    if (depositAllButton == null)
                    {
                        depositAllButton = Object.Instantiate(__instance.m_takeAllButton, buttonRect.parent);
                        MoveButtonToIndex(ref depositAllButton, startOffset, vOffset, extraContainerButtons, ++buttonsBelowTakeAll);

                        depositAllButton.onClick.RemoveAllListeners();
                        depositAllButton.onClick.AddListener(new UnityAction(() => StoreTakeAllModule.StoreAllItemsInOrder(Player.m_localPlayer)));

                        depositAllButton.GetComponentInChildren<Text>().text = LocalizationConfig.StoreAllLabel.Value;
                    }
                }

                if (RestockConfig.DisplayRestockButtons.Value != ShowTwoButtons.OnlyInventoryButton)
                {
                    if (restockChestButton == null)
                    {
                        restockChestButton = Object.Instantiate(__instance.m_takeAllButton, buttonRect.parent);
                        MoveButtonToIndex(ref restockChestButton, startOffset, vOffset, extraContainerButtons, ++buttonsBelowTakeAll);

                        restockChestButton.onClick.RemoveAllListeners();
                        restockChestButton.onClick.AddListener(new UnityAction(() => QuickStackRestockModule.DoRestock(Player.m_localPlayer, true)));

                        restockChestButton.GetComponentInChildren<Text>().text = LocalizationConfig.RestockLabel.Value;
                    }
                }

                if (SortConfig.DisplaySortButtons.Value != ShowTwoButtons.OnlyInventoryButton)
                {
                    if (sortChestButton == null)
                    {
                        sortChestButton = Object.Instantiate(__instance.m_takeAllButton, buttonRect.parent);
                        MoveButtonToIndex(ref sortChestButton, startOffset, vOffset, extraContainerButtons, ++buttonsBelowTakeAll);

                        sortChestButton.onClick.RemoveAllListeners();
                        sortChestButton.onClick.AddListener(new UnityAction(() => SortModule.Sort(__instance.m_currentContainer.GetInventory())));

                        var label = LocalizationConfig.SortLabel.Value;

                        if (SortConfig.DisplaySortCriteriaInLabel.Value)
                        {
                            label += $" ({SortCriteriaToShortHumanReadableString(SortConfig.SortCriteria.Value)})";
                        }

                        sortChestButton.GetComponentInChildren<Text>().text = label;
                    }
                }
            }
        }

        private static void MoveButtonToIndex(ref Button buttonToMove, Vector3 startVector, float vOffset, int visibleExtraButtons, int buttonsBelowTakeAll)
        {
            if (visibleExtraButtons == 1)
            {
                buttonToMove.transform.localPosition = OppositePositionOfTakeAllButton();
            }
            else
            {
                buttonToMove.transform.localPosition = startVector;
                buttonToMove.transform.localPosition -= new Vector3(0, buttonsBelowTakeAll * vOffset);
            }
        }

        private static Vector3 OppositePositionOfTakeAllButton()
        {
            // move the button to the right by half of its removed length
            var scaleBased = (origButtonLength / 2) * (1 - shrinkFactor);
            return origButtonPosition + new Vector3(440f + scaleBased, 0f);
        }

        public static string SortCriteriaToShortHumanReadableString(SortCriteriaEnum sortingCriteria)
        {
            switch (sortingCriteria)
            {
                case SortCriteriaEnum.InternalName:
                    return LocalizationConfig.SortByInternalNameLabel.Value;

                case SortCriteriaEnum.TranslatedName:
                    return LocalizationConfig.SortByTranslatedNameLabel.Value;

                case SortCriteriaEnum.Value:
                    return LocalizationConfig.SortByValueLabel.Value;

                case SortCriteriaEnum.Weight:
                    return LocalizationConfig.SortByWeightLabel.Value;

                default:
                    return "invalid";
            }
        }

        private static Button CreateSmallButton(InventoryGui instance, string name, string buttonText, int existingMiniButtons)
        {
            var playerInventory = InventoryGui.instance.m_player.transform;

            var weight = playerInventory.Find("Weight");

            int hPadding = 2;
            int size = 38;

            Transform obj = Object.Instantiate(instance.m_takeAllButton.transform, weight.parent);
            obj.localPosition = weight.localPosition + new Vector3((size + hPadding) * (existingMiniButtons - 1) + 2, -56f);
            obj.name = name;

            obj.transform.SetAsFirstSibling();

            var rect = (RectTransform)obj.transform;
            rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, size);
            rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size);

            var rect2 = (RectTransform)rect.transform.Find("Text");
            rect2.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, size - 8);
            rect2.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size - 4);

            Text text = rect2.GetComponent<Text>();
            text.text = buttonText;
            text.resizeTextForBestFit = true;

            return rect.GetComponent<Button>();
        }

        internal static void OnButtonRelevantSettingChanged(QuickStackStorePlugin plugin)
        {
            // reminder to never use ?. on monobehaviors

            if (InventoryGui.instance != null && InventoryGui.instance.m_takeAllButton != null)
            {
                InventoryGui.instance.m_takeAllButton.transform.localPosition = origButtonPosition;
            }

            if (depositAllButton != null)
            {
                Object.Destroy(depositAllButton.gameObject);
            }

            if (quickStackAreaButton != null)
            {
                Object.Destroy(quickStackAreaButton.gameObject);
            }

            if (quickStackToChestButton != null)
            {
                Object.Destroy(quickStackToChestButton.gameObject);
            }

            if (sortChestButton != null)
            {
                Object.Destroy(sortChestButton.gameObject);
            }

            if (sortInventoryButton != null)
            {
                Object.Destroy(sortInventoryButton.gameObject);
            }

            if (restockChestButton != null)
            {
                Object.Destroy(restockChestButton.gameObject);
            }

            if (restockAreaButton != null)
            {
                Object.Destroy(restockAreaButton.gameObject);
            }

            if (TrashModule.trashRoot != null)
            {
                Object.Destroy(TrashModule.trashRoot.gameObject);
            }

            plugin.StartCoroutine(WaitAFrameToUpdateUIElements(InventoryGui.instance));
        }

        /// <summary>
        /// Wait one frame for Destroy to finish, then reset UI
        /// </summary>
        internal static IEnumerator WaitAFrameToUpdateUIElements(InventoryGui instance)
        {
            yield return null;
            PatchGuiShow.Show_Postfix(instance);
            TrashModule.TrashItemsPatches.Show_Postfix(instance);
        }
    }
}