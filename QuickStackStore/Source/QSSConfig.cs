using BepInEx.Configuration;
using UnityEngine;
using static QuickStackStore.QSSConfig.FavoriteConfig;
using static QuickStackStore.QSSConfig.GeneralConfig;
using static QuickStackStore.QSSConfig.LocalizationConfig;
using static QuickStackStore.QSSConfig.QuickStackConfig;
using static QuickStackStore.QSSConfig.RestockConfig;
using static QuickStackStore.QSSConfig.SortConfig;
using static QuickStackStore.QSSConfig.StoreTakeAllConfig;
using static QuickStackStore.QSSConfig.TrashConfig;

namespace QuickStackStore
{
    internal class QSSConfig
    {
        public static ConfigFile Config;

        internal class GeneralConfig
        {
            internal static ConfigEntry<bool> UseTopDownLogicForEverything;
            internal static ConfigEntry<bool> DisableAllNewButtons;
            internal static ConfigEntry<bool> DisableAllNewKeybinds;
            internal static ConfigEntry<bool> SuppressContainerSoundAndVisuals;
        }

        internal class FavoriteConfig
        {
            public static ConfigEntry<KeyCode> FavoriteModifierKey1;
            public static ConfigEntry<KeyCode> FavoriteModifierKey2;
            public static ConfigEntry<Color> BorderColorFavoritedItem; // valheim yellow/ orange-ish
            public static ConfigEntry<Color> BorderColorFavoritedSlot; // light-ish blue
            public static ConfigEntry<Color> BorderColorFavoritedItemOnFavoritedSlot; // dark-ish green
            public static ConfigEntry<Color> BorderColorTrashFlaggedItem; // dark-ish red
            public static ConfigEntry<Color> BorderColorTrashFlaggedItemOnFavoritedSlot;
        }

        internal class QuickStackConfig
        {
            public static ConfigEntry<ShowTwoButtons> DisplayQuickStackButtons;
            public static ConfigEntry<float> QuickStackToNearbyRange;
            public static ConfigEntry<KeyCode> QuickStackKey;
            public static ConfigEntry<bool> ShowQuickStackResultMessage;
            public static ConfigEntry<bool> QuickStackIncludesHotkeyBar;
            public static ConfigEntry<bool> QuickStackTrophiesIntoSameContainer;
            public static ConfigEntry<QuickStackBehavior> QuickStackHotkeyBehaviorWhenContainerOpen;
        }

        internal class RestockConfig
        {
            public static ConfigEntry<ShowTwoButtons> DisplayRestockButtons;
            public static ConfigEntry<RestockBehavior> RestockHotkeyBehaviorWhenContainerOpen;
            public static ConfigEntry<bool> RestockOnlyAmmoAndConsumables;
            public static ConfigEntry<KeyCode> RestockKey;
            public static ConfigEntry<bool> ShowRestockResultMessage;
            public static ConfigEntry<float> RestockFromNearbyRange;
        }

        internal class StoreTakeAllConfig
        {
            public static ConfigEntry<bool> StoreAllIncludesEquippedItems;
            public static ConfigEntry<bool> StoreAllIncludesHotkeyBar;
            public static ConfigEntry<bool> DisplayStoreAllButton;
            public static ConfigEntry<bool> ChestsUseImprovedTakeAllLogic;
        }

        internal class SortConfig
        {
            public static ConfigEntry<ShowTwoButtons> DisplaySortButtons;
            public static ConfigEntry<bool> SortIncludesHotkeyBar;
            public static ConfigEntry<bool> SortInAscendingOrder;
            public static ConfigEntry<bool> SortLeavesEmptyFavoritedSlotsEmpty;
            public static ConfigEntry<SortBehavior> SortHotkeyBehaviorWhenContainerOpen;
            public static ConfigEntry<bool> DisplaySortCriteriaInLabel;
            public static ConfigEntry<KeyCode> SortKey;
            public static ConfigEntry<bool> SortMergesStacks;
            public static ConfigEntry<SortCriteriaEnum> SortCriteria;
        }

