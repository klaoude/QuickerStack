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

        private static Button quickStackToContainerButton;
        private static Button depositAllButton;
        private static Button sortContainerButton;
        private static Button restockFromContainerButton;

        private const float shrinkFactor = 0.9f;
        private const int vPadding = 8;

        [HarmonyPatch(typeof(InventoryGui))]
        internal static class PatchGuiShow
        {
            // slightly lower priority so we get rendered on top of equipment slot mods
            // (lower priority -> later rendering -> you get rendered on top)
            [HarmonyPriority(Priority.LowerThanNormal)]
            [HarmonyPatch(nameof(InventoryGui.Show))]
            [HarmonyPostfix]
            public static void Show_Postfix(InventoryGui __instance)

            {
                if (__instance != InventoryGui.instance)
                {
                    return;
                }

                var takeAllButtonRect = __instance.m_takeAllButton.GetComponent<RectTransform>();

                if (origButtonLength == -1)
                {
                    origButtonLength = takeAllButtonRect.sizeDelta.x;
                    origButtonPosition = takeAllButtonRect.localPosition;

                    takeAllButtonRect.GetComponent<Button>().onClick.RemoveAllListeners();
                    takeAllButtonRect.GetComponent<Button>().onClick.AddListener(new UnityAction(() => StoreTakeAllModule.ContextSensitiveTakeAll(__instance)));
                }

                if (takeAllButtonRect.sizeDelta.x == origButtonLength)
                {
                    takeAllButtonRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, origButtonLength * shrinkFactor);
                }

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

                float vOffset = takeAllButtonRect.sizeDelta.y + vPadding;

                if (takeAllButtonRect.localPosition == origButtonPosition)
                {
                    if (extraContainerButtons <= 1)
                    {
                        // move the button to the left by half of its removed length
                        takeAllButtonRect.localPosition -= new Vector3((origButtonLength / 2) * (1 - shrinkFactor), 0);
                    }
                    else
                    {
                        takeAllButtonRect.localPosition = OppositePositionOfTakeAllButton();
                        bool goToTop = QuickStackConfig.DisplayQuickStackButtons.Value == ShowTwoButtons.OnlyInventoryButton;
                        takeAllButtonRect.localPosition += new Vector3(origButtonLength, goToTop ? 0 : -vOffset);
                    }
                }

                if (GeneralConfig.DisableAllNewButtons.Value)
                {
                    return;
                }

                int miniButtons = 0;

                var displaySortButtons = RestockConfig.DisplayRestockButtons.Value;

                if (SortConfig.DisplaySortButtons.Value != ShowTwoButtons.OnlyContainerButton)
                {
                    if (sortInventoryButton == null)
                    {
                        sortInventoryButton = CreateSmallButton(__instance, nameof(sortInventoryButton), LocalizationConfig.SortLabelCharacter.Value, ++miniButtons);

                        sortInventoryButton.onClick.RemoveAllListeners();
                        sortInventoryButton.onClick.AddListener(new UnityAction(() => SortModule.Sort(Player.m_localPlayer.GetInventory(), Player.m_localPlayer)));
                    }

                    // this one is deliberately unaffected by the randy equipment slot compatibility
                    sortInventoryButton.gameObject.SetActive(__instance.m_currentContainer == null || displaySortButtons != ShowTwoButtons.BothButDependingOnContext);
                }

                var displayRestockButtons = RestockConfig.DisplayRestockButtons.Value;

                if (displayRestockButtons != ShowTwoButtons.OnlyContainerButton)
                {
                    if (restockAreaButton == null)
                    {
                        restockAreaButton = CreateSmallButton(__instance, nameof(restockAreaButton), LocalizationConfig.RestockLabelCharacter.Value, ++miniButtons);

                        restockAreaButton.onClick.RemoveAllListeners();
                        restockAreaButton.onClick.AddListener(new UnityAction(() => QuickStackRestockModule.DoRestock(Player.m_localPlayer)));
                    }

                    bool shouldntShow = __instance.m_currentContainer != null && (displayRestockButtons == ShowTwoButtons.BothButDependingOnContext || CompatibilitySupport.HasPlugin(CompatibilitySupport.randy));

                    restockAreaButton.gameObject.SetActive(!shouldntShow);
                }

                var displayQuickStackButtons = QuickStackConfig.DisplayQuickStackButtons.Value;

                if (displayQuickStackButtons != ShowTwoButtons.OnlyContainerButton)
                {
                    if (quickStackAreaButton == null)
                    {
                        quickStackAreaButton = CreateSmallButton(__instance, nameof(quickStackAreaButton), LocalizationConfig.QuickStackLabelCharacter.Value, ++miniButtons);

                        quickStackAreaButton.onClick.RemoveAllListeners();
                        quickStackAreaButton.onClick.AddListener(new UnityAction(() => QuickStackRestockModule.DoQuickStack(Player.m_localPlayer)));
                    }

                    bool shouldntShow = __instance.m_currentContainer != null && (displayQuickStackButtons == ShowTwoButtons.BothButDependingOnContext || CompatibilitySupport.HasPlugin(CompatibilitySupport.randy));

                    quickStackAreaButton.gameObject.SetActive(!shouldntShow);
                }

                Vector2 startOffset = takeAllButtonRect.localPosition;
                int buttonsBelowTakeAll = 0;

                __instance.m_takeAllButton.gameObject.SetActive(__instance.m_currentContainer != null);

                if (QuickStackConfig.DisplayQuickStackButtons.Value != ShowTwoButtons.OnlyInventoryButton)
                {
                    if (quickStackToContainerButton == null)
                    {
                        quickStackToContainerButton = Object.Instantiate(__instance.m_takeAllButton, takeAllButtonRect.parent);

                        if (CompatibilitySupport.HasPlugin(CompatibilitySupport.randy))
                        {
                            // jump to the opposite side of the default 'take all' button position, because we are out of space
                            MoveButtonToIndex(ref quickStackToContainerButton, startOffset, -vOffset, 1, 1);
                        }
                        else
                        {
                            // revert the vertical movement from the 'take all' button
                            MoveButtonToIndex(ref quickStackToContainerButton, startOffset, -vOffset, extraContainerButtons, 1);
                        }

                        quickStackToContainerButton.onClick.RemoveAllListeners();
                        quickStackToContainerButton.onClick.AddListener(new UnityAction(() => QuickStackRestockModule.DoQuickStack(Player.m_localPlayer, true)));

                        quickStackToContainerButton.GetComponentInChildren<Text>().text = LocalizationConfig.QuickStackLabel.Value;
                    }

                    quickStackToContainerButton.gameObject.SetActive(__instance.m_currentContainer != null);
                }

                if (StoreTakeAllConfig.DisplayStoreAllButton.Value)
                {
                    if (depositAllButton == null)
                    {
                        depositAllButton = Object.Instantiate(__instance.m_takeAllButton, takeAllButtonRect.parent);
                        MoveButtonToIndex(ref depositAllButton, startOffset, vOffset, extraContainerButtons, ++buttonsBelowTakeAll);

                        depositAllButton.onClick.RemoveAllListeners();
                        depositAllButton.onClick.AddListener(new UnityAction(() => StoreTakeAllModule.StoreAllItemsInOrder(Player.m_localPlayer)));

                        depositAllButton.GetComponentInChildren<Text>().text = LocalizationConfig.StoreAllLabel.Value;
                    }

                    depositAllButton.gameObject.SetActive(__instance.m_currentContainer != null);
                }

                if (RestockConfig.DisplayRestockButtons.Value != ShowTwoButtons.OnlyInventoryButton)
                {
                    if (restockFromContainerButton == null)
                    {
                        restockFromContainerButton = Object.Instantiate(__instance.m_takeAllButton, takeAllButtonRect.parent);
                        MoveButtonToIndex(ref restockFromContainerButton, startOffset, vOffset, extraContainerButtons, ++buttonsBelowTakeAll);

                        restockFromContainerButton.onClick.RemoveAllListeners();
                        restockFromContainerButton.onClick.AddListener(new UnityAction(() => QuickStackRestockModule.DoRestock(Player.m_localPlayer, true)));

                        restockFromContainerButton.GetComponentInChildren<Text>().text = LocalizationConfig.RestockLabel.Value;
                    }

                    restockFromContainerButton.gameObject.SetActive(__instance.m_currentContainer != null);
                }

                if (SortConfig.DisplaySortButtons.Value != ShowTwoButtons.OnlyInventoryButton)
                {
                    if (sortContainerButton == null)
                    {
                        sortContainerButton = Object.Instantiate(__instance.m_takeAllButton, takeAllButtonRect.parent);
                        MoveButtonToIndex(ref sortContainerButton, startOffset, vOffset, extraContainerButtons, ++buttonsBelowTakeAll);

                        sortContainerButton.onClick.RemoveAllListeners();
                        sortContainerButton.onClick.AddListener(new UnityAction(() => SortModule.Sort(__instance.m_currentContainer.GetInventory())));

                        var label = LocalizationConfig.SortLabel.Value;

                        if (SortConfig.DisplaySortCriteriaInLabel.Value)
                        {
                            label += $" ({SortCriteriaToShortHumanReadableString(SortConfig.SortCriteria.Value)})";
                        }

                        sortContainerButton.GetComponentInChildren<Text>().text = label;
                    }

                    sortContainerButton.gameObject.SetActive(__instance.m_currentContainer != null);
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

                case SortCriteriaEnum.Type:
                    return LocalizationConfig.SortByTypeLabel.Value;

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
            obj.name = name;

            if (CompatibilitySupport.HasPlugin(CompatibilitySupport.randy))
            {
                obj.localPosition = weight.localPosition - new Vector3(-2, (size + hPadding) * (existingMiniButtons - 1) + 58f);
            }
            else
            {
                var shouldMoveLower = CompatibilitySupport.HasPluginThatRequiresMiniButtonHMove() && instance.m_player.Find("EquipmentBkg") != null;
                float vPos = shouldMoveLower ? -75f : -58f;

                obj.localPosition = weight.localPosition + new Vector3((size + hPadding) * (existingMiniButtons - 1) + 2, vPos);
            }

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

            if (quickStackToContainerButton != null)
            {
                Object.Destroy(quickStackToContainerButton.gameObject);
            }

            if (sortContainerButton != null)
            {
                Object.Destroy(sortContainerButton.gameObject);
            }

            if (sortInventoryButton != null)
            {
                Object.Destroy(sortInventoryButton.gameObject);
            }

            if (restockFromContainerButton != null)
            {
                Object.Destroy(restockFromContainerButton.gameObject);
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