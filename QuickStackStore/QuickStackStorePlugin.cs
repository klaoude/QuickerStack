using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using static ItemDrop;

namespace QuickStackStore
{
    // QuickStackStoreSortTrashRestock (QSSSTR)
    [BepInPlugin("org.bepinex.plugins.valheim.quick_stack_store", NAME, VERSION)]
    public class QuickStackStorePlugin : BaseUnityPlugin
    {
        private const string NAME = "Quick Stack and Store";
        private const string VERSION = "0.9";

        // TODO chest in use can still get quick stacked to!!!! applied first fix, check if working

        // TODO config conversion and onsettingchanged listener for UI reset
        // TODO quick restock
        // TODO quick trash
        // TODO button repositioning including quick stack
        // TODO make sure quick stack to ships and carts works
        // TODO make sure hotkeys don't work while typing in chat
        // TODO controller support
        protected void Awake()
        {
            var path = "QuickStackStore.Resources";
            border = Extensions.LoadSprite($"{path}.border.png", new Rect(0, 0, 1024, 1024), new Vector2(512, 512));
            TrashItems.trashSprite = Extensions.LoadSprite($"{path}.trash.png", new Rect(0, 0, 64, 64), new Vector2(32, 32));
            TrashItems.bgSprite = Extensions.LoadSprite($"{path}.trashmask.png", new Rect(0, 0, 96, 112), new Vector2(48, 56));

            this.LoadConfig();
            Config.SettingChanged += OnSettingChanged;

            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());
        }