        internal class TrashConfig
        {
            public static ConfigEntry<bool> EnableQuickTrash;
            public static ConfigEntry<ShowConfirmDialogOption> ShowConfirmDialogForNormalItem;
            public static ConfigEntry<bool> ShowConfirmDialogForQuickTrash;
            public static ConfigEntry<KeyCode> TrashHotkey;
            public static ConfigEntry<Color> TrashLabelColor;
            public static ConfigEntry<bool> DisplayTrashCanUI;
            public static ConfigEntry<bool> AlwaysConsiderTrophiesTrashFlagged;
        }

        internal class LocalizationConfig
        {
            public static ConfigEntry<string> RestockLabelCharacter;
            public static ConfigEntry<string> QuickStackLabelCharacter;
            public static ConfigEntry<string> SortLabelCharacter;

            public static ConfigEntry<string> QuickStackResultMessageNothing;
            public static ConfigEntry<string> QuickStackResultMessageNone;
            public static ConfigEntry<string> QuickStackResultMessageOne;
            public static ConfigEntry<string> QuickStackResultMessageMore;

            public static ConfigEntry<string> RestockResultMessageNothing;
            public static ConfigEntry<string> RestockResultMessageNone;
            public static ConfigEntry<string> RestockResultMessagePartial;
            public static ConfigEntry<string> RestockResultMessageFull;

            public static ConfigEntry<string> QuickStackLabel;
            public static ConfigEntry<string> StoreAllLabel;
            public static ConfigEntry<string> SortLabel;
            public static ConfigEntry<string> RestockLabel;
            public static ConfigEntry<string> TrashLabel;

            public static ConfigEntry<string> SortByInternalNameLabel;
            public static ConfigEntry<string> SortByTranslatedNameLabel;
            public static ConfigEntry<string> SortByValueLabel;
            public static ConfigEntry<string> SortByWeightLabel;

            public static ConfigEntry<string> QuickTrashConfirmation;
            public static ConfigEntry<string> CantTrashFavoritedItemWarning;
            public static ConfigEntry<string> CantTrashFlagFavoritedItemWarning;
            public static ConfigEntry<string> CantFavoriteTrashFlaggedItemWarning;

            public static ConfigEntry<string> FavoritedItemTooltip;
            public static ConfigEntry<string> TrashFlaggedItemTooltip;
        }

