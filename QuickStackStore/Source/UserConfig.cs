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
            this._uid = uid;
            this._configPath = Path.Combine(Paths.ConfigPath, $"QuickStackStore_player_{this._uid}.dat");
            this.Load();
        }

        private void Save()
        {
            using (Stream stream = File.Open(this._configPath, FileMode.Create))
            {
                _bf.Serialize(stream, this.favoritedSlots);
                _bf.Serialize(stream, this.favoritedItems);
            }
        }

        private static object TryDeserialize(Stream stream)
        {
            object result;

            try
            {
                result = _bf.Deserialize(stream);
            }
            catch (SerializationException)
            {
                result = null;
            }

            return result;
        }

        private static void LoadProperty<T>(Stream file, out T property) where T : new()
        {
            object obj = TryDeserialize(file);

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
                LoadProperty<List<Tuple<int, int>>>(stream, out this.favoritedSlots);
                LoadProperty<List<string>>(stream, out this.favoritedItems);
            }
        }

        public bool Toggle(Vector2i position)
        {
            Tuple<int, int> item = new Tuple<int, int>(position.x, position.y);
            bool result = this.favoritedSlots.XAdd(item);
            this.Save();

            return result;
        }

        public bool IsSlotFavorited(Vector2i position)
        {
            Tuple<int, int> item = new Tuple<int, int>(position.x, position.y);

            return this.favoritedSlots.Contains(item);
        }

        public bool Toggle(ItemDrop.ItemData.SharedData item)
        {
            bool result = this.favoritedItems.XAdd(item.m_name);
            this.Save();

            return result;
        }

        public bool IsItemNameFavorited(ItemDrop.ItemData.SharedData item)
        {
            return this.favoritedItems.Contains(item.m_name);
        }

        public bool IsItemNameOrSlotFavorited(ItemDrop.ItemData item)
        {
            return IsItemNameFavorited(item.m_shared) || IsSlotFavorited(item.m_gridPos);
        }

        public Vector2i[] GetFavoritedSlotsCopy()
        {
            var ret = new Vector2i[favoritedSlots.Count];

            for (int i = 0; i < ret.Length; i++)
            {
                ret[i] = new Vector2i(favoritedSlots[i].Item1, favoritedSlots[i].Item2);
            }

            return ret;
        }

        private readonly string _configPath = string.Empty;
        private List<Tuple<int, int>> favoritedSlots;
        private List<string> favoritedItems;
        private readonly long _uid;
        private static readonly BinaryFormatter _bf = new BinaryFormatter();
    }
}