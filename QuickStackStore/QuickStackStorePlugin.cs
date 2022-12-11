using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace QuickStackStore
{
    [BepInPlugin("org.bepinex.plugins.valheim.quick_stack_store", "Quick Stack and Store", VERSION)]
    public class QuickStackStorePlugin : BaseUnityPlugin
    {
        private const string VERSION = "0.0.5";

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

            Config.SettingChanged += OnSettingChanged;

            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), null);
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
        }

        private void OnSettingChanged(object sender, SettingChangedEventArgs e)
        {
            LoadConfig();
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
                    !playerConfig.IsMarked(itemData.m_gridPos) && !playerConfig.IsMarked(itemData.m_shared))
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

        public static void DoQuickStack(Player player)
        {
            if (player.IsTeleporting())
            {
                return;
            }

            int movedCount = 0;
            Container container = Extensions.GetCurrentContainer(InventoryGui.instance);

            if (StackToCurrentContainer && container != null)
            {
                movedCount = StackItems(player, player.GetInventory(), container.GetInventory());

                if (StackOnlyToCurrentContainer)
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

        public const string configKey = "QuickStackStore";
        public static List<Container> AllContainers = new List<Container>();
        private readonly string ConfigPath = Path.Combine(Paths.ConfigPath, $"{configKey}.cfg");
        private static ConfigFile configFile = null;
        public static bool IgnoreAmmo = false;
        public static bool IgnoreConsumable = false;
        public static bool StackToCurrentContainer = true;
        public static bool StackOnlyToCurrentContainer = false;
        internal static float NearbyRange = 10f;
        public static KeyCode QuickStackKey = KeyCode.P;
        public static bool CoalesceTrophies = false;
        private static readonly Dictionary<long, UserConfig> playerConfigs = new Dictionary<long, UserConfig>();
    }
}