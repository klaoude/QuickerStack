using BepInEx;
using BepInEx.Configuration;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using static QuickStackStore.CompatibilitySupport;
using static QuickStackStore.ControllerButtonHintHelper;
using static QuickStackStore.QSSConfig;

namespace QuickStackStore
{
    internal class ButtonRenderer
    {
        internal static bool hasOpenedInventoryOnce = false;
        internal static float origButtonLength = -1;
        internal static Vector3 origButtonPosition;

        internal static Text favoritingTogglingButtonText;

        internal static Button favoritingTogglingButton;
        internal static Button quickStackAreaButton;
        internal static Button sortInventoryButton;
        internal static Button restockAreaButton;

        internal static Button quickStackToContainerButton;
        internal static Button storeAllButton;
        internal static Button sortContainerButton;
        internal static Button restockFromContainerButton;

        private const float shrinkFactor = 0.9f;
        private const int vPadding = 8;
        private const int hAlign = 1;

        internal class MainButtonUpdate
        {
            internal static void UpdateInventoryGuiButtons(InventoryGui __instance)
            {
                if (!hasOpenedInventoryOnce)
                {
                    return;
                }

                if (__instance != InventoryGui.instance)
                {
                    return;
                }

                if (Player.m_localPlayer)
                {
                    // reset in case player forgot to turn it off
                    FavoritingMode.HasCurrentlyToggledFavoriting = false;

                    var conf = SortConfig.AutoSort.Value;

                    if (conf == AutoSortBehavior.SortPlayerInventoryOnOpen || conf == AutoSortBehavior.Both)
                    {
                        SortModule.SortPlayerInv(Player.m_localPlayer.m_inventory, UserConfig.GetPlayerConfig(Player.m_localPlayer.GetPlayerID()));
                    }

                    if (__instance.m_currentContainer && (conf == AutoSortBehavior.SortContainerOnOpen || conf == AutoSortBehavior.Both))
                    {
                        SortModule.SortContainer(__instance.m_currentContainer);
                    }
                }

                // if AUGA is installed, the 'take all' button might not exist
                if (!__instance.m_takeAllButton || !__instance.m_takeAllButton.TryGetComponent(out RectTransform takeAllButtonRect))
                {
                    return;
                }

                FixTakeAllButtonControllerHint(__instance);

                if (origButtonLength == -1)
                {
                    origButtonLength = takeAllButtonRect.sizeDelta.x;
                    origButtonPosition = takeAllButtonRect.localPosition;
                }

                // intentionally not checking "ShouldBlockChangesToTakeAllButton", because then everything would look stupid
                if (takeAllButtonRect.sizeDelta.x == origButtonLength)
                {
                    takeAllButtonRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, origButtonLength * shrinkFactor);
                }

                int extraContainerButtons = 0;

                if (GeneralConfig.OverrideButtonDisplay.Value != OverrideButtonDisplay.DisableAllNewButtons)
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

                Vector3 startOffset = takeAllButtonRect.localPosition;

                if (takeAllButtonRect.localPosition == origButtonPosition)
                {
                    if (extraContainerButtons <= 1)
                    {
                        // move the button to the left by half of its removed length
                        startOffset -= new Vector3((origButtonLength / 2) * (1 - shrinkFactor), 0);
                    }
                    else
                    {
                        startOffset = OppositePositionOfTakeAllButton();

                        bool goToTop = QuickStackConfig.DisplayQuickStackButtons.Value == ShowTwoButtons.OnlyInventoryButton;
                        startOffset += new Vector3(origButtonLength - hAlign, goToTop ? 0 : -vOffset);
                    }

                    if (!ShouldBlockChangesToTakeAllButton())
                    {
                        takeAllButtonRect.localPosition = startOffset;
                    }
                }

                if (GeneralConfig.OverrideButtonDisplay.Value == OverrideButtonDisplay.DisableAllNewButtons)
                {
                    return;
                }

                int miniButtons = 0;

                var weight = __instance.m_player.transform.Find("Weight");

                var displaySortButtons = SortConfig.DisplaySortButtons.Value;

                var randyStatus = HasRandyPlugin();

