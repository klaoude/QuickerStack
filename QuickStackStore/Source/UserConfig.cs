using BepInEx;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace QuickStackStore
{
    public class UserConfig
    {
        public UserConfig(long uid)
        {
            this._configPath = Path.Combine(Paths.ConfigPath, string.Format($"{QuickStackStorePlugin.configKey}_player_{0}.dat", uid));
            this._uid = uid;
            this.Load();
        }

        private void Save()
        {
            using (Stream stream = File.Open(this._configPath, FileMode.Create))
            {
                UserConfig._bf.Serialize(stream, this._markedItems);
                UserConfig._bf.Serialize(stream, this._markedCategories);
            }
        }

        private static object TryDeserialize(Stream stream)
        {
            object result;

            try
            {
                result = UserConfig._bf.Deserialize(stream);
            }
            catch (SerializationException)
            {
                result = null;
            }

            return result;
        }

        private static void LoadProperty<T>(Stream file, out T property) where T : new()
        {
            object obj = UserConfig.TryDeserialize(file);

            if (obj is T)
            {
                T t = (T)((object)obj);
                property = t;
                return;
            }

            property = Activator.CreateInstance<T>();
        }

        private void Load()
        {
            using (Stream stream = File.Open(this._configPath, FileMode.OpenOrCreate))
            {
                stream.Seek(0L, SeekOrigin.Begin);
                UserConfig.LoadProperty<List<Tuple<int, int>>>(stream, out this._markedItems);
                UserConfig.LoadProperty<List<string>>(stream, out this._markedCategories);
            }
        }

        public bool Toggle(Vector2i position)
        {
            Tuple<int, int> item = new Tuple<int, int>(position.x, position.y);
            bool result = this._markedItems.XAdd(item);
            this.Save();

            return result;
        }

        public bool IsSlotMarked(Vector2i position)
        {
            Tuple<int, int> item = new Tuple<int, int>(position.x, position.y);

            return this._markedItems.Contains(item);
        }

        public bool Toggle(ItemDrop.ItemData.SharedData item)
        {
            bool result = this._markedCategories.XAdd(item.m_name);
            this.Save();

            return result;
        }

        public bool IsItemMarked(ItemDrop.ItemData.SharedData item)
        {
            return this._markedCategories.Contains(item.m_name);
        }

        private string _configPath = string.Empty;
        private List<Tuple<int, int>> _markedItems;
        private List<string> _markedCategories;
        private long _uid;
        private static BinaryFormatter _bf = new BinaryFormatter();
    }
}