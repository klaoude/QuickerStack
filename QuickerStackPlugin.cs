using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;
using System.Threading;
using static ItemDrop;

namespace QuickerStack
{
    [BepInPlugin("org.bepinex.plugins.valheim.quicker_stack", "Quicker Stack", VERSION)]
    public class QuickerStackPlugin : BaseUnityPlugin
    {
        private const string VERSION = "0.0.2";

        public void Awake()
        {
            Logger.LogInfo(String.Format("Initializing Quicker Stack {0}.", VERSION));

            if (File.Exists(this.ConfigPath))
            {
                QuickerStackPlugin.configFile = new ConfigFile(this.ConfigPath, true);
                this.LoadConfig();
            }
            else
            {
                QuickerStackPlugin.configFile = base.Config;
                this.LoadConfig();
            }

            Config.SettingChanged += OnSettingChanged;

            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), null);
        }

        private T BindParameter<T>(T param, string key, string description)
        {
            return QuickerStackPlugin.configFile.Bind<T>("QuickerStack", key, param, description).Value;
        }

        public static UserConfig GetPlayerConfig(long playerID)
        {
            UserConfig userConfig = null;
            if (QuickerStackPlugin.playerConfigs.TryGetValue(playerID, out userConfig))
            {
                return userConfig;
            }
            userConfig = new UserConfig(playerID);
            QuickerStackPlugin.playerConfigs[playerID] = userConfig;
            return userConfig;
        }

        private void LoadConfig()
        {
            string value = this.BindParameter<string>(QuickerStackPlugin.QuickStackKey.ToString(), "QuickStackKey", "Get key codes here: https://docs.unity3d.com/ScriptReference/KeyCode.html");
            try
            {
                QuickerStackPlugin.QuickStackKey = (KeyCode)Enum.Parse(typeof(KeyCode), value, true);
            }
            catch (Exception)
            {
                base.Logger.LogError("Failed to parse QuickStackKey, using default");
            }
            QuickerStackPlugin.NearbyRange = this.BindParameter<float>(QuickerStackPlugin.NearbyRange, "NearbyRange", "How far from you is nearby, greater value = greater range");
            QuickerStackPlugin.IgnoreConsumable = this.BindParameter<bool>(QuickerStackPlugin.IgnoreConsumable, "IgnoreConsumable", "Whether to completely exclude consumables from quick stacking (food, potions).");
            QuickerStackPlugin.IgnoreAmmo = this.BindParameter<bool>(QuickerStackPlugin.IgnoreAmmo, "IgnoreAmmo", "Whether to completely exclude ammo from quick stacking (arrows)");
            QuickerStackPlugin.CoalesceTrophies = this.BindParameter<bool>(QuickerStackPlugin.CoalesceTrophies, "CoalesceTrophies", "Whether to put all types of trophies in the container if any trophy is found in that container."); 
        }

        private void OnSettingChanged(object sender, SettingChangedEventArgs e)
        {
            LoadConfig();
        }

        public static List<Container> FindContainersInRange(Vector3 point, float range)
        {
            List<Container> list = new List<Container>();
            foreach (Container container in QuickerStackPlugin.AllContainers)
            {
                if (!(container == null) && !(container.transform == null) && Vector3.Distance(point, container.transform.position) < range)
                {
                    list.Add(container);
                }
            }
            return list;
        }

        public static List<Container> FindNearbyContainers(Vector3 point)
        {
            return QuickerStackPlugin.FindContainersInRange(point, QuickerStackPlugin.NearbyRange);
        }

        internal static int StackItems(Player player, Inventory fromInventory, Inventory toInventory)
        {
            UserConfig playerConfig = QuickerStackPlugin.GetPlayerConfig(player.GetPlayerID());
            List<ItemDrop.ItemData> list = new List<ItemDrop.ItemData>(fromInventory.GetAllItems());
            List<ItemDrop.ItemData> list2 = new List<ItemDrop.ItemData>();
            List<ItemDrop.ItemData> list3 = new List<ItemDrop.ItemData>();
            toInventory.GetAllItems(ItemDrop.ItemData.ItemType.Trophie, list2);
            bool flag = QuickerStackPlugin.CoalesceTrophies && list2.Count != 0;
            int num = 0;
            foreach (ItemDrop.ItemData itemData in list)
            {
                if (!itemData.IsEquipable() && 
                    itemData.m_shared.m_maxStackSize != 1 && 
                    (!QuickerStackPlugin.IgnoreAmmo || itemData.m_shared.m_itemType != ItemDrop.ItemData.ItemType.Ammo) && 
                    (!QuickerStackPlugin.IgnoreConsumable || itemData.m_shared.m_itemType != ItemDrop.ItemData.ItemType.Consumable) && 
                    !playerConfig.IsMarked(itemData.m_gridPos) && 
                    !playerConfig.IsMarked(itemData.m_shared))
                {
                    if (itemData.m_shared.m_itemType == ItemDrop.ItemData.ItemType.Trophie && flag)
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
                            player.Message(MessageHud.MessageType.Center, String.Format("Storing {0} in container", itemData.m_shared.m_name));
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
                    player.Message(MessageHud.MessageType.Center, String.Format("Storing {0} in container", itemData2.m_shared.m_name));
                    num++;
                    fromInventory.RemoveItem(itemData2);
                }
            }
            toInventory.Changed();
            fromInventory.Changed();
            return num;
        }

        public static void reportResult(Player player, int movedCount)
        {
            player.Message(
                MessageHud.MessageType.Center,
                (movedCount > 0) ? string.Format("Stacked {0} item{1}", movedCount, (movedCount % 10 == 1) ? "" : "s") : "Nothing to stack",
                0,
                null
           );
        }

        private static void StackToMany(Player player, List<Container> containers)
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();
            Inventory inventory = player.GetInventory();
            int num = 0;
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
                    num += QuickerStackPlugin.StackItems(player, inventory, container.GetInventory());
                    container.SetInUse(false);
                }
            }
            QuickerStackPlugin.reportResult(player, num);
            watch.Stop();
            var elapsedMs = watch.ElapsedMilliseconds;
            Debug.Log(String.Format("DoQuickStack() in {0}ms.", elapsedMs));
        }

        public static void DoQuickStack(Player player)
        {
            Debug.Log("DoQuickStack()");
            if (player.IsTeleporting())
                return;

            var watch = System.Diagnostics.Stopwatch.StartNew();
            List<Container> list = QuickerStackPlugin.FindNearbyContainers(player.transform.position);
            watch.Stop();
            var elapsedMs = watch.ElapsedMilliseconds;
            Debug.Log(String.Format("Found nearby containers in {0}ms.", elapsedMs));
            if (list.Count != 0)
            {
                player.Message(MessageHud.MessageType.Center, String.Format("found {0} containers in range", list.Count));
                Thread stackThread = new Thread(() => QuickerStackPlugin.StackToMany(player, list));
                stackThread.Start();
            }
        }

        public static List<Container> AllContainers = new List<Container>();
        private readonly string ConfigPath = Path.Combine(Paths.ConfigPath, "QuickerStack.cfg");
        private static ConfigFile configFile = null;
        public event EventHandler<SettingChangedEventArgs> SettingChanged;
        public static bool IgnoreAmmo = true;
        public static bool IgnoreConsumable = true;
        internal static float NearbyRange = 15f;
        public static KeyCode QuickStackKey = KeyCode.P;
        public static bool CoalesceTrophies = true;
        private static Dictionary<long, UserConfig> playerConfigs = new Dictionary<long, UserConfig>();
    }
}