                if (displaySortButtons != ShowTwoButtons.OnlyContainerButton)
                {
                    // this one is deliberately unaffected by the randy equipment slot compatibility
                    bool shouldShow = __instance.m_currentContainer == null || displaySortButtons != ShowTwoButtons.BothButDependingOnContext;

                    if (sortInventoryButton == null)
                    {
                        sortInventoryButton = CreateMiniButton(__instance, nameof(sortInventoryButton), KeybindChecker.joySort);
                        sortInventoryButton.gameObject.SetActive(shouldShow);

                        if (shouldShow)
                        {
                            __instance.StartCoroutine(WaitAFrameToRepositionMiniButton(__instance, sortInventoryButton.transform, weight, ++miniButtons, randyStatus));
                        }

                        sortInventoryButton.onClick.RemoveAllListeners();
                        sortInventoryButton.onClick.AddListener(new UnityAction(() => SortModule.SortPlayerInv(Player.m_localPlayer.m_inventory, UserConfig.GetPlayerConfig(Player.m_localPlayer.GetPlayerID()))));
                    }
                    else
                    {
                        RepositionMiniButton(__instance, sortInventoryButton.transform, weight, ++miniButtons, randyStatus);
                    }
                }

                var allowAreaButtons = AllowAreaStackingRestocking();

                var displayRestockButtons = RestockConfig.DisplayRestockButtons.Value;

                if (allowAreaButtons && displayRestockButtons != ShowTwoButtons.OnlyContainerButton && RestockConfig.RestockFromNearbyRange.Value > 0)
                {
                    bool shouldntShow = __instance.m_currentContainer != null && (displayRestockButtons == ShowTwoButtons.BothButDependingOnContext || randyStatus == RandyStatus.EnabledWithQuickSlots);

                    if (restockAreaButton == null)
                    {
                        restockAreaButton = CreateMiniButton(__instance, nameof(restockAreaButton), KeybindChecker.joyRestock);
                        restockAreaButton.gameObject.SetActive(!shouldntShow);

                        if (!shouldntShow)
                        {
                            __instance.StartCoroutine(WaitAFrameToRepositionMiniButton(__instance, restockAreaButton.transform, weight, ++miniButtons, randyStatus));
                        }

                        restockAreaButton.onClick.RemoveAllListeners();
                        restockAreaButton.onClick.AddListener(new UnityAction(() => RestockModule.DoRestock(Player.m_localPlayer)));
                    }
                    else
                    {
                        restockAreaButton.gameObject.SetActive(!shouldntShow);

                        if (!shouldntShow)
                        {
                            RepositionMiniButton(__instance, restockAreaButton.transform, weight, ++miniButtons, randyStatus);
                        }
                    }
                }

                var displayQuickStackButtons = QuickStackConfig.DisplayQuickStackButtons.Value;

                if (allowAreaButtons && displayQuickStackButtons != ShowTwoButtons.OnlyContainerButton && QuickStackConfig.QuickStackToNearbyRange.Value > 0)
                {
                    bool shouldntShow = __instance.m_currentContainer != null && (displayQuickStackButtons == ShowTwoButtons.BothButDependingOnContext || randyStatus == RandyStatus.EnabledWithQuickSlots);

                    if (quickStackAreaButton == null)
                    {
                        quickStackAreaButton = CreateMiniButton(__instance, nameof(quickStackAreaButton), KeybindChecker.joyQuickStack);
                        quickStackAreaButton.gameObject.SetActive(!shouldntShow);

                        if (!shouldntShow)
                        {
                            __instance.StartCoroutine(WaitAFrameToRepositionMiniButton(__instance, quickStackAreaButton.transform, weight, ++miniButtons, randyStatus));
                        }

                        quickStackAreaButton.onClick.RemoveAllListeners();
                        quickStackAreaButton.onClick.AddListener(new UnityAction(() => QuickStackModule.DoQuickStack(Player.m_localPlayer)));
                    }
                    else
                    {
                        quickStackAreaButton.gameObject.SetActive(!shouldntShow);

                        if (!shouldntShow)
                        {
                            RepositionMiniButton(__instance, quickStackAreaButton.transform, weight, ++miniButtons, randyStatus);
                        }
                    }
                }

                var favConf = FavoriteConfig.DisplayFavoriteToggleButton.Value;