        private const float shrinkFactor = 0.9f;
        private const int hPadding = 4;
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
                    buttonRect.GetComponent<Button>().onClick.AddListener(new UnityAction(() => ContextSensitiveTakeAll(__instance)));
                }

                if (buttonRect.sizeDelta.x == origButtonLength)
                {
                    buttonRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, origButtonLength * shrinkFactor);
                }

                float vOffset = buttonRect.sizeDelta.y + vPadding;

                int visibleExtraButtons = 0;

                if (!DisableAllNewButtons)
                {
                    if (DisplayStoreAllButton)
                    {
                        visibleExtraButtons++;
                    }

                    if (DisplayQuickStackButtons)
                    {
                        visibleExtraButtons++;
                    }

                    if (DisplaySortButtons)
                    {
                        visibleExtraButtons++;
                    }
                }

                if (buttonRect.localPosition == origButtonPosition)
                {
                    if (visibleExtraButtons <= 1)
                    {
                        // move the button to the left by half of its removed length
                        buttonRect.localPosition -= new Vector3((origButtonLength / 2) * (1 - shrinkFactor), 0);
                    }
                    else
                    {
                        //TODO the horizontal offset (440f + origButtonLength) should be dependant on shrinkFactor, but I hit the perfect value for 0.9
                        buttonRect.localPosition += new Vector3(440f + origButtonLength, 0f);
                        buttonRect.localPosition -= new Vector3(0, buttonRect.sizeDelta.y);
                        buttonRect.localPosition += new Vector3(hPadding, -vPadding);
                    }
                }

                if (DisableAllNewButtons)
                {
                    return;
                }

                if (DisplayStoreAllButton)
                {
                    if (depositAllButton == null)
                    {
                        depositAllButton = Instantiate(__instance.m_takeAllButton, buttonRect.parent);

                        if (visibleExtraButtons == 1)
                        {
                            MoveButtonOppositeOfTakeAllButton(ref quickStackAreaButton);
                        }
                        else
                        {
                            depositAllButton.transform.localPosition = buttonRect.localPosition;
                            depositAllButton.transform.localPosition -= new Vector3(0, vOffset);
                        }

                        depositAllButton.onClick.RemoveAllListeners();
                        depositAllButton.onClick.AddListener(new UnityAction(() => StoreAllItemsInOrder(Player.m_localPlayer)));

                        depositAllButton.GetComponentInChildren<Text>().text = StoreAllLabel;
                    }
                }

                if (DisplayQuickStackButtons)
                {
                    if (quickStackAreaButton == null)
                    {
                        quickStackAreaButton = CreateSmallButton(__instance, nameof(quickStackAreaButton), QuickStackLabelCharacter);
                        quickStackAreaButton.onClick.RemoveAllListeners();
                        quickStackAreaButton.onClick.AddListener(new UnityAction(() => DoQuickStack(Player.m_localPlayer)));
                    }

                    if (quickStackToChestButton == null)
                    {
                        quickStackToChestButton = Instantiate(__instance.m_takeAllButton, buttonRect.parent);

                        if (visibleExtraButtons == 1)
                        {
                            MoveButtonOppositeOfTakeAllButton(ref quickStackAreaButton);
                        }
                        else
                        {
                            quickStackToChestButton.transform.localPosition = buttonRect.localPosition;
                            quickStackToChestButton.transform.localPosition += new Vector3(0, vOffset);
                        }

                        quickStackToChestButton.onClick.RemoveAllListeners();
                        quickStackToChestButton.onClick.AddListener(new UnityAction(() => DoQuickStack(Player.m_localPlayer, true)));

                        quickStackToChestButton.GetComponentInChildren<Text>().text = QuickStackLabel;
                    }
                }

                if (DisplaySortButtons)
                {
                    if (sortInventoryButton == null)
                    {
                        var hOffset = DisplayQuickStackButtons ? 42 : 0;
                        sortInventoryButton = CreateSmallButton(__instance, nameof(sortInventoryButton), SortLabelCharacter, hOffset);
                        sortInventoryButton.onClick.RemoveAllListeners();
                        sortInventoryButton.onClick.AddListener(new UnityAction(() => SortingUtilts.Sort(Player.m_localPlayer.GetInventory(), Player.m_localPlayer)));
                    }

                    if (sortChestButton == null)
                    {
                        sortChestButton = Instantiate(__instance.m_takeAllButton, buttonRect.parent);

                        if (visibleExtraButtons == 1)
                        {
                            MoveButtonOppositeOfTakeAllButton(ref sortChestButton);
                        }
                        else
                        {
                            sortChestButton.transform.localPosition = buttonRect.localPosition;
                            sortChestButton.transform.localPosition -= new Vector3(0, DisplayStoreAllButton ? 2 * vOffset : vOffset);
                        }

                        sortChestButton.onClick.RemoveAllListeners();
                        sortChestButton.onClick.AddListener(new UnityAction(() => SortingUtilts.Sort(__instance.m_currentContainer.GetInventory())));

                        var label = SortLabel;

                        if (DisplaySortCriteriaInLabel)
                        {
                            label += $" ({SortCriteriaToShortHumanReadableString(SortCriteria)})";
                        }

                        sortChestButton.GetComponentInChildren<Text>().text = label;
                    }
                }
            }
        }

        private static void MoveButtonOppositeOfTakeAllButton(ref Button button)
        {
            button.transform.localPosition = origButtonPosition + new Vector3(440f, 0f, -1f);
            // move the button to the right by half of its removed length
            button.transform.localPosition += new Vector3((origButtonLength / 2) * (1 - shrinkFactor), 0);
        }

        public static void DoSort(Player player)
        {
            Container container = InventoryGui.instance.m_currentContainer;

            if (container != null)
            {
                switch (SortHotkeyBehaviorWhenContainerOpen)
                {
                    case SortBehavior.OnlySortContainer:
                        SortingUtilts.Sort(InventoryGui.instance.m_currentContainer.GetInventory());
                        break;

                    case SortBehavior.SortBoth:
                        SortingUtilts.Sort(InventoryGui.instance.m_currentContainer.GetInventory());
                        SortingUtilts.Sort(player.GetInventory(), player);
                        break;
                }
            }
            else
            {
                SortingUtilts.Sort(player.GetInventory(), player);
            }
        }

        public static string SortCriteriaToShortHumanReadableString(SortCriteriaEnum sortingCriteria)
        {
            switch (sortingCriteria)
            {
                case SortCriteriaEnum.InternalName:
                    return SortByInternalNameLabel;

                case SortCriteriaEnum.TranslatedName:
                    return SortByTranslatedNameLabel;

                case SortCriteriaEnum.Value:
                    return SortByValueLabel;

                case SortCriteriaEnum.Weight:
                    return SortByWeightLabel;

                default:
                    return "invalid";
            }
        }

        public static void ContextSensitiveTakeAll(InventoryGui instance)
        {
            if (instance.m_currentContainer)
            {
                if (!ChestsUseImprovedTakeAllLogic || instance.m_currentContainer.GetComponent<TombStone>())
                {
                    instance.OnTakeAll();
                }
                else
                {
                    TakeAllItemsInOrder(Player.m_localPlayer);
                }
            }
        }

        private static Button CreateSmallButton(InventoryGui instance, string name, string buttonText, float hOffset = 0f)
        {
            var playerInventory = InventoryGui.instance.m_player.transform;

            var weight = playerInventory.Find("Weight");

            Transform obj = Instantiate(instance.m_takeAllButton.transform, weight.parent);
            obj.localPosition = weight.localPosition + new Vector3(hOffset, -56f, 0f);
            obj.name = name;

            obj.transform.SetAsFirstSibling();

            int size = 38;

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

        private T BindConfig<T>(string section, T param, string key, string description)
        {
            return Config.Bind(section, key, param, description).Value;
        }

        public static UserConfig GetPlayerConfig(long playerID)
        {
            if (playerConfigs.TryGetValue(playerID, out UserConfig userConfig))
            {
                return userConfig;
            }
            else
            {
                userConfig = new UserConfig(playerID);
                playerConfigs[playerID] = userConfig;

                return userConfig;
            }
        }

        private void LoadConfig()
        {
            string sectionName;

            sectionName = "0 - General";

            // TODO
            UseTopDownLogicForEverything = this.BindConfig(sectionName, UseTopDownLogicForEverything, nameof(UseTopDownLogicForEverything), "");
            DisableAllNewButtons = this.BindConfig(sectionName, DisableAllNewButtons, nameof(DisableAllNewButtons), "");
            DisableAllNewKeybinds = this.BindConfig(sectionName, DisableAllNewKeybinds, nameof(DisableAllNewKeybinds), "");

            sectionName = "1 - Quick Stacking";

            SuppressContainerSoundAndVisuals = this.BindConfig(sectionName, SuppressContainerSoundAndVisuals, nameof(SuppressContainerSoundAndVisuals), "");
            QuickStackIncludesHotkeyBar = this.BindConfig(sectionName, QuickStackIncludesHotkeyBar, nameof(QuickStackIncludesHotkeyBar), "");
            ShowQuickStackResultMessage = this.BindConfig(sectionName, ShowQuickStackResultMessage, nameof(ShowQuickStackResultMessage), "");
            QuickStackKey = this.BindConfig(sectionName, QuickStackKey, nameof(QuickStackKey), "The hotkey to immediately start quick stacking to nearby chests.");
            QuickStackToNearbyRange = this.BindConfig(sectionName, QuickStackToNearbyRange, nameof(QuickStackToNearbyRange), "How far from you is nearby, greater value = greater range.");
            QuickStackHotkeyBehaviorWhenContainerOpen = this.BindConfig(sectionName, QuickStackHotkeyBehaviorWhenContainerOpen, nameof(QuickStackHotkeyBehaviorWhenContainerOpen), "");
            QuickStackTrophiesIntoSameContainer = this.BindConfig(sectionName, QuickStackTrophiesIntoSameContainer, nameof(QuickStackTrophiesIntoSameContainer), "Whether to put all types of trophies in the container if any trophy is found in that container.");
            DisplayQuickStackButtons = this.BindConfig(sectionName, DisplayQuickStackButtons, nameof(DisplayQuickStackButtons), "Whether to display the two quick stack buttons. Hotkeys work independently.");

            sectionName = "2 - Store and Take All";

            DisplayStoreAllButton = this.BindConfig(sectionName, DisplayStoreAllButton, nameof(DisplayStoreAllButton), "Whether to add the 'store all' button and move the 'take all' button.");
            StoreAllIncludesEquippedItems = this.BindConfig(sectionName, StoreAllIncludesEquippedItems, nameof(StoreAllIncludesEquippedItems), "Whether 'Store All' should exclude or also unequip and store equipped items.");
            StoreAllIncludesHotkeyBar = this.BindConfig(sectionName, StoreAllIncludesHotkeyBar, nameof(StoreAllIncludesHotkeyBar), "");
            ChestsUseImprovedTakeAllLogic = this.BindConfig(sectionName, ChestsUseImprovedTakeAllLogic, nameof(ChestsUseImprovedTakeAllLogic), "Whether to use the improved logic for 'Take All' for non tomb stones. Disable if needed for compatibility.");

            sectionName = "3 - Favoriting";

            FavoriteModifierKey1 = this.BindConfig(sectionName, FavoriteModifierKey1, nameof(FavoriteModifierKey1), $"While holding this, left clicking on slots or right clicking on items favorites them. Identical to {nameof(FavoriteModifierKey2)}.");
            FavoriteModifierKey2 = this.BindConfig(sectionName, FavoriteModifierKey2, nameof(FavoriteModifierKey2), $"While holding this, left clicking on slots or right clicking on items favorites them. Identical to {nameof(FavoriteModifierKey1)}.");
            BorderColorFavoritedSlot = this.BindConfig(sectionName, BorderColorFavoritedSlot, nameof(BorderColorFavoritedSlot), "Color of the border for favorited slots.");
            BorderColorFavoritedItem = this.BindConfig(sectionName, BorderColorFavoritedItem, nameof(BorderColorFavoritedItem), "Color of the border for slots containing favorited items.");
            BorderColorFavoritedItemOnFavoritedSlot = this.BindConfig(sectionName, BorderColorFavoritedItemOnFavoritedSlot, nameof(BorderColorFavoritedItemOnFavoritedSlot), "If not disabled, color of the border of a favorited slots that also contains a favorited item.");
            BorderColorTrashFlaggedItem = this.BindConfig(sectionName, BorderColorTrashFlaggedItem, nameof(BorderColorTrashFlaggedItem), "");
            BorderColorTrashFlaggedItemOnFavoritedSlot = this.BindConfig(sectionName, BorderColorTrashFlaggedItemOnFavoritedSlot, nameof(BorderColorTrashFlaggedItemOnFavoritedSlot), "If not disabled, color of the border of a favorited slots that also contains a favorited item.");

            sectionName = "4 - Sorting";

            // TODO descriptions
            SortIncludesHotkeyBar = this.BindConfig(sectionName, SortIncludesHotkeyBar, nameof(SortIncludesHotkeyBar), "");
            SortHotkeyBehaviorWhenContainerOpen = this.BindConfig(sectionName, SortHotkeyBehaviorWhenContainerOpen, nameof(SortHotkeyBehaviorWhenContainerOpen), "");
            SortCriteria = this.BindConfig(sectionName, SortCriteria, nameof(SortCriteria), "");
            SortLeavesEmptyFavoritedSlotsEmpty = this.BindConfig(sectionName, SortLeavesEmptyFavoritedSlotsEmpty, nameof(SortLeavesEmptyFavoritedSlotsEmpty), "");
            DisplaySortButtons = this.BindConfig(sectionName, DisplaySortButtons, nameof(DisplaySortButtons), "");
            DisplaySortCriteriaInLabel = this.BindConfig(sectionName, DisplaySortCriteriaInLabel, nameof(DisplaySortCriteriaInLabel), "");
            SortInAscendingOrder = this.BindConfig(sectionName, SortInAscendingOrder, nameof(SortInAscendingOrder), "");
            SortMergesStacks = this.BindConfig(sectionName, SortMergesStacks, nameof(SortMergesStacks), "");
            SortKey = this.BindConfig(sectionName, SortKey, nameof(SortKey), "");

            sectionName = "5 - Trashing";

            // TODO descriptions

            TrashItems.EnableQuickTrash = this.BindConfig(sectionName, TrashItems.EnableQuickTrash, nameof(TrashItems.EnableQuickTrash), "");
            TrashItems.ShowConfirmDialogForNormalItem = this.BindConfig(sectionName, TrashItems.ShowConfirmDialogForNormalItem, nameof(TrashItems.ShowConfirmDialogForNormalItem), "");
            TrashItems.ShowConfirmDialogForQuickTrash = this.BindConfig(sectionName, TrashItems.ShowConfirmDialogForQuickTrash, nameof(TrashItems.ShowConfirmDialogForQuickTrash), "");
            TrashItems.DisplayTrashCanUI = this.BindConfig(sectionName, TrashItems.DisplayTrashCanUI, nameof(TrashItems.DisplayTrashCanUI), "");
            TrashItems.AlwaysConsiderTrophiesTrashFlagged = this.BindConfig(sectionName, TrashItems.AlwaysConsiderTrophiesTrashFlagged, nameof(TrashItems.AlwaysConsiderTrophiesTrashFlagged), "");
            TrashItems.TrashHotkey = this.BindConfig(sectionName, TrashItems.TrashHotkey, nameof(TrashItems.TrashHotkey), "");
            TrashItems.TrashLabelColor = this.BindConfig(sectionName, TrashItems.TrashLabelColor, nameof(TrashItems.TrashLabelColor), "");

            sectionName = "9 - Localization";

            // TODO descriptions
            TrashItems.TrashLabel = this.BindConfig(sectionName, TrashItems.TrashLabel, nameof(TrashItems.TrashLabel), "");
            QuickStackLabel = this.BindConfig(sectionName, QuickStackLabel, nameof(QuickStackLabel), "");

            QuickStackResultMessageNone = this.BindConfig(sectionName, QuickStackResultMessageNone, nameof(QuickStackResultMessageNone), "");
            QuickStackResultMessageOne = this.BindConfig(sectionName, QuickStackResultMessageOne, nameof(QuickStackResultMessageOne), "");
            QuickStackResultMessageMore = this.BindConfig(sectionName, QuickStackResultMessageMore, nameof(QuickStackResultMessageMore), "");

            StoreAllLabel = this.BindConfig(sectionName, StoreAllLabel, nameof(StoreAllLabel), "");

            SortLabel = this.BindConfig(sectionName, SortLabel, nameof(SortLabel), "");

            SortByInternalNameLabel = this.BindConfig(sectionName, SortByInternalNameLabel, nameof(SortByInternalNameLabel), "");
            SortByTranslatedNameLabel = this.BindConfig(sectionName, SortByTranslatedNameLabel, nameof(SortByTranslatedNameLabel), "");
            SortByValueLabel = this.BindConfig(sectionName, SortByValueLabel, nameof(SortByValueLabel), "");
            SortByWeightLabel = this.BindConfig(sectionName, SortByWeightLabel, nameof(SortByWeightLabel), "");

            QuickStackLabelCharacter = this.BindConfig(sectionName, QuickStackLabelCharacter, nameof(QuickStackLabelCharacter), "");
            SortLabelCharacter = this.BindConfig(sectionName, SortLabelCharacter, nameof(SortLabelCharacter), "");
        }

        private void OnSettingChanged(object sender, SettingChangedEventArgs e)
        {
            LoadConfig();

            // reminder to never use ?. on monobehaviors

            if (InventoryGui.instance != null && InventoryGui.instance.m_takeAllButton != null)
            {
                InventoryGui.instance.m_takeAllButton.transform.localPosition = origButtonPosition;
            }

            if (depositAllButton != null)
            {
                Destroy(depositAllButton.gameObject);
            }

            if (quickStackAreaButton != null)
            {
                Destroy(quickStackAreaButton.gameObject);
            }

            if (quickStackToChestButton != null)
            {
                Destroy(quickStackToChestButton.gameObject);
            }

            if (sortChestButton != null)
            {
                Destroy(sortChestButton.gameObject);
            }

            if (sortInventoryButton != null)
            {
                Destroy(sortInventoryButton.gameObject);
            }

            if (TrashItems.trashRoot != null)
            {
                Destroy(TrashItems.trashRoot.gameObject);
            }

            StartCoroutine(UpdateUIElements(InventoryGui.instance));
        }

        /// <summary>
        /// Wait one frame for Destroy to finish, then reset UI
        /// </summary>
        public IEnumerator UpdateUIElements(InventoryGui instance)
        {
            yield return null;
            PatchGuiShow.Show_Postfix(instance);
            TrashItems.TrashItemsPatches.Show_Postfix(instance);
        }

        public static List<Container> FindContainersInRange(Vector3 point, float range)
        {
            List<Container> list = new List<Container>();

            foreach (Container container in AllContainers)
            {
                if (container != null && container.transform != null && Vector3.Distance(point, container.transform.position) < range)
                {
                    list.Add(container);
                }
            }

            return list;
        }

        public static List<Container> FindNearbyContainers(Vector3 point)
        {
            return FindContainersInRange(point, QuickStackToNearbyRange);
        }

        private static void TakeAllItemsInOrder(Player player)
        {
            Inventory fromInventory = InventoryGui.instance.m_currentContainer.GetInventory();
            Inventory toInventory = player.GetInventory();

            MoveAllItemsInOrder(player, fromInventory, toInventory, true);
        }

        private static void StoreAllItemsInOrder(Player player)
        {
            Inventory fromInventory = player.GetInventory();
            Inventory toInventory = InventoryGui.instance.m_currentContainer.GetInventory();

            MoveAllItemsInOrder(player, fromInventory, toInventory);
        }

        private static bool ShouldMoveItem(ItemData item, UserConfig playerConfig, bool takeAllOverride = false)
        {
            return takeAllOverride ||
                ((!StoreAllIncludesHotkeyBar || item.m_gridPos.y != 0)
                && (!StoreAllIncludesEquippedItems || !item.m_equiped)
                && !playerConfig.IsItemNameOrSlotFavorited(item)
                && !CompatibilitySupport.IsEquipOrQuickSlot(item.m_gridPos));
        }

        private static void MoveAllItemsInOrder(Player player, Inventory fromInventory, Inventory toInventory, bool takeAllOverride = false)
        {
            if (player.IsTeleporting() || !InventoryGui.instance.m_container)
            {
                return;
            }

            InventoryGui.instance.SetupDragItem(null, null, 0);

            UserConfig playerConfig = GetPlayerConfig(player.GetPlayerID());
            var list = fromInventory.GetAllItems().Where((item) => ShouldMoveItem(item, playerConfig, takeAllOverride)).ToList();

            list.Sort((ItemData a, ItemData b) => CompareSlotOrder(a.m_gridPos, b.m_gridPos));

            int num = 0;

            foreach (ItemData itemData in list)
            {
                if (toInventory.AddItem(itemData))
                {
                    fromInventory.RemoveItem(itemData);
                    num++;

                    if (itemData.m_equiped)
                    {
                        Player.m_localPlayer.RemoveEquipAction(itemData);
                        Player.m_localPlayer.UnequipItem(itemData, false);
                    }
                }
            }

            Debug.Log($"Moved {num} item/s to container");

            toInventory.Changed();
            fromInventory.Changed();
        }

        private static int CompareSlotOrder(Vector2i a, Vector2i b)
        {
            // Bottom left to top right
            var yPosCompare = -a.y.CompareTo(b.y);

            if (UseTopDownLogicForEverything)
            {
                // Top left to bottom right
                yPosCompare *= -1;
            }

            return yPosCompare != 0 ? yPosCompare : a.x.CompareTo(b.x);
        }

        private static bool ShouldQuickStackItem(ItemData item, UserConfig playerConfig)
        {
            return item.m_shared.m_maxStackSize != 1 && (!QuickStackIncludesHotkeyBar || item.m_gridPos.y != 0) && !playerConfig.IsItemNameOrSlotFavorited(item) && !CompatibilitySupport.IsEquipOrQuickSlot(item.m_gridPos);
        }

        internal static int StackItems(Player player, Inventory fromInventory, Inventory toInventory)
        {
            UserConfig playerConfig = GetPlayerConfig(player.GetPlayerID());
            List<ItemData> list = fromInventory.GetAllItems().Where((itm) => ShouldQuickStackItem(itm, playerConfig)).ToList();

            // sort in reverse, because we iterate in reverse
            list.Sort((ItemData a, ItemData b) => -1 * CompareSlotOrder(a.m_gridPos, b.m_gridPos));

            int num = 0;
            ItemData item;

            if (QuickStackTrophiesIntoSameContainer)
            {
                List<ItemData> trophies = new List<ItemData>();
                toInventory.GetAllItems(ItemData.ItemType.Trophie, trophies);

                if (trophies.Count > 0)
                {
                    for (int i = list.Count - 1; i >= 0; i--)
                    {
                        item = list[i];

                        if (item.m_shared.m_itemType == ItemData.ItemType.Trophie)
                        {
                            if (toInventory.AddItem(item))
                            {
                                fromInventory.RemoveItem(item);
                                list.RemoveAt(i);
                                num++;
                            }
                        }
                    }
                }
            }

            for (int i = list.Count - 1; i >= 0; i--)
            {
                item = list[i];
                if (toInventory.HaveItem(item.m_shared.m_name))
                {
                    if (toInventory.AddItem(item))
                    {
                        fromInventory.RemoveItem(item);
                        num++;
                    }
                }
            }

            toInventory.Changed();
            fromInventory.Changed();

            return num;
        }

        public static void ReportResult(Player player, int movedCount)
        {
            if (!ShowQuickStackResultMessage)
            {
                return;
            }

            string message;

            if (movedCount == 0)
            {
                message = QuickStackResultMessageNone;
            }
            else if (movedCount == 1)
            {
                message = QuickStackResultMessageOne;
            }
            else
            {
                message = string.Format(QuickStackResultMessageMore, movedCount);
            }

            player.Message(MessageHud.MessageType.Center, message, 0, null);
        }

        private static void StackToMany(Player player, List<Container> containers, int initialReportCount = 0)
        {
            Inventory inventory = player.GetInventory();
            int num = initialReportCount;

            foreach (Container container in containers)
            {
                ZNetView nview = container.m_nview;
                var inUse = nview.GetZDO().GetInt("InUse", 0) == 1;

                // container.IsInUse is only client side
                // ownership is useless since every container always has an owner (its last user)
                if (!inUse && !container.IsInUse() && container.CheckAccess(player.GetPlayerID())
                    && (!container.m_checkGuardStone || PrivateArea.CheckAccess(container.transform.position, 0f, true, false)))
                {
                    nview.ClaimOwnership();
                    //ZDOMan.instance.ForceSendZDO(ZNet.instance.GetUID(), nview.GetZDO().m_uid);

                    if (SuppressContainerSoundAndVisuals)
                    {
                        container.m_inUse = true;
                        nview.GetZDO().Set("InUse", container.m_inUse ? 1 : 0);
                        ZDOMan.instance.ForceSendZDO(ZNet.instance.GetUID(), nview.GetZDO().m_uid);

                        num += StackItems(player, inventory, container.GetInventory());

                        container.m_inUse = false;
                        nview.GetZDO().Set("InUse", container.m_inUse ? 1 : 0);
                        ZDOMan.instance.ForceSendZDO(ZNet.instance.GetUID(), nview.GetZDO().m_uid);
                    }
                    else
                    {
                        container.SetInUse(true);
                        ZDOMan.instance.ForceSendZDO(ZNet.instance.GetUID(), nview.GetZDO().m_uid);

                        num += StackItems(player, inventory, container.GetInventory());

                        container.SetInUse(false);
                        ZDOMan.instance.ForceSendZDO(ZNet.instance.GetUID(), nview.GetZDO().m_uid);
                    }
                }
            }

            ReportResult(player, num);
        }

        public static void DoQuickStack(Player player, bool StackOnlyToCurrentContainerOverride = false)
        {
            if (player.IsTeleporting() || !InventoryGui.instance.m_container)
            {
                return;
            }

            InventoryGui.instance.SetupDragItem(null, null, 0);

            int movedCount = 0;
            Container container = InventoryGui.instance.m_currentContainer;

            if (container != null)
            {
                movedCount = StackItems(player, player.GetInventory(), container.GetInventory());

                if (QuickStackHotkeyBehaviorWhenContainerOpen != QuickStackBehavior.QuickStackOnlyToContainer || StackOnlyToCurrentContainerOverride)
                {
                    ReportResult(player, movedCount);
                    return;
                }
            }

            List<Container> list = FindNearbyContainers(player.transform.position);

            if (list.Count > 0)
            {
                Debug.Log($"Found {list.Count} containers in range");
                StackToMany(player, list, movedCount);
            }
        }

        private static float origButtonLength = -1;
        private static Vector3 origButtonPosition;

        private static Button quickStackAreaButton;
        private static Button sortInventoryButton;

        private static Button quickStackToChestButton;
        private static Button depositAllButton;
        private static Button sortChestButton;

        public static Sprite border;

        // TODO if this list gets to big, dynamically switch to different system
        public static List<Container> AllContainers = new List<Container>();

        private static readonly Dictionary<long, UserConfig> playerConfigs = new Dictionary<long, UserConfig>();

        public static bool UseTopDownLogicForEverything = false;

        // TODO
        public static bool DisableAllNewKeybinds = false;

        // TODO
        public static bool DisableAllNewButtons = false;

        public static float QuickStackToNearbyRange = 10f;
        public static KeyCode QuickStackKey = KeyCode.P;
        public static bool SuppressContainerSoundAndVisuals = true;
        public static bool ShowQuickStackResultMessage = true;
        public static bool QuickStackIncludesHotkeyBar = true;
        public static bool QuickStackTrophiesIntoSameContainer = false;
        public static QuickStackBehavior QuickStackHotkeyBehaviorWhenContainerOpen = QuickStackBehavior.QuickStackOnlyToContainer;
        public static bool DisplayQuickStackButtons = true;

        public enum QuickStackBehavior
        {
            QuickStackOnlyToContainer,
            QuickStackToBoth
        }

        public static bool StoreAllIncludesEquippedItems = true;
        public static bool StoreAllIncludesHotkeyBar = true;
        public static bool DisplayStoreAllButton = true;
        public static bool ChestsUseImprovedTakeAllLogic = true;

        public static bool SortIncludesHotkeyBar = true;
        public static bool SortInAscendingOrder = true;
        public static bool SortLeavesEmptyFavoritedSlotsEmpty = false;
        public static SortBehavior SortHotkeyBehaviorWhenContainerOpen = SortBehavior.OnlySortContainer;
        public static bool DisplaySortCriteriaInLabel = false;
        public static bool DisplaySortButtons = true;
        public static KeyCode SortKey = KeyCode.R;
        public static bool SortMergesStacks = true;
        public static SortCriteriaEnum SortCriteria = SortCriteriaEnum.InternalName;

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

        public static KeyCode FavoriteModifierKey1 = KeyCode.LeftAlt;
        public static KeyCode FavoriteModifierKey2 = KeyCode.RightAlt;
        public static Color BorderColorFavoritedItem = new Color(1f, 0.8482759f, 0f); // valheim yellow/ orange-ish
        public static Color BorderColorFavoritedSlot = new Color(0f, 0.5f, 1f); // light-ish blue
        public static Color BorderColorFavoritedItemOnFavoritedSlot = new Color(0.5f, 0.67413795f, 0.5f); // dark-ish green
        public static Color BorderColorTrashFlaggedItem = new Color(0.5f, 0f, 0); // dark-ish red
        public static Color BorderColorTrashFlaggedItemOnFavoritedSlot = Color.black;

        public static string QuickStackLabelCharacter = "Q";
        public static string SortLabelCharacter = "S";

        public static string QuickStackResultMessageNone = "Nothing to stack";
        public static string QuickStackResultMessageOne = "Stacked 1 item";
        public static string QuickStackResultMessageMore = "Stacked {0} items";

        public static string QuickStackLabel = "Quick Stack";
        public static string StoreAllLabel = "Store All";
        public static string SortLabel = "Sort";

        public static string SortByInternalNameLabel = "i. name";
        public static string SortByTranslatedNameLabel = "t. name";
        public static string SortByValueLabel = "value";
        public static string SortByWeightLabel = "weight";
    }
}