        internal static void LoadConfig(QuickStackStorePlugin plugin)
        {
            Config = plugin.Config;

            string sectionName;

            // TODO descriptions

            sectionName = "0 - General";

            UseTopDownLogicForEverything = Config.Bind(sectionName, nameof(UseTopDownLogicForEverything), false, "");
            DisableAllNewKeybinds = Config.Bind(sectionName, nameof(DisableAllNewKeybinds), false, "");
            DisableAllNewButtons = Config.Bind(sectionName, nameof(DisableAllNewButtons), false, "");
            DisableAllNewButtons.SettingChanged += (a, b) => ButtonRenderer.OnButtonRelevantSettingChanged(plugin);

            sectionName = "1 - Favoriting";

            FavoriteModifierKey1 = Config.Bind(sectionName, nameof(FavoriteModifierKey1), KeyCode.LeftAlt, $"While holding this, left clicking on slots or right clicking on items favorites them. Identical to {nameof(FavoriteModifierKey2)}.");
            FavoriteModifierKey2 = Config.Bind(sectionName, nameof(FavoriteModifierKey2), KeyCode.RightAlt, $"While holding this, left clicking on slots or right clicking on items favorites them. Identical to {nameof(FavoriteModifierKey1)}.");
            BorderColorFavoritedItem = Config.Bind(sectionName, nameof(BorderColorFavoritedItem), new Color(1f, 0.8482759f, 0f), "Color of the border for slots containing favorited items.");
            BorderColorFavoritedSlot = Config.Bind(sectionName, nameof(BorderColorFavoritedSlot), new Color(0f, 0.5f, 1f), "Color of the border for favorited slots.");
            BorderColorFavoritedItemOnFavoritedSlot = Config.Bind(sectionName, nameof(BorderColorFavoritedItemOnFavoritedSlot), new Color(0.5f, 0.67413795f, 0.5f), "If not disabled, color of the border of a favorited slots that also contains a favorited item.");
            BorderColorTrashFlaggedItem = Config.Bind(sectionName, nameof(BorderColorTrashFlaggedItem), new Color(0.5f, 0f, 0), "");
            BorderColorTrashFlaggedItemOnFavoritedSlot = Config.Bind(sectionName, nameof(BorderColorTrashFlaggedItemOnFavoritedSlot), Color.black, "If not disabled, color of the border of a favorited slots that also contains a favorited item.");

            sectionName = "2 - Quick Stacking and Restocking";

            SuppressContainerSoundAndVisuals = Config.Bind(sectionName, nameof(SuppressContainerSoundAndVisuals), true, "");

            sectionName = "2.1 - Quick Stacking";

            DisplayQuickStackButtons = Config.Bind(sectionName, nameof(DisplayQuickStackButtons), ShowTwoButtons.Both, "Whether to display the two quick stack buttons. Hotkeys work independently.");
            DisplayQuickStackButtons.SettingChanged += (a, b) => ButtonRenderer.OnButtonRelevantSettingChanged(plugin);

            QuickStackIncludesHotkeyBar = Config.Bind(sectionName, nameof(QuickStackIncludesHotkeyBar), true, "");
            ShowQuickStackResultMessage = Config.Bind(sectionName, nameof(ShowQuickStackResultMessage), true, "");
            QuickStackKey = Config.Bind(sectionName, nameof(QuickStackKey), KeyCode.P, "The hotkey to start quick stacking to nearby chests.");
            QuickStackToNearbyRange = Config.Bind(sectionName, nameof(QuickStackToNearbyRange), 10f, "How far from you is nearby, greater value = greater range.");
            QuickStackHotkeyBehaviorWhenContainerOpen = Config.Bind(sectionName, nameof(QuickStackHotkeyBehaviorWhenContainerOpen), QuickStackBehavior.QuickStackOnlyToCurrentContainer, "");
            QuickStackTrophiesIntoSameContainer = Config.Bind(sectionName, nameof(QuickStackTrophiesIntoSameContainer), false, "Whether to put all types of trophies in the container if any trophy is found in that container.");

            sectionName = "2.2 - Quick Restocking";

            DisplayRestockButtons = Config.Bind(sectionName, nameof(DisplayRestockButtons), ShowTwoButtons.Both, "");
            DisplayRestockButtons.SettingChanged += (a, b) => ButtonRenderer.OnButtonRelevantSettingChanged(plugin);

            ShowRestockResultMessage = Config.Bind(sectionName, nameof(ShowRestockResultMessage), true, "");
            RestockHotkeyBehaviorWhenContainerOpen = Config.Bind(sectionName, nameof(RestockHotkeyBehaviorWhenContainerOpen), RestockBehavior.RestockOnlyFromCurrentContainer, "");
            RestockFromNearbyRange = Config.Bind(sectionName, nameof(RestockFromNearbyRange), 10f, "");
            RestockOnlyAmmoAndConsumables = Config.Bind(sectionName, nameof(RestockOnlyAmmoAndConsumables), false, "");
            RestockKey = Config.Bind(sectionName, nameof(RestockKey), KeyCode.R, "");

            sectionName = "3 - Store and Take All";

            DisplayStoreAllButton = Config.Bind(sectionName, nameof(DisplayStoreAllButton), true, "Whether to add the 'store all' button and move the 'take all' button.");
            DisplayStoreAllButton.SettingChanged += (a, b) => ButtonRenderer.OnButtonRelevantSettingChanged(plugin);

            StoreAllIncludesEquippedItems = Config.Bind(sectionName, nameof(StoreAllIncludesEquippedItems), true, "Whether 'Store All' should exclude or also unequip and store equipped items.");
            StoreAllIncludesHotkeyBar = Config.Bind(sectionName, nameof(StoreAllIncludesHotkeyBar), true, "");
            ChestsUseImprovedTakeAllLogic = Config.Bind(sectionName, nameof(ChestsUseImprovedTakeAllLogic), true, "Whether to use the improved logic for 'Take All' for non tomb stones. Disable if needed for compatibility.");

            sectionName = "4 - Sorting";

            DisplaySortButtons = Config.Bind(sectionName, nameof(DisplaySortButtons), ShowTwoButtons.Both, "");
            DisplaySortButtons.SettingChanged += (a, b) => ButtonRenderer.OnButtonRelevantSettingChanged(plugin);

            DisplaySortCriteriaInLabel = Config.Bind(sectionName, nameof(DisplaySortCriteriaInLabel), false, "");
            DisplaySortCriteriaInLabel.SettingChanged += (a, b) => ButtonRenderer.OnButtonRelevantSettingChanged(plugin);

            SortIncludesHotkeyBar = Config.Bind(sectionName, nameof(SortIncludesHotkeyBar), false, "");
            SortHotkeyBehaviorWhenContainerOpen = Config.Bind(sectionName, nameof(SortHotkeyBehaviorWhenContainerOpen), SortBehavior.OnlySortContainer, "");
            SortCriteria = Config.Bind(sectionName, nameof(SortCriteria), SortCriteriaEnum.InternalName, "");
            SortLeavesEmptyFavoritedSlotsEmpty = Config.Bind(sectionName, nameof(SortLeavesEmptyFavoritedSlotsEmpty), false, "");
            SortInAscendingOrder = Config.Bind(sectionName, nameof(SortInAscendingOrder), true, "");
            SortMergesStacks = Config.Bind(sectionName, nameof(SortMergesStacks), true, "");
            SortKey = Config.Bind(sectionName, nameof(SortKey), KeyCode.O, "");

            sectionName = "5 - Trashing";

            DisplayTrashCanUI = Config.Bind(sectionName, nameof(DisplayTrashCanUI), true, "");
            DisplayTrashCanUI.SettingChanged += (a, b) => ButtonRenderer.OnButtonRelevantSettingChanged(plugin);

            EnableQuickTrash = Config.Bind(sectionName, nameof(EnableQuickTrash), true, "");
            ShowConfirmDialogForNormalItem = Config.Bind(sectionName, nameof(ShowConfirmDialogForNormalItem), ShowConfirmDialogOption.WhenNotTrashFlagged, "");
            ShowConfirmDialogForQuickTrash = Config.Bind(sectionName, nameof(ShowConfirmDialogForQuickTrash), true, "");
            AlwaysConsiderTrophiesTrashFlagged = Config.Bind(sectionName, nameof(AlwaysConsiderTrophiesTrashFlagged), false, "");
            TrashHotkey = Config.Bind(sectionName, nameof(TrashHotkey), KeyCode.Delete, "");
            TrashLabelColor = Config.Bind(sectionName, nameof(TrashLabelColor), new Color(1f, 0.8482759f, 0), "");

            sectionName = "9 - Localization";

            TrashLabel = Config.Bind(sectionName, nameof(TrashLabel), "Trash", "");
            TrashLabel.SettingChanged += (a, b) => ButtonRenderer.OnButtonRelevantSettingChanged(plugin);

            QuickStackLabel = Config.Bind(sectionName, nameof(QuickStackLabel), "Quick Stack", "");
            QuickStackLabel.SettingChanged += (a, b) => ButtonRenderer.OnButtonRelevantSettingChanged(plugin);

            StoreAllLabel = Config.Bind(sectionName, nameof(StoreAllLabel), "Store All", "");
            StoreAllLabel.SettingChanged += (a, b) => ButtonRenderer.OnButtonRelevantSettingChanged(plugin);

            RestockLabel = Config.Bind(sectionName, nameof(RestockLabel), "Restock", "");
            RestockLabel.SettingChanged += (a, b) => ButtonRenderer.OnButtonRelevantSettingChanged(plugin);

            SortLabel = Config.Bind(sectionName, nameof(SortLabel), "Sort", "");
            SortLabel.SettingChanged += (a, b) => ButtonRenderer.OnButtonRelevantSettingChanged(plugin);

            QuickStackLabelCharacter = Config.Bind(sectionName, nameof(QuickStackLabelCharacter), "Q", "");
            QuickStackLabelCharacter.SettingChanged += (a, b) => ButtonRenderer.OnButtonRelevantSettingChanged(plugin);

            SortLabelCharacter = Config.Bind(sectionName, nameof(SortLabelCharacter), "S", "");
            SortLabelCharacter.SettingChanged += (a, b) => ButtonRenderer.OnButtonRelevantSettingChanged(plugin);

            RestockLabelCharacter = Config.Bind(sectionName, nameof(RestockLabelCharacter), "R", "");
            RestockLabelCharacter.SettingChanged += (a, b) => ButtonRenderer.OnButtonRelevantSettingChanged(plugin);

            SortByInternalNameLabel = Config.Bind(sectionName, nameof(SortByInternalNameLabel), "i. name", "");
            SortByInternalNameLabel.SettingChanged += (a, b) => ButtonRenderer.OnButtonRelevantSettingChanged(plugin);

            SortByTranslatedNameLabel = Config.Bind(sectionName, nameof(SortByTranslatedNameLabel), "t. name", "");
            SortByTranslatedNameLabel.SettingChanged += (a, b) => ButtonRenderer.OnButtonRelevantSettingChanged(plugin);

            SortByValueLabel = Config.Bind(sectionName, nameof(SortByValueLabel), "value", "");
            SortByValueLabel.SettingChanged += (a, b) => ButtonRenderer.OnButtonRelevantSettingChanged(plugin);

            SortByWeightLabel = Config.Bind(sectionName, nameof(SortByWeightLabel), "weight", "");
            SortByWeightLabel.SettingChanged += (a, b) => ButtonRenderer.OnButtonRelevantSettingChanged(plugin);

            QuickStackResultMessageNothing = Config.Bind(sectionName, nameof(QuickStackResultMessageNothing), "Nothing to quick stack", "");
            QuickStackResultMessageNone = Config.Bind(sectionName, nameof(QuickStackResultMessageNone), "Stacked 0 items", "");
            QuickStackResultMessageOne = Config.Bind(sectionName, nameof(QuickStackResultMessageOne), "Stacked 1 item", "");
            QuickStackResultMessageMore = Config.Bind(sectionName, nameof(QuickStackResultMessageMore), "Stacked {0} items", "");

            RestockResultMessageNothing = Config.Bind(sectionName, nameof(RestockResultMessageNothing), "Nothing to restock", "");
            RestockResultMessageNone = Config.Bind(sectionName, nameof(RestockResultMessageNone), "Couldn't restock (0/{0})", "");
            RestockResultMessagePartial = Config.Bind(sectionName, nameof(RestockResultMessagePartial), "Partially restocked ({0}/{1})", "");
            RestockResultMessageFull = Config.Bind(sectionName, nameof(RestockResultMessageFull), "Fully restocked (total: {0})", "");

            QuickTrashConfirmation = Config.Bind(sectionName, nameof(QuickTrashConfirmation), "Quick trash?", "");
            CantTrashFavoritedItemWarning = Config.Bind(sectionName, nameof(CantTrashFavoritedItemWarning), "Can't trash favorited item!", "");
            CantTrashFlagFavoritedItemWarning = Config.Bind(sectionName, nameof(CantTrashFlagFavoritedItemWarning), "Can't trash flag a favorited item!", "");
            CantFavoriteTrashFlaggedItemWarning = Config.Bind(sectionName, nameof(CantFavoriteTrashFlaggedItemWarning), "Can't favorite a trash flagged item!", "");

            FavoritedItemTooltip = Config.Bind(sectionName, nameof(FavoritedItemTooltip), "Will not be quick stacked", "");
            TrashFlaggedItemTooltip = Config.Bind(sectionName, nameof(TrashFlaggedItemTooltip), "Can be quick trashed", "");
        }

        public enum ShowConfirmDialogOption
        {
            Never,
            WhenNotTrashFlagged,
            Always
        }

        public enum ShowTwoButtons
        {
            Both,
            OnlyInventoryButton,
            OnlyContainerButton
        }

        public enum QuickStackBehavior
        {
            QuickStackOnlyToCurrentContainer,
            QuickStackToBoth
        }

        public enum RestockBehavior
        {
            RestockOnlyFromCurrentContainer,
            RestockFromBoth
        }

        public enum SortBehavior
        {
            OnlySortContainer,
            SortBoth,
        }

        public enum SortCriteriaEnum
        {
            InternalName,
            TranslatedName,
            Value,
            Weight
        }
    }
}