                if (favConf != FavoritingToggling.Disabled)
                {
                    int index;
                    Transform parent;

                    if (favConf == FavoritingToggling.EnabledBottomButton)
                    {
                        index = ++miniButtons;
                        parent = weight;
                    }
                    else
                    {
                        index = -1;
                        parent = __instance.m_player.transform.Find("Armor");
                    }

                    if (favoritingTogglingButton == null)
                    {
                        favoritingTogglingButton = CreateMiniButton(__instance, nameof(favoritingTogglingButton), KeybindChecker.joyFavoriteToggling);
                        favoritingTogglingButton.gameObject.SetActive(true);

                        favoritingTogglingButtonText = favoritingTogglingButton.transform.Find("Text").GetComponent<Text>();

                        // trigger text reset without changing value
                        FavoritingMode.RefreshDisplay();

                        __instance.StartCoroutine(WaitAFrameToRepositionMiniButton(__instance, favoritingTogglingButton.transform, parent, index, randyStatus));

                        favoritingTogglingButton.onClick.RemoveAllListeners();
                        favoritingTogglingButton.onClick.AddListener(new UnityAction(() => FavoritingMode.ToggleFavoriteToggling()));
                    }
                    else
                    {
                        RepositionMiniButton(__instance, favoritingTogglingButton.transform, parent, index, randyStatus);
                    }
                }

                int buttonsBelowTakeAll = 0;

                if (QuickStackConfig.DisplayQuickStackButtons.Value != ShowTwoButtons.OnlyInventoryButton)
                {
                    if (quickStackToContainerButton == null)
                    {
                        quickStackToContainerButton = CreateBigButton(__instance, nameof(quickStackToContainerButton), KeybindChecker.joyQuickStack);

                        if (randyStatus == RandyStatus.EnabledWithQuickSlots)
                        {
                            // jump to the opposite side of the default 'take all' button position, because we are out of space due to randy's quickslots
                            MoveButtonToIndex(ref quickStackToContainerButton, startOffset, -vOffset, 1, 1);
                        }
                        else if (ShouldBlockChangesToTakeAllButton())
                        {
                            MoveButtonToIndex(ref quickStackToContainerButton, startOffset, 0, extraContainerButtons, 1);
                        }
                        else
                        {
                            // revert the vertical movement from the 'take all' button
                            MoveButtonToIndex(ref quickStackToContainerButton, startOffset, -vOffset, extraContainerButtons, 1);
                        }

                        quickStackToContainerButton.onClick.AddListener(new UnityAction(() => QuickStackModule.DoQuickStack(Player.m_localPlayer, true)));
                    }

                    quickStackToContainerButton.gameObject.SetActive(__instance.m_currentContainer != null);
                }

                if (StoreTakeAllConfig.DisplayStoreAllButton.Value)
                {
                    if (storeAllButton == null)
                    {
                        storeAllButton = CreateBigButton(__instance, nameof(storeAllButton), KeybindChecker.joyStoreAll);
                        MoveButtonToIndex(ref storeAllButton, startOffset, vOffset, extraContainerButtons, ++buttonsBelowTakeAll);

                        storeAllButton.onClick.AddListener(new UnityAction(() => StoreTakeAllModule.StoreAllItemsInOrder(Player.m_localPlayer)));
                    }

                    storeAllButton.gameObject.SetActive(__instance.m_currentContainer != null);
                }

                if (RestockConfig.DisplayRestockButtons.Value != ShowTwoButtons.OnlyInventoryButton)
                {
                    if (restockFromContainerButton == null)
                    {
                        restockFromContainerButton = CreateBigButton(__instance, nameof(restockFromContainerButton), KeybindChecker.joyRestock);
                        MoveButtonToIndex(ref restockFromContainerButton, startOffset, vOffset, extraContainerButtons, ++buttonsBelowTakeAll);

                        restockFromContainerButton.onClick.AddListener(new UnityAction(() => RestockModule.DoRestock(Player.m_localPlayer, true)));
                    }

                    restockFromContainerButton.gameObject.SetActive(__instance.m_currentContainer != null);
                }

                if (SortConfig.DisplaySortButtons.Value != ShowTwoButtons.OnlyInventoryButton)
                {
                    if (sortContainerButton == null)
                    {
                        sortContainerButton = CreateBigButton(__instance, nameof(sortContainerButton), KeybindChecker.joySort);
                        MoveButtonToIndex(ref sortContainerButton, startOffset, vOffset, extraContainerButtons, ++buttonsBelowTakeAll);

                        sortContainerButton.onClick.AddListener(new UnityAction(() => SortModule.SortContainer(__instance.m_currentContainer)));
                    }

                    sortContainerButton.gameObject.SetActive(__instance.m_currentContainer != null);
                }

