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
            internal static ConfigEntry<bool> DisableAllNewButtons;
            internal static ConfigEntry<bool> DisableAllNewKeybinds;
            internal static ConfigEntry<bool> NeverAffectHotkeyBar;
            internal static ConfigEntry<bool> SuppressContainerSoundAndVisuals;
            internal static ConfigEntry<bool> UseTopDownLogicForEverything;
        }

        internal class FavoriteConfig
        {
            public static ConfigEntry<Color> BorderColorFavoritedItem;
            public static ConfigEntry<Color> BorderColorFavoritedItemOnFavoritedSlot;
            public static ConfigEntry<Color> BorderColorFavoritedSlot;
            public static ConfigEntry<Color> BorderColorTrashFlaggedItem;
            public static ConfigEntry<Color> BorderColorTrashFlaggedItemOnFavoritedSlot;
            public static ConfigEntry<KeyCode> FavoritingModifierKey1;
            public static ConfigEntry<KeyCode> FavoritingModifierKey2;
        }

        internal class QuickStackConfig
        {
            public static ConfigEntry<ShowTwoButtons> DisplayQuickStackButtons;
            public static ConfigEntry<QuickStackBehavior> QuickStackHotkeyBehaviorWhenContainerOpen;
            public static ConfigEntry<bool> QuickStackIncludesHotkeyBar;
            public static ConfigEntry<KeyCode> QuickStackKey;
            public static ConfigEntry<float> QuickStackToNearbyRange;
            public static ConfigEntry<bool> QuickStackTrophiesIntoSameContainer;
            public static ConfigEntry<bool> ShowQuickStackResultMessage;
        }

        internal class RestockConfig
        {
            public static ConfigEntry<ShowTwoButtons> DisplayRestockButtons;
            public static ConfigEntry<float> RestockFromNearbyRange;
            public static ConfigEntry<RestockBehavior> RestockHotkeyBehaviorWhenContainerOpen;
            public static ConfigEntry<bool> RestockIncludesHotkeyBar;
            public static ConfigEntry<KeyCode> RestockKey;
            public static ConfigEntry<bool> RestockOnlyAmmoAndConsumables;
            public static ConfigEntry<bool> RestockOnlyFavoritedItems;
            public static ConfigEntry<bool> ShowRestockResultMessage;
        }

        internal class StoreTakeAllConfig
        {
            public static ConfigEntry<bool> ChestsUseImprovedTakeAllLogic;
            public static ConfigEntry<bool> DisplayStoreAllButton;
            public static ConfigEntry<bool> StoreAllIncludesEquippedItems;
            public static ConfigEntry<bool> StoreAllIncludesHotkeyBar;
        }

        internal class SortConfig
        {
            public static ConfigEntry<ShowTwoButtons> DisplaySortButtons;
            public static ConfigEntry<bool> DisplaySortCriteriaInLabel;
            public static ConfigEntry<SortCriteriaEnum> SortCriteria;
            public static ConfigEntry<SortBehavior> SortHotkeyBehaviorWhenContainerOpen;
            public static ConfigEntry<bool> SortInAscendingOrder;
            public static ConfigEntry<bool> SortIncludesHotkeyBar;
            public static ConfigEntry<KeyCode> SortKey;
            public static ConfigEntry<bool> SortLeavesEmptyFavoritedSlotsEmpty;
            public static ConfigEntry<bool> SortMergesStacks;
        }

        internal class TrashConfig
        {
            public static ConfigEntry<bool> AlwaysConsiderTrophiesTrashFlagged;
            public static ConfigEntry<bool> DisplayTrashCanUI;
            public static ConfigEntry<bool> EnableQuickTrash;
            public static ConfigEntry<KeyCode> QuickTrashHotkey;
            public static ConfigEntry<ShowConfirmDialogOption> ShowConfirmDialogForNormalItem;
            public static ConfigEntry<bool> ShowConfirmDialogForQuickTrash;
            public static ConfigEntry<KeyCode> TrashHotkey;
            public static ConfigEntry<bool> TrashingCanAffectHotkeyBar;
            public static ConfigEntry<Color> TrashLabelColor;
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
            public static ConfigEntry<string> SortByTypeLabel;

            public static ConfigEntry<string> TrashConfirmationOkayButton;
            public static ConfigEntry<string> QuickTrashConfirmation;
            public static ConfigEntry<string> CantTrashFavoritedItemWarning;
            public static ConfigEntry<string> CantTrashFlagFavoritedItemWarning;
            public static ConfigEntry<string> CantTrashHotkeyBarItemWarning;
            public static ConfigEntry<string> CantFavoriteTrashFlaggedItemWarning;

            public static ConfigEntry<string> FavoritedItemTooltip;
            public static ConfigEntry<string> TrashFlaggedItemTooltip;
        }

        internal static void LoadConfig(QuickStackStorePlugin plugin)
        {
            Config = plugin.Config;

            string sectionName;

            // keep the entries within a section in alphabetical order for the r2modman config manager

            string overrideButton = $"overridden by {DisableAllNewButtons}";
            string overrideHotkey = $"overridden by {DisableAllNewKeybinds}";
            string overrideHotkeyBar = $"overridden by {NeverAffectHotkeyBar}";
            string hotkey = "What to do when the hotkey is pressed while you have a container open.";
            string twoButtons = $"Which of the two buttons to display ({overrideButton}). The hotkey works independently.";
            string range = "How close the searched through containers have to be.";
            string favoriteFunction = "disallowing quick stacking, storing, sorting and trashing";

            sectionName = "0 - General";

            DisableAllNewButtons = Config.Bind(sectionName, nameof(DisableAllNewButtons), false, "Override to disable all new UI elements no matter the current individual setting of each of them.");
            DisableAllNewButtons.SettingChanged += (a, b) => ButtonRenderer.OnButtonRelevantSettingChanged(plugin);

            DisableAllNewKeybinds = Config.Bind(sectionName, nameof(DisableAllNewKeybinds), false, "Override to disable all new keybinds no matter the current individual setting of each of them.");
            NeverAffectHotkeyBar = Config.Bind(sectionName, nameof(NeverAffectHotkeyBar), true, "Override to never affect the hotkey bar with any feature no matter the individual setting of each of them. Recommended to turn off if you are actually using favoriting.");
            UseTopDownLogicForEverything = Config.Bind(sectionName, nameof(UseTopDownLogicForEverything), false, "Whether to always put items into the top first row (affects the entire game) rather than top or bottom first depending on the item type (base game uses top first only for weapons and tools, bottom first for the rest). Recommended to keep off.");

            sectionName = "1 - Favoriting";

            // valheim yellow/ orange-ish
            BorderColorFavoritedItem = Config.Bind(sectionName, nameof(BorderColorFavoritedItem), new Color(1f, 0.8482759f, 0f), "Color of the border for slots containing favorited items.");
            // dark-ish green
            BorderColorFavoritedItemOnFavoritedSlot = Config.Bind(sectionName, nameof(BorderColorFavoritedItemOnFavoritedSlot), new Color(0.5f, 0.67413795f, 0.5f), "Color of the border of a favorited slot that also contains a favorited item.");

            // light-ish blue
            BorderColorFavoritedSlot = Config.Bind(sectionName, nameof(BorderColorFavoritedSlot), new Color(0f, 0.5f, 1f), "Color of the border for favorited slots.");
            // dark-ish red
            BorderColorTrashFlaggedItem = Config.Bind(sectionName, nameof(BorderColorTrashFlaggedItem), new Color(0.5f, 0f, 0), "Color of the border for slots containing trash flagged items.");
            // black
            BorderColorTrashFlaggedItemOnFavoritedSlot = Config.Bind(sectionName, nameof(BorderColorTrashFlaggedItemOnFavoritedSlot), Color.black, "Color of the border of a favorited slot that also contains a trash flagged item.");

            string favoritingKey = $"While holding this, left clicking on items or right clicking on slots favorites them, {favoriteFunction}, or trash flags them if you are hovering an item on the trash can.";
            FavoritingModifierKey1 = Config.Bind(sectionName, nameof(FavoritingModifierKey1), KeyCode.LeftAlt, $"{favoritingKey} Identical to {nameof(FavoritingModifierKey2)}.");
            FavoritingModifierKey2 = Config.Bind(sectionName, nameof(FavoritingModifierKey2), KeyCode.RightAlt, $"{favoritingKey} Identical to {nameof(FavoritingModifierKey1)}.");

            sectionName = "2 - Quick Stacking and Restocking";

            SuppressContainerSoundAndVisuals = Config.Bind(sectionName, nameof(SuppressContainerSoundAndVisuals), true, "Whether when a feature checks multiple containers in an area, they actually play opening sounds and visuals. Disable if the suppression causes incompatibilities.");

            sectionName = "2.1 - Quick Stacking";

            DisplayQuickStackButtons = Config.Bind(sectionName, nameof(DisplayQuickStackButtons), ShowTwoButtons.Both, twoButtons);
            DisplayQuickStackButtons.SettingChanged += (a, b) => ButtonRenderer.OnButtonRelevantSettingChanged(plugin);

            QuickStackHotkeyBehaviorWhenContainerOpen = Config.Bind(sectionName, nameof(QuickStackHotkeyBehaviorWhenContainerOpen), QuickStackBehavior.QuickStackOnlyToCurrentContainer, hotkey);
            QuickStackIncludesHotkeyBar = Config.Bind(sectionName, nameof(QuickStackIncludesHotkeyBar), true, $"Whether to also quick stack items from the hotkey bar ({overrideHotkeyBar}).");
            QuickStackKey = Config.Bind(sectionName, nameof(QuickStackKey), KeyCode.P, $"The hotkey to start quick stacking to the current or nearby containers (depending on {QuickStackHotkeyBehaviorWhenContainerOpen}, {overrideHotkey}).");
            QuickStackToNearbyRange = Config.Bind(sectionName, nameof(QuickStackToNearbyRange), 10f, range);
            QuickStackTrophiesIntoSameContainer = Config.Bind(sectionName, nameof(QuickStackTrophiesIntoSameContainer), false, "Whether to put all types of trophies in the container if any trophy is found in that container.");

            ShowQuickStackResultMessage = Config.Bind(sectionName, nameof(ShowQuickStackResultMessage), true, "Whether to show the central screen report message after quick stacking.");

            sectionName = "2.2 - Quick Restocking";

            DisplayRestockButtons = Config.Bind(sectionName, nameof(DisplayRestockButtons), ShowTwoButtons.Both, twoButtons);
            DisplayRestockButtons.SettingChanged += (a, b) => ButtonRenderer.OnButtonRelevantSettingChanged(plugin);

            RestockFromNearbyRange = Config.Bind(sectionName, nameof(RestockFromNearbyRange), 10f, range);
            RestockHotkeyBehaviorWhenContainerOpen = Config.Bind(sectionName, nameof(RestockHotkeyBehaviorWhenContainerOpen), RestockBehavior.RestockOnlyFromCurrentContainer, hotkey);
            RestockIncludesHotkeyBar = Config.Bind(sectionName, nameof(RestockIncludesHotkeyBar), true, $"Whether to also try to restock items currently in the hotkey bar ({overrideHotkeyBar}).");
            RestockKey = Config.Bind(sectionName, nameof(RestockKey), KeyCode.R, $"The hotkey to start restocking from the current or nearby containers (depending on {RestockHotkeyBehaviorWhenContainerOpen}, {overrideHotkey}).");
            RestockOnlyAmmoAndConsumables = Config.Bind(sectionName, nameof(RestockOnlyAmmoAndConsumables), false, $"Whether restocking should only restock ammo and consumable or every stackable item (like materials). Also affected by {RestockOnlyFavoritedItems}.");
            RestockOnlyFavoritedItems = Config.Bind(sectionName, nameof(RestockOnlyFavoritedItems), true, $"Whether restocking should only restock favorited items or items on favorited slots or every stackable item. Also affected by {RestockOnlyAmmoAndConsumables}.");
            ShowRestockResultMessage = Config.Bind(sectionName, nameof(ShowRestockResultMessage), true, "Whether to show the central screen report message after restocking.");

            sectionName = "3 - Store and Take All";

            ChestsUseImprovedTakeAllLogic = Config.Bind(sectionName, nameof(ChestsUseImprovedTakeAllLogic), true, "Whether to use the improved logic for 'Take All' for non tomb stones. Disable if needed for compatibility.");

            DisplayStoreAllButton = Config.Bind(sectionName, nameof(DisplayStoreAllButton), true, $"Whether to display the 'Store All' button in containers {overrideButton}");
            DisplayStoreAllButton.SettingChanged += (a, b) => ButtonRenderer.OnButtonRelevantSettingChanged(plugin);

            StoreAllIncludesEquippedItems = Config.Bind(sectionName, nameof(StoreAllIncludesEquippedItems), true, "Whether to also unequip and store non favorited equipped items or exclude them.");
            StoreAllIncludesHotkeyBar = Config.Bind(sectionName, nameof(StoreAllIncludesHotkeyBar), true, $"Whether to also store all non favorited items from the hotkey bar ({overrideHotkeyBar})");

            sectionName = "4 - Sorting";

            DisplaySortButtons = Config.Bind(sectionName, nameof(DisplaySortButtons), ShowTwoButtons.Both, twoButtons);
            DisplaySortButtons.SettingChanged += (a, b) => ButtonRenderer.OnButtonRelevantSettingChanged(plugin);

            DisplaySortCriteriaInLabel = Config.Bind(sectionName, nameof(DisplaySortCriteriaInLabel), false, "Whether to display the current sort criteria in the inventory sort button as a reminder. The author thinks the button is a bit too small for it to look good.");
            DisplaySortCriteriaInLabel.SettingChanged += (a, b) => ButtonRenderer.OnButtonRelevantSettingChanged(plugin);

            SortCriteria = Config.Bind(sectionName, nameof(SortCriteria), SortCriteriaEnum.InternalName, "The sort criteria the sort button uses. Ties are settled by internal name, quality and stack size.");
            SortHotkeyBehaviorWhenContainerOpen = Config.Bind(sectionName, nameof(SortHotkeyBehaviorWhenContainerOpen), SortBehavior.OnlySortContainer, hotkey);
            SortInAscendingOrder = Config.Bind(sectionName, nameof(SortInAscendingOrder), true, "Whether the current first sort criteria should be used in ascending or descending order.");
            SortIncludesHotkeyBar = Config.Bind(sectionName, nameof(SortIncludesHotkeyBar), true, $"Whether to also sort non favorited items from the hotkey bar ({overrideHotkeyBar}).");
            SortKey = Config.Bind(sectionName, nameof(SortKey), KeyCode.O, $"The hotkey to sort the inventory or the current container or both (depending on {SortHotkeyBehaviorWhenContainerOpen}, {overrideHotkey}).");
            SortLeavesEmptyFavoritedSlotsEmpty = Config.Bind(sectionName, nameof(SortLeavesEmptyFavoritedSlotsEmpty), false, "Whether sort treats empty favorited slots as occupied and leaves them empty, so you don't accidentally put items on them.");
            SortMergesStacks = Config.Bind(sectionName, nameof(SortMergesStacks), true, "Whether to merge stacks after sorting or keep them as separate non full stacks.");

            sectionName = "5 - Trashing";

            AlwaysConsiderTrophiesTrashFlagged = Config.Bind(sectionName, nameof(AlwaysConsiderTrophiesTrashFlagged), false, "Whether to always consider trophies as trash flagged, allowing for immediate trashing or to be affected by quick trashing.");

            DisplayTrashCanUI = Config.Bind(sectionName, nameof(DisplayTrashCanUI), true, $"Whether to display the trash can UI element ({overrideButton}). Hotkeys work independently.");
            DisplayTrashCanUI.SettingChanged += (a, b) => ButtonRenderer.OnButtonRelevantSettingChanged(plugin);

            EnableQuickTrash = Config.Bind(sectionName, nameof(EnableQuickTrash), true, "Whether quick trashing can be called with the hotkey or be clicking on the trash can while not holding anything.");
            QuickTrashHotkey = Config.Bind(sectionName, nameof(QuickTrashHotkey), KeyCode.None, $"The hotkey to perform a quick trash on the player inventory, deleting all trash flagged items ({overrideHotkey}).");
            ShowConfirmDialogForNormalItem = Config.Bind(sectionName, nameof(ShowConfirmDialogForNormalItem), ShowConfirmDialogOption.WhenNotTrashFlagged, "When to show a confirmation dialog while doing a non quick trash.");
            ShowConfirmDialogForQuickTrash = Config.Bind(sectionName, nameof(ShowConfirmDialogForQuickTrash), true, "Whether to show a confirmation dialog while doing a quick trash.");
            TrashHotkey = Config.Bind(sectionName, nameof(TrashHotkey), KeyCode.Delete, $"The hotkey to trash the currently held item ({overrideHotkey}).");
            TrashingCanAffectHotkeyBar = Config.Bind(sectionName, nameof(TrashingCanAffectHotkeyBar), true, $"Whether trashing and quick trashing can trash items that are currently in the hotkey bar ({overrideHotkeyBar}).");
            TrashLabelColor = Config.Bind(sectionName, nameof(TrashLabelColor), new Color(1f, 0.8482759f, 0), "The color of the text below the trash can in the player inventory.");

            sectionName = "9 - Localization";

            TrashLabel = Config.Bind(sectionName, nameof(TrashLabel), "Trash", string.Empty);
            TrashLabel.SettingChanged += (a, b) => ButtonRenderer.OnButtonRelevantSettingChanged(plugin);

            QuickStackLabel = Config.Bind(sectionName, nameof(QuickStackLabel), "Quick Stack", string.Empty);
            QuickStackLabel.SettingChanged += (a, b) => ButtonRenderer.OnButtonRelevantSettingChanged(plugin);

            StoreAllLabel = Config.Bind(sectionName, nameof(StoreAllLabel), "Store All", string.Empty);
            StoreAllLabel.SettingChanged += (a, b) => ButtonRenderer.OnButtonRelevantSettingChanged(plugin);

            RestockLabel = Config.Bind(sectionName, nameof(RestockLabel), "Restock", string.Empty);
            RestockLabel.SettingChanged += (a, b) => ButtonRenderer.OnButtonRelevantSettingChanged(plugin);

            SortLabel = Config.Bind(sectionName, nameof(SortLabel), "Sort", string.Empty);
            SortLabel.SettingChanged += (a, b) => ButtonRenderer.OnButtonRelevantSettingChanged(plugin);

            QuickStackLabelCharacter = Config.Bind(sectionName, nameof(QuickStackLabelCharacter), "Q", string.Empty);
            QuickStackLabelCharacter.SettingChanged += (a, b) => ButtonRenderer.OnButtonRelevantSettingChanged(plugin);

            SortLabelCharacter = Config.Bind(sectionName, nameof(SortLabelCharacter), "S", string.Empty);
            SortLabelCharacter.SettingChanged += (a, b) => ButtonRenderer.OnButtonRelevantSettingChanged(plugin);

            RestockLabelCharacter = Config.Bind(sectionName, nameof(RestockLabelCharacter), "R", string.Empty);
            RestockLabelCharacter.SettingChanged += (a, b) => ButtonRenderer.OnButtonRelevantSettingChanged(plugin);

            SortByInternalNameLabel = Config.Bind(sectionName, nameof(SortByInternalNameLabel), "i. name", string.Empty);
            SortByInternalNameLabel.SettingChanged += (a, b) => ButtonRenderer.OnButtonRelevantSettingChanged(plugin);

            SortByTranslatedNameLabel = Config.Bind(sectionName, nameof(SortByTranslatedNameLabel), "t. name", string.Empty);
            SortByTranslatedNameLabel.SettingChanged += (a, b) => ButtonRenderer.OnButtonRelevantSettingChanged(plugin);

            SortByValueLabel = Config.Bind(sectionName, nameof(SortByValueLabel), "value", string.Empty);
            SortByValueLabel.SettingChanged += (a, b) => ButtonRenderer.OnButtonRelevantSettingChanged(plugin);

            SortByWeightLabel = Config.Bind(sectionName, nameof(SortByWeightLabel), "weight", string.Empty);
            SortByWeightLabel.SettingChanged += (a, b) => ButtonRenderer.OnButtonRelevantSettingChanged(plugin);

            SortByTypeLabel = Config.Bind(sectionName, nameof(SortByTypeLabel), "type", string.Empty);
            SortByTypeLabel.SettingChanged += (a, b) => ButtonRenderer.OnButtonRelevantSettingChanged(plugin);

            QuickStackResultMessageNothing = Config.Bind(sectionName, nameof(QuickStackResultMessageNothing), "Nothing to quick stack", string.Empty);
            QuickStackResultMessageNone = Config.Bind(sectionName, nameof(QuickStackResultMessageNone), "Stacked 0 items", string.Empty);
            QuickStackResultMessageOne = Config.Bind(sectionName, nameof(QuickStackResultMessageOne), "Stacked 1 item", string.Empty);
            QuickStackResultMessageMore = Config.Bind(sectionName, nameof(QuickStackResultMessageMore), "Stacked {0} items", string.Empty);

            RestockResultMessageNothing = Config.Bind(sectionName, nameof(RestockResultMessageNothing), "Nothing to restock", string.Empty);
            RestockResultMessageNone = Config.Bind(sectionName, nameof(RestockResultMessageNone), "Couldn't restock (0/{0})", string.Empty);
            RestockResultMessagePartial = Config.Bind(sectionName, nameof(RestockResultMessagePartial), "Partially restocked ({0}/{1})", string.Empty);
            RestockResultMessageFull = Config.Bind(sectionName, nameof(RestockResultMessageFull), "Fully restocked (total: {0})", string.Empty);

            TrashConfirmationOkayButton = Config.Bind(sectionName, nameof(QuickTrashConfirmation), "Trash", string.Empty);
            QuickTrashConfirmation = Config.Bind(sectionName, nameof(QuickTrashConfirmation), "Quick trash?", string.Empty);
            CantTrashFavoritedItemWarning = Config.Bind(sectionName, nameof(CantTrashFavoritedItemWarning), "Can't trash favorited item!", string.Empty);
            CantTrashHotkeyBarItemWarning = Config.Bind(sectionName, nameof(CantTrashHotkeyBarItemWarning), "Settings disallow trashing of item in hotkey bar!", string.Empty);
            CantTrashFlagFavoritedItemWarning = Config.Bind(sectionName, nameof(CantTrashFlagFavoritedItemWarning), "Can't trash flag a favorited item!", string.Empty);
            CantFavoriteTrashFlaggedItemWarning = Config.Bind(sectionName, nameof(CantFavoriteTrashFlaggedItemWarning), "Can't favorite a trash flagged item!", string.Empty);

            FavoritedItemTooltip = Config.Bind(sectionName, nameof(FavoritedItemTooltip), "Will not be quick stacked, stored, sorted or trashed", string.Empty);
            TrashFlaggedItemTooltip = Config.Bind(sectionName, nameof(TrashFlaggedItemTooltip), "Can be quick trashed", string.Empty);
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
            Weight,
            Type
        }
    }
}