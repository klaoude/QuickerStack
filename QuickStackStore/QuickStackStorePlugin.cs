using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace QuickStackStore
{
    // QuickStackSortStoreTakeTrash (QSSSTT)
    [BepInPlugin("org.bepinex.plugins.valheim.quick_stack_store", "Quick Stack and Store", VERSION)]
    public class QuickStackStorePlugin : BaseUnityPlugin
    {
        private const string VERSION = "0.9";

        // TODO maybe add a sort button (using this as the base, even if it's currently quite broken https://github.com/aedenthorn/ValheimMods/blob/master/SimpleSort/BepInExPlugin.cs )
        // TODO controller support
        public void Awake()
        {
            Logger.LogInfo(String.Format("Initializing Quick Stack and Store {0}.", VERSION));

            border = LoadSprite("QuickStackStore.border.png", new Rect(0, 0, 1024, 1024), new Vector2(512, 512));

            this.LoadConfig();
            Config.SettingChanged += OnSettingChanged;

            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), null);
        }

        // taken from the 'Trash Items' mod, as allowed in their permission settings on nexus
        // https://www.nexusmods.com/valheim/mods/441
        // https://github.com/virtuaCode/valheim-mods/tree/main/TrashItems
        public static Sprite LoadSprite(string path, Rect size, Vector2 pivot, int units = 100)
        {
            Assembly asm = Assembly.GetExecutingAssembly();
            Stream img = asm.GetManifestResourceStream(path);

            Texture2D tex = new Texture2D((int)size.width, (int)size.height, TextureFormat.RGBA32, false, true);

            using (MemoryStream mStream = new MemoryStream())
            {
                img.CopyTo(mStream);
                tex.LoadImage(mStream.ToArray());
                tex.Apply();
                return Sprite.Create(tex, size, pivot, units);
            }
        }

        public void Update()
        {
            if (InventoryGui.instance == null)
            {
                return;
            }

            float shrinkFactor = 0.9f;
            var buttonRect = InventoryGui.instance.m_takeAllButton.GetComponent<RectTransform>();

            if (origButtonLength == -1)
            {
                origButtonLength = buttonRect.sizeDelta.x;
                origButtonPosition = buttonRect.localPosition;

                buttonRect.GetComponent<Button>().onClick.RemoveAllListeners();
                buttonRect.GetComponent<Button>().onClick.AddListener(new UnityAction(() => ContextSensitiveTakeAll(InventoryGui.instance)));
            }

            if (buttonRect.sizeDelta.x == origButtonLength)
            {
                buttonRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, origButtonLength * shrinkFactor);
            }

            int hPadding = 4;
            int vPadding = 8;

            if (buttonRect.localPosition == origButtonPosition)
            {
                if (!DisplayStoreButtonAndMoveTakeButton)
                {
                    // move the button to the left by half of its removed length
                    buttonRect.localPosition -= new Vector3((origButtonLength / 2) * (1 - shrinkFactor), 0);
                }
                else
                {
                    //TODO the horizontal value should be dependant on shrinkFactor, but I hit the perfect value for 0.9
                    buttonRect.localPosition += new Vector3(440f, 0f);
                    buttonRect.localPosition += new Vector3(origButtonLength, 0f);
                    buttonRect.localPosition -= new Vector3(0, buttonRect.sizeDelta.y);
                    buttonRect.localPosition += new Vector3(hPadding, -vPadding);
                }
            }

            if (DisplayStoreButtonAndMoveTakeButton)
            {
                if (depositAllButton == null)
                {
                    depositAllButton = Instantiate(InventoryGui.instance.m_takeAllButton, buttonRect.parent);
                    depositAllButton.transform.localPosition = buttonRect.localPosition;
                    depositAllButton.transform.localPosition -= new Vector3(0, buttonRect.sizeDelta.y + vPadding);

                    //depositAllButton.transform.localPosition = origButtonPosition + new Vector3(buttonLength, 0f, -1f);
                    //// move the button to the left by 1.5 its removed length (to also compensate for the takeall button movement)
                    //depositAllButton.transform.localPosition -= new Vector3(buttonLength * 1.5f * (1 - shrinkFactor), 0);

                    depositAllButton.onClick.RemoveAllListeners();
                    depositAllButton.onClick.AddListener(new UnityAction(() => StoreAllItemsInOrder(Player.m_localPlayer)));

                    depositAllButton.GetComponentInChildren<Text>().text = "Store All";
                }
            }

            if (DisplayStackButtons)
            {
                if (quickStackAreaButton == null)
                {
                    quickStackAreaButton = CreateQuickStackAreaButton(InventoryGui.instance, nameof(quickStackAreaButton), "Q");
                    quickStackAreaButton.onClick.RemoveAllListeners();
                    quickStackAreaButton.onClick.AddListener(new UnityAction(() => DoQuickStack(Player.m_localPlayer)));
                }

                if (quickStackToChestButton == null)
                {
                    quickStackToChestButton = Instantiate(InventoryGui.instance.m_takeAllButton, buttonRect.parent);

                    if (DisplayStoreButtonAndMoveTakeButton)
                    {
                        quickStackToChestButton.transform.localPosition = buttonRect.localPosition;
                        quickStackToChestButton.transform.localPosition += new Vector3(0, buttonRect.sizeDelta.y + vPadding);
                    }
                    else
                    {
                        quickStackToChestButton.transform.localPosition = origButtonPosition + new Vector3(440f, 0f, -1f);
                        //move the button to the right by half of its removed length
                        quickStackToChestButton.transform.localPosition += new Vector3((origButtonLength / 2) * (1 - shrinkFactor), 0);
                    }

                    quickStackToChestButton.onClick.RemoveAllListeners();
                    quickStackToChestButton.onClick.AddListener(new UnityAction(() => DoQuickStack(Player.m_localPlayer, true, true)));

                    quickStackToChestButton.GetComponentInChildren<Text>().text = "Quick Stack";
                }
            }
        }

        public void ContextSensitiveTakeAll(InventoryGui instance)
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

        private Button CreateQuickStackAreaButton(InventoryGui instance, string name, string buttonText)
        {
            var playerInventory = InventoryGui.instance.m_player.transform;

            var weight = playerInventory.Find("Weight");

            Transform obj = Instantiate(instance.m_takeAllButton.transform, weight.parent);
            obj.localPosition = weight.localPosition + new Vector3(0f, -56f, 0f);
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
            return Config.Bind<T>(section, key, param, description).Value;
        }

        public static UserConfig GetPlayerConfig(long playerID)
        {
            if (QuickStackStorePlugin.playerConfigs.TryGetValue(playerID, out UserConfig userConfig))
            {
                return userConfig;
            }
            else
            {
                userConfig = new UserConfig(playerID);
                QuickStackStorePlugin.playerConfigs[playerID] = userConfig;

                return userConfig;
            }
        }

        private void LoadConfig()
        {
            string sectionName = "1 - Quick Stacking";

            QuickStackKey = this.BindConfig(sectionName, QuickStackKey, nameof(QuickStackKey), "The hotkey to immediately start quick stacking to nearby chests.");
            NearbyRange = this.BindConfig(sectionName, NearbyRange, nameof(NearbyRange), "How far from you is nearby, greater value = greater range.");
            HotkeyStacksToCurrentContainer = this.BindConfig(sectionName, HotkeyStacksToCurrentContainer, nameof(HotkeyStacksToCurrentContainer), "Whether to ignore the container that you are currently using or not.");
            HotkeyOnlyStacksToCurrentContainerIfOpen = this.BindConfig(sectionName, HotkeyOnlyStacksToCurrentContainerIfOpen, nameof(HotkeyOnlyStacksToCurrentContainerIfOpen), "Whether to only stack to the currently open container (like QuickStack did), or also look at all nearby containers afterwards.");
            StackIgnoresConsumable = this.BindConfig(sectionName, StackIgnoresConsumable, nameof(StackIgnoresConsumable), "Whether to completely exclude consumables from quick stacking (food, potions).");
            StackIgnoresAmmo = this.BindConfig(sectionName, StackIgnoresAmmo, nameof(StackIgnoresAmmo), "Whether to completely exclude ammo from quick stacking (arrows).");
            PutTrophiesTogether = this.BindConfig(sectionName, PutTrophiesTogether, nameof(PutTrophiesTogether), "Whether to put all types of trophies in the container if any trophy is found in that container.");
            DisplayStackButtons = this.BindConfig(sectionName, DisplayStackButtons, nameof(DisplayStackButtons), "Whether to display the two quick stack buttons. Hotkeys work independently.");

            sectionName = "2 - Store and Take All";

            DisplayStoreButtonAndMoveTakeButton = this.BindConfig(sectionName, DisplayStoreButtonAndMoveTakeButton, nameof(DisplayStoreButtonAndMoveTakeButton), "Whether to add the 'store all' button and move the 'take all' button.");
            StoreIgnoresEquipped = this.BindConfig(sectionName, StoreIgnoresEquipped, nameof(StoreIgnoresEquipped), "Whether 'Store All' should exclude or also unequip and store equipped items.");
            ChestsUseImprovedTakeAllLogic = this.BindConfig(sectionName, ChestsUseImprovedTakeAllLogic, nameof(ChestsUseImprovedTakeAllLogic), "Whether to use the improved logic for 'Take All' for non tomb stones. Disable if needed for compatibility.");

            sectionName = "3 - Favoriting";

            FavoriteModifierKey1 = this.BindConfig(sectionName, FavoriteModifierKey1, nameof(FavoriteModifierKey1), $"While holding this, left clicking on slots or right clicking on items favorites them. Identical to {nameof(FavoriteModifierKey2)}.");
            FavoriteModifierKey2 = this.BindConfig(sectionName, FavoriteModifierKey2, nameof(FavoriteModifierKey2), $"While holding this, left clicking on slots or right clicking on items favorites them. Identical to {nameof(FavoriteModifierKey1)}.");
            BorderColorFavoriteItem = this.BindConfig(sectionName, BorderColorFavoriteItem, nameof(BorderColorFavoriteItem), "Color of the border for slots containing favorited items.");
            BorderColorFavoriteSlot = this.BindConfig(sectionName, BorderColorFavoriteSlot, nameof(BorderColorFavoriteSlot), "Color of the border for favorited slots.");
            BorderColorFavoriteBoth = this.BindConfig(sectionName, BorderColorFavoriteBoth, nameof(BorderColorFavoriteBoth), "If not disabled, color of the border of a favorited slots that also contains a favorited item.");
            MixColorsInsteadOfUsingFavoriteBothColor = this.BindConfig(sectionName, MixColorsInsteadOfUsingFavoriteBothColor, nameof(MixColorsInsteadOfUsingFavoriteBothColor), "Whether to mix the item and slot color to produce the 'FavoriteBoth' color or use the config color.");
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
        }

        public static List<Container> FindContainersInRange(Vector3 point, float range)
        {
            List<Container> list = new List<Container>();

            foreach (Container container in QuickStackStorePlugin.AllContainers)
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
            return QuickStackStorePlugin.FindContainersInRange(point, QuickStackStorePlugin.NearbyRange);
        }

        private void TakeAllItemsInOrder(Player player)
        {
            Inventory fromInventory = InventoryGui.instance.m_currentContainer.GetInventory();
            Inventory toInventory = player.GetInventory();

            MoveAllItemsInOrder(player, fromInventory, toInventory, true);
        }

        private void StoreAllItemsInOrder(Player player)
        {
            Inventory fromInventory = player.GetInventory();
            Inventory toInventory = InventoryGui.instance.m_currentContainer.GetInventory();

            MoveAllItemsInOrder(player, fromInventory, toInventory);
        }

        private void MoveAllItemsInOrder(Player player, Inventory fromInventory, Inventory toInventory, bool takeAllOverride = false)
        {
            if (player.IsTeleporting() || !InventoryGui.instance.m_container)
            {
                return;
            }

            InventoryGui.instance.SetupDragItem(null, null, 1);

            UserConfig playerConfig = QuickStackStorePlugin.GetPlayerConfig(player.GetPlayerID());
            List<ItemDrop.ItemData> list = new List<ItemDrop.ItemData>(fromInventory.GetAllItems());

            var width = fromInventory.GetWidth();
            list.Sort((a, b) => CompareSlotOrder(a.m_gridPos, b.m_gridPos));

            int num = 0;

            foreach (ItemDrop.ItemData itemData in list)
            {
                if (takeAllOverride || ((StoreIgnoresEquipped || !itemData.m_equiped) && !playerConfig.IsSlotFavorited(itemData.m_gridPos) && !playerConfig.IsItemFavorited(itemData.m_shared)))
                {
                    if (toInventory.AddItem(itemData))
                    {
                        fromInventory.RemoveItem(itemData);
                        num++;

                        if (itemData.m_equiped)
                        {
                            Player.m_localPlayer.UnequipItem(itemData, false);
                        }
                    }
                }
            }

            Debug.Log($"Moved {num} item/s to container");

            toInventory.Changed();
            fromInventory.Changed();
        }

        /// <summary>
        /// Bottom left to top right
        /// </summary>
        private int CompareSlotOrder(Vector2i a, Vector2i b)
        {
            var yPos = -a.y.CompareTo(b.y);

            return yPos != 0 ? yPos : a.x.CompareTo(b.x);
        }

        internal static int StackItems(Player player, Inventory fromInventory, Inventory toInventory)
        {
            UserConfig playerConfig = QuickStackStorePlugin.GetPlayerConfig(player.GetPlayerID());
            List<ItemDrop.ItemData> list = new List<ItemDrop.ItemData>(fromInventory.GetAllItems());
            List<ItemDrop.ItemData> list2 = new List<ItemDrop.ItemData>();
            List<ItemDrop.ItemData> list3 = new List<ItemDrop.ItemData>();
            toInventory.GetAllItems(ItemDrop.ItemData.ItemType.Trophie, list2);
            int num = 0;

            foreach (ItemDrop.ItemData itemData in list)
            {
                if (itemData.m_shared.m_maxStackSize != 1 &&
                    (!QuickStackStorePlugin.StackIgnoresAmmo || itemData.m_shared.m_itemType != ItemDrop.ItemData.ItemType.Ammo) &&
                    (!QuickStackStorePlugin.StackIgnoresConsumable || itemData.m_shared.m_itemType != ItemDrop.ItemData.ItemType.Consumable) &&
                    !playerConfig.IsSlotFavorited(itemData.m_gridPos) && !playerConfig.IsItemFavorited(itemData.m_shared))
                {
                    if (itemData.m_shared.m_itemType == ItemDrop.ItemData.ItemType.Trophie && QuickStackStorePlugin.PutTrophiesTogether && list2.Count > 0)
                    {
                        if (toInventory.AddItem(itemData))
                        {
                            fromInventory.RemoveItem(itemData);
                            num++;
                        }
                        else
                        {
                            list3.Add(itemData);
                        }
                    }
                    else if (toInventory.HaveItem(itemData.m_shared.m_name))
                    {
                        if (toInventory.AddItem(itemData))
                        {
                            //Debug.Log($"Storing {itemData.m_shared.m_name} in container");
                            fromInventory.RemoveItem(itemData);
                            num++;
                        }
                        else
                        {
                            list3.Add(itemData);
                        }
                    }
                }
            }

            foreach (ItemDrop.ItemData itemData2 in list3)
            {
                if (toInventory.AddItem(itemData2))
                {
                    //Debug.Log($"Storing {itemData2.m_shared.m_name} in container");
                    fromInventory.RemoveItem(itemData2);
                    num++;
                }
            }

            toInventory.Changed();
            fromInventory.Changed();

            return num;
        }

        public static void ReportResult(Player player, int movedCount)
        {
            player.Message(
                MessageHud.MessageType.Center,
                (movedCount > 0) ? string.Format("Stacked {0} item{1}", movedCount, (movedCount == 1) ? "" : "s") : "Nothing to stack",
                0,
                null
           );
        }

        private static void StackToMany(Player player, List<Container> containers, int initialReportCount = 0)
        {
            Inventory inventory = player.GetInventory();
            int num = initialReportCount;

            foreach (Container container in containers)
            {
                if ((!container.m_checkGuardStone ||
                    PrivateArea.CheckAccess(container.transform.position, 0f, true, false)) &&
                    container.CheckAccess(player.GetPlayerID()) &&
                    !container.IsInUse())
                {
                    ZNetView nview = container.m_nview;
                    nview.ClaimOwnership();
                    ZDOMan.instance.ForceSendZDO(ZNet.instance.GetUID(), nview.GetZDO().m_uid);
                    container.SetInUse(true);
                    num += QuickStackStorePlugin.StackItems(player, inventory, container.GetInventory());
                    container.SetInUse(false);
                }
            }
            QuickStackStorePlugin.ReportResult(player, num);
        }

        public static void DoQuickStack(Player player, bool StackToCurrentContainerOverride = false, bool StackOnlyToCurrentContainerOverride = false)
        {
            if (player.IsTeleporting() || !InventoryGui.instance.m_container)
            {
                return;
            }

            InventoryGui.instance.SetupDragItem(null, null, 1);

            int movedCount = 0;
            Container container = InventoryGui.instance.m_currentContainer;

            if ((HotkeyStacksToCurrentContainer || StackToCurrentContainerOverride) && container != null)
            {
                movedCount = StackItems(player, player.GetInventory(), container.GetInventory());

                if (HotkeyOnlyStacksToCurrentContainerIfOpen || StackOnlyToCurrentContainerOverride)
                {
                    ReportResult(player, movedCount);
                    return;
                }
            }

            List<Container> list = QuickStackStorePlugin.FindNearbyContainers(player.transform.position);

            if (list.Count > 0)
            {
                Debug.Log($"Found {list.Count} containers in range");
                StackToMany(player, list, movedCount);
            }
        }

        private float origButtonLength = -1;
        private Vector3 origButtonPosition;

        private Button quickStackToChestButton;
        private Button quickStackAreaButton;
        private Button depositAllButton;

        public static Sprite border;
        public static List<Container> AllContainers = new List<Container>();

        private static readonly Dictionary<long, UserConfig> playerConfigs = new Dictionary<long, UserConfig>();

        public static float NearbyRange = 10f;
        public static KeyCode QuickStackKey = KeyCode.P;
        public static bool PutTrophiesTogether = false;

        public static bool StackIgnoresAmmo = false;
        public static bool StackIgnoresConsumable = false;
        public static bool HotkeyStacksToCurrentContainer = true;
        public static bool HotkeyOnlyStacksToCurrentContainerIfOpen = false;
        public static bool DisplayStackButtons = true;

        public static bool StoreIgnoresEquipped = false;
        public static bool DisplayStoreButtonAndMoveTakeButton = true;
        public static bool ChestsUseImprovedTakeAllLogic = true;

        public static KeyCode FavoriteModifierKey1 = KeyCode.LeftAlt;
        public static KeyCode FavoriteModifierKey2 = KeyCode.RightAlt;
        public static Color BorderColorFavoriteItem = new Color(1f, 0.8482759f, 0f); // valheim yellow/ orange-ish
        public static Color BorderColorFavoriteSlot = new Color(0f, 0.5f, 1); // light-ish blue
        public static Color BorderColorFavoriteBoth = Color.green;
        public static bool MixColorsInsteadOfUsingFavoriteBothColor = true;
    }
}