                if (!ShouldBlockChangesToTakeAllButton())
                {
                    takeAllButtonRect.gameObject.SetActive(__instance.m_currentContainer != null);
                }

                OnButtonTextTranslationSettingChanged(false);
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
                    return LocalizationConfig.GetRelevantTranslation(LocalizationConfig.SortByInternalNameLabel, nameof(LocalizationConfig.SortByInternalNameLabel));

                case SortCriteriaEnum.TranslatedName:
                    return LocalizationConfig.GetRelevantTranslation(LocalizationConfig.SortByTranslatedNameLabel, nameof(LocalizationConfig.SortByTranslatedNameLabel));

                case SortCriteriaEnum.Value:
                    return LocalizationConfig.GetRelevantTranslation(LocalizationConfig.SortByValueLabel, nameof(LocalizationConfig.SortByValueLabel));

                case SortCriteriaEnum.Weight:
                    return LocalizationConfig.GetRelevantTranslation(LocalizationConfig.SortByWeightLabel, nameof(LocalizationConfig.SortByWeightLabel));

                case SortCriteriaEnum.Type:
                    return LocalizationConfig.GetRelevantTranslation(LocalizationConfig.SortByTypeLabel, nameof(LocalizationConfig.SortByTypeLabel));

                default:
                    return "invalid";
            }
        }

        private static Button CreateBigButton(InventoryGui instance, string name, string joyHint)
        {
            var button = Object.Instantiate(instance.m_takeAllButton, instance.m_takeAllButton.transform.parent);
            button.name = name;

            instance.StartCoroutine(WaitAFrameToSetupControllerHint(button, joyHint));

            button.onClick.RemoveAllListeners();

            return button;
        }

        private const int miniButtonSize = 38;
        private const int miniButtonHPadding = 2;
        private const float normalMiniButtonVOffset = -56f;
        private const float lowerMiniButtonVOffset = -75f;

