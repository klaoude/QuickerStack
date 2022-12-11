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
    [BepInPlugin("org.bepinex.plugins.valheim.quick_stack_store", "Quick Stack and Store", VERSION)]
    public class QuickStackStorePlugin : BaseUnityPlugin
    {
        private const string VERSION = "0.0.5";
        public static Sprite border;

        public void Awake()
        {
            Logger.LogInfo(String.Format("Initializing Quick Stack and Store {0}.", VERSION));

            if (File.Exists(this.ConfigPath))
            {
                QuickStackStorePlugin.configFile = new ConfigFile(this.ConfigPath, true);
                this.LoadConfig();
            }
            else
            {
                QuickStackStorePlugin.configFile = base.Config;
                this.LoadConfig();
            }

            border = LoadSprite("QuickStackStore.border.png", new Rect(0, 0, 1024, 1024), new Vector2(512, 512));

            Config.SettingChanged += OnSettingChanged;

            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), null);
        }

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
            }

            if (buttonRect.sizeDelta.x == origButtonLength)
            {
                buttonRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, origButtonLength * shrinkFactor);
            }

            int hPadding = 4;
            int vPadding = 8;

            if (buttonRect.localPosition == origButtonPosition)
            {
                if (!DisplayUIButtons)
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

            if (!DisplayUIButtons)
            {
                return;
            }

            if (quickStackToChestButton == null)
            {
                quickStackToChestButton = Instantiate(InventoryGui.instance.m_takeAllButton, buttonRect.parent);
                quickStackToChestButton.transform.localPosition = buttonRect.localPosition;
                quickStackToChestButton.transform.localPosition += new Vector3(0, buttonRect.sizeDelta.y + vPadding);

                // unshrink the quickstack button
                //quickStackToChestButton.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, buttonLength * (1 / shrinkFactor));
                //quickStackToChestButton.transform.localPosition = origButtonPosition + new Vector3(440f, 0f, -1f);
                // move the button to the right by half of its removed length
                //quickStackToChestButton.transform.localPosition += new Vector3((buttonLength / 2) * (1 - shrinkFactor), 0);

                quickStackToChestButton.onClick.RemoveAllListeners();
                quickStackToChestButton.onClick.AddListener(new UnityAction(() => DoQuickStack(Player.m_localPlayer, true, true)));

                quickStackToChestButton.GetComponentInChildren<Text>().text = "Quick Stack";

                Debug.Log("Spawned quickstack button");
            }

            if (depositAllButton == null)
            {
                depositAllButton = Instantiate(InventoryGui.instance.m_takeAllButton, buttonRect.parent);
                depositAllButton.transform.localPosition = buttonRect.localPosition;
                depositAllButton.transform.localPosition -= new Vector3(0, buttonRect.sizeDelta.y + vPadding);

                //depositAllButton.transform.localPosition = origButtonPosition + new Vector3(buttonLength, 0f, -1f);
                //// move the button to the left by 1.5 its removed length (to also compensate for the takeall button movement)
                //depositAllButton.transform.localPosition -= new Vector3(buttonLength * 1.5f * (1 - shrinkFactor), 0);

                depositAllButton.onClick.RemoveAllListeners();
                depositAllButton.onClick.AddListener(new UnityAction(() => StoreItems(Player.m_localPlayer)));

                depositAllButton.GetComponentInChildren<Text>().text = "Store All";

                Debug.Log("Spawned depositall button");
            }

            if (quickStackAreaButton == null)
            {
                quickStackAreaButton = CreateQuickStackAreaButton(InventoryGui.instance, nameof(quickStackAreaButton), "Q");
                quickStackAreaButton.onClick.RemoveAllListeners();
                quickStackAreaButton.onClick.AddListener(new UnityAction(() => DoQuickStack(Player.m_localPlayer)));
            }
        }

        private Button CreateQuickStackAreaButton(InventoryGui __instance, string name, string buttonText)
        {
            var playerInventory = InventoryGui.instance.m_player.transform;

            var weight = playerInventory.Find("Weight");

            Transform obj = Instantiate(__instance.m_takeAllButton.transform, weight.parent);
            obj.localPosition = weight.localPosition + new Vector3(0f, -56f, 0f);
            obj.name = name;

            ((Component)obj).transform.SetAsFirstSibling();

            int size = 38;

            var rect = (RectTransform)((Component)obj).transform;
            rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, size);
            rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size);

            RectTransform rect2 = (RectTransform)((Component)rect).transform.Find("Text");
            rect2.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, size - 8);
            rect2.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size - 4);

            Text text = ((Component)rect2).GetComponent<Text>();
            text.text = buttonText;
            text.resizeTextForBestFit = (true);

            return rect.GetComponent<Button>();
        }

        private T BindParameter<T>(T param, string key, string description)
        {
            return QuickStackStorePlugin.configFile.Bind<T>(configKey, key, param, description).Value;
        }

        public static UserConfig GetPlayerConfig(long playerID)
        {
            if (QuickStackStorePlugin.playerConfigs.TryGetValue(playerID, out UserConfig userConfig))
            {
                return userConfig;
            }

            userConfig = new UserConfig(playerID);
            QuickStackStorePlugin.playerConfigs[playerID] = userConfig;
            return userConfig;
        }

        private void LoadConfig()
        {
            string value = this.BindParameter<string>(QuickStackStorePlugin.QuickStackKey.ToString(), "QuickStackKey", "Get key codes here: https://docs.unity3d.com/ScriptReference/KeyCode.html");

            try
            {
                QuickStackStorePlugin.QuickStackKey = (KeyCode)Enum.Parse(typeof(KeyCode), value, true);
            }
            catch (Exception)
            {
                base.Logger.LogError("Failed to parse QuickStackKey, using default");
            }

            QuickStackStorePlugin.NearbyRange = this.BindParameter<float>(QuickStackStorePlugin.NearbyRange, nameof(QuickStackStorePlugin.NearbyRange), "How far from you is nearby, greater value = greater range.");
            QuickStackStorePlugin.StackToCurrentContainer = this.BindParameter<bool>(QuickStackStorePlugin.StackToCurrentContainer, nameof(QuickStackStorePlugin.StackToCurrentContainer), "Whether to ignore the container that you are currently using or not.");
            QuickStackStorePlugin.StackOnlyToCurrentContainer = this.BindParameter<bool>(QuickStackStorePlugin.StackOnlyToCurrentContainer, nameof(QuickStackStorePlugin.StackOnlyToCurrentContainer), "Whether to only stack to the currently open container (like QuickStack did), or also look at all nearby containers afterwards.");
            QuickStackStorePlugin.IgnoreConsumable = this.BindParameter<bool>(QuickStackStorePlugin.IgnoreConsumable, nameof(QuickStackStorePlugin.IgnoreConsumable), "Whether to completely exclude consumables from quick stacking (food, potions).");
            QuickStackStorePlugin.IgnoreAmmo = this.BindParameter<bool>(QuickStackStorePlugin.IgnoreAmmo, nameof(QuickStackStorePlugin.IgnoreAmmo), "Whether to completely exclude ammo from quick stacking (arrows).");
            QuickStackStorePlugin.CoalesceTrophies = this.BindParameter<bool>(QuickStackStorePlugin.CoalesceTrophies, nameof(QuickStackStorePlugin.CoalesceTrophies), "Whether to put all types of trophies in the container if any trophy is found in that container.");
            QuickStackStorePlugin.DisplayUIButtons = this.BindParameter<bool>(QuickStackStorePlugin.DisplayUIButtons, nameof(QuickStackStorePlugin.DisplayUIButtons), "Whether to display the three new buttons or not. Hotkeys work independently.");
        }

        private void OnSettingChanged(object sender, SettingChangedEventArgs e)
        {
            LoadConfig();

            if (InventoryGui.instance?.m_takeAllButton?.transform != null)
            {
                InventoryGui.instance.m_takeAllButton.transform.localPosition = origButtonPosition;
            }

            Destroy(depositAllButton.gameObject);
            Destroy(quickStackAreaButton.gameObject);
            Destroy(quickStackToChestButton.gameObject);
        }

        public static List<Container> FindContainersInRange(Vector3 point, float range)
        {
            List<Container> list = new List<Container>();

            foreach (Container container in QuickStackStorePlugin.AllContainers)
            {
                if (container?.transform != null && Vector3.Distance(point, container.transform.position) < range)
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

        // deterministic order of storing

        internal static int StoreItems(Player player)
        {
            Inventory fromInventory = player.GetInventory();
            Inventory toInventory = Extensions.GetCurrentContainer(InventoryGui.instance).GetInventory();

            UserConfig playerConfig = QuickStackStorePlugin.GetPlayerConfig(player.GetPlayerID());
            List<ItemDrop.ItemData> list = new List<ItemDrop.ItemData>(fromInventory.GetAllItems());
            int num = 0;

            foreach (ItemDrop.ItemData itemData in list)
            {
                // TODO unequip equipped items
                if (!itemData.m_equiped && !playerConfig.IsSlotMarked(itemData.m_gridPos) && !playerConfig.IsItemMarked(itemData.m_shared))
                {
                    if (toInventory.AddItem(itemData))
                    {
                        Debug.Log($"Storing {itemData.m_shared.m_name} in container");
                        fromInventory.RemoveItem(itemData);
                        num++;
                    }
                }
            }

            toInventory.Changed();
            fromInventory.Changed();

            return num;
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
                    (!QuickStackStorePlugin.IgnoreAmmo || itemData.m_shared.m_itemType != ItemDrop.ItemData.ItemType.Ammo) &&
                    (!QuickStackStorePlugin.IgnoreConsumable || itemData.m_shared.m_itemType != ItemDrop.ItemData.ItemType.Consumable) &&
                    !playerConfig.IsSlotMarked(itemData.m_gridPos) && !playerConfig.IsItemMarked(itemData.m_shared))
                {
                    if (itemData.m_shared.m_itemType == ItemDrop.ItemData.ItemType.Trophie && QuickStackStorePlugin.CoalesceTrophies && list2.Count > 0)
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
                            Debug.Log($"Storing {itemData.m_shared.m_name} in container");
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
                    Debug.Log($"Storing {itemData2.m_shared.m_name} in container");
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
                    ZNetView nview = container.GetNView();
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
            if (player.IsTeleporting())
            {
                return;
            }

            int movedCount = 0;
            Container container = Extensions.GetCurrentContainer(InventoryGui.instance);

            if ((StackToCurrentContainer || StackToCurrentContainerOverride) && container != null)
            {
                movedCount = StackItems(player, player.GetInventory(), container.GetInventory());

                if (StackOnlyToCurrentContainer || StackOnlyToCurrentContainerOverride)
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
        private Button quickStackAreaButton;
        private Button depositAllButton;
        private Button quickStackToChestButton;
        public const string configKey = "QuickStackStore";
        public static List<Container> AllContainers = new List<Container>();
        private readonly string ConfigPath = Path.Combine(Paths.ConfigPath, $"{configKey}.cfg");
        private static ConfigFile configFile = null;
        public static bool IgnoreAmmo = false;
        public static bool IgnoreConsumable = false;
        public static bool StackToCurrentContainer = true;
        public static bool StackOnlyToCurrentContainer = false;
        public static bool DisplayUIButtons = true;
        internal static float NearbyRange = 10f;
        public static KeyCode QuickStackKey = KeyCode.P;
        public static bool CoalesceTrophies = false;
        private static readonly Dictionary<long, UserConfig> playerConfigs = new Dictionary<long, UserConfig>();
    }
}