        private static Button CreateMiniButton(InventoryGui instance, string name, string joyHint)
        {
            var playerInventory = instance.m_player.transform;

            Button button = Object.Instantiate(instance.m_takeAllButton, playerInventory);
            button.name = name;

            instance.StartCoroutine(WaitAFrameToSetupControllerHint(button, joyHint));

            var rect = (RectTransform)button.transform;
            rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, miniButtonSize);
            rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, miniButtonSize);

            Text text = rect.Find("Text").GetComponent<Text>();
            text.resizeTextForBestFit = true;

            return button;
        }

        private static void RepositionMiniButton(InventoryGui instance, Transform button, Transform weight, int existingMiniButtons, RandyStatus randyStatus)
        {
            if (existingMiniButtons == -1)
            {
                button.localPosition = weight.localPosition + new Vector3(hAlign, 70f);
                return;
            }

            float distanceToMove = (miniButtonSize + miniButtonHPadding) * (existingMiniButtons - 1);

            if (randyStatus == RandyStatus.EnabledWithQuickSlots)
            {
                button.localPosition = weight.localPosition + new Vector3(hAlign, -distanceToMove + normalMiniButtonVOffset);
            }
            else
            {
                var shouldMoveLower = randyStatus == RandyStatus.EnabledWithoutQuickSlots || (HasPluginThatRequiresMiniButtonVMove() && instance.m_player.Find("EquipmentBkg") != null);
                float vPos = shouldMoveLower ? lowerMiniButtonVOffset : normalMiniButtonVOffset;

                button.localPosition = weight.localPosition + new Vector3(hAlign + distanceToMove, vPos);
            }
        }

        /// <summary>
        /// Wait for one frame, so the two Odin equipment slot mods can finish spawning the 'EquipmentBkg' object
        /// </summary>
        internal static IEnumerator WaitAFrameToRepositionMiniButton(InventoryGui instance, Transform button, Transform weight, int existingMiniButtons, RandyStatus randyStatus)
        {
            yield return null;
            RepositionMiniButton(instance, button, weight, existingMiniButtons, randyStatus);
        }

        /// <summary>
        /// Wait one frame for Destroy to finish, then reset UI
        /// </summary>
        internal static IEnumerator WaitAFrameToUpdateUIElements(InventoryGui instance, bool includeTrashButton)
        {
            yield return null;

            if (instance == null)
            {
                yield break;
            }

            MainButtonUpdate.UpdateInventoryGuiButtons(instance);

            if (includeTrashButton)
            {
                TrashModule.TrashItemsPatches.UpdateTrashCanUI(instance);
            }
        }

        internal static void OnButtonRelevantSettingChanged(BaseUnityPlugin plugin, bool includeTrashButton = false)
        {
            if (!hasOpenedInventoryOnce || !TrashModule.TrashItemsPatches.hasOpenedInventoryOnce)
            {
                return;
            }

            // reminder to never use ?. on monobehaviors

            if (InventoryGui.instance != null)
            {
                var takeAllButton = InventoryGui.instance.m_takeAllButton;

                if (takeAllButton != null)
                {
                    if (!ShouldBlockChangesToTakeAllButton())
                    {
                        takeAllButton.transform.localPosition = origButtonPosition;
                    }
                }
            }

            var buttons = new Button[] { storeAllButton, quickStackToContainerButton, sortContainerButton, restockFromContainerButton, sortInventoryButton, quickStackAreaButton, restockAreaButton, favoritingTogglingButton };

            foreach (var button in buttons)
            {
                if (button != null)
                {
                    Object.Destroy(button.gameObject);
                }
            }

            favoritingTogglingButtonText = null;

            if (includeTrashButton)
            {
                if (TrashModule.trashRoot != null)
                {
                    Object.Destroy(TrashModule.trashRoot.gameObject);
                }
            }

            plugin.StartCoroutine(WaitAFrameToUpdateUIElements(InventoryGui.instance, includeTrashButton));
        }

        private static void UpdateButtonTextTranslation(Button button, ConfigEntry<string> overrideConfig, string configName)
        {
            if (button != null)
            {
                var text = button.GetComponentInChildren<Text>();

                if (text != null)
                {
                    text.text = LocalizationConfig.GetRelevantTranslation(overrideConfig, configName);
                }
            }
        }

        internal static void OnButtonTextTranslationSettingChanged(bool includeTrashButton = true)
        {
            // reminder to never use ?. on monobehaviors

            if (InventoryGui.instance != null)
            {
                var takeAllButton = InventoryGui.instance.m_takeAllButton;

                if (takeAllButton != null)
                {
                    var text = takeAllButton.GetComponentInChildren<Text>();

                    if (text != null)
                    {
                        text.text = !LocalizationConfig.TakeAllLabel.Value.IsNullOrWhiteSpace() ? LocalizationConfig.TakeAllLabel.Value : Localization.instance.Translate(LocalizationConfig.takeAllKey);
                    }
                }
            }

            UpdateButtonTextTranslation(storeAllButton, LocalizationConfig.StoreAllLabel, nameof(LocalizationConfig.StoreAllLabel));
            UpdateButtonTextTranslation(quickStackToContainerButton, LocalizationConfig.QuickStackLabel, nameof(LocalizationConfig.QuickStackLabel));
            UpdateButtonTextTranslation(restockFromContainerButton, LocalizationConfig.RestockLabel, nameof(LocalizationConfig.RestockLabel));
            UpdateButtonTextTranslation(sortInventoryButton, LocalizationConfig.SortLabelCharacter, nameof(LocalizationConfig.SortLabelCharacter));
            UpdateButtonTextTranslation(quickStackAreaButton, LocalizationConfig.QuickStackLabelCharacter, nameof(LocalizationConfig.QuickStackLabelCharacter));
            UpdateButtonTextTranslation(restockAreaButton, LocalizationConfig.RestockLabelCharacter, nameof(LocalizationConfig.RestockLabelCharacter));

            if (sortContainerButton != null)
            {
                var text = sortContainerButton.GetComponentInChildren<Text>();

                if (text != null)
                {
                    var label = LocalizationConfig.GetRelevantTranslation(LocalizationConfig.SortLabel, nameof(LocalizationConfig.SortLabel));

                    if (SortConfig.DisplaySortCriteriaInLabel.Value)
                    {
                        label += $" ({SortCriteriaToShortHumanReadableString(SortConfig.SortCriteria.Value)})";
                    }

                    text.text = label;
                }
            }

            if (includeTrashButton && TrashModule.trashButton != null)
            {
                var text = TrashModule.trashButton.GetComponentInChildren<Text>();

                if (text != null)
                {
                    text.text = LocalizationConfig.GetRelevantTranslation(LocalizationConfig.TrashLabel, nameof(LocalizationConfig.TrashLabel));
                }
            }
        }
    }
}