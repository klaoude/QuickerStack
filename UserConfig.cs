using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using BepInEx;
using UnityEngine;

namespace QuickerStack
{
    public class UserConfig
    {
        // Token: 0x06000001 RID: 1 RVA: 0x00002050 File Offset: 0x00000250
        public UserConfig(long uid)
        {
            this._configPath = Path.Combine(Paths.ConfigPath, string.Format("QuickerStack_player_{0}.dat", uid));
            this._uid = uid;
            this.Load();
        }

        // Token: 0x06000002 RID: 2 RVA: 0x00002090 File Offset: 0x00000290
        private void Save()
        {
            using (Stream stream = File.Open(this._configPath, FileMode.Create))
            {
                UserConfig._bf.Serialize(stream, this._markedItems);
                UserConfig._bf.Serialize(stream, this._markedCategories);
            }
        }

        // Token: 0x06000003 RID: 3 RVA: 0x000020E8 File Offset: 0x000002E8
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

        // Token: 0x06000004 RID: 4 RVA: 0x0000211C File Offset: 0x0000031C
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

        // Token: 0x06000005 RID: 5 RVA: 0x00002154 File Offset: 0x00000354
        private void Load()
        {
            using (Stream stream = File.Open(this._configPath, FileMode.OpenOrCreate))
            {
                stream.Seek(0L, SeekOrigin.Begin);
                UserConfig.LoadProperty<List<Tuple<int, int>>>(stream, out this._markedItems);
                UserConfig.LoadProperty<List<string>>(stream, out this._markedCategories);
            }
        }

        // Token: 0x06000006 RID: 6 RVA: 0x000021AC File Offset: 0x000003AC
        public bool Toggle(Vector2i position)
        {
            Tuple<int, int> item = new Tuple<int, int>(position.x, position.y);
            bool result = this._markedItems.XAdd(item);
            this.Save();
            return result;
        }

        // Token: 0x06000007 RID: 7 RVA: 0x000021E0 File Offset: 0x000003E0
        public bool IsMarked(Vector2i position)
        {
            Tuple<int, int> item = new Tuple<int, int>(position.x, position.y);
            return this._markedItems.Contains(item);
        }

        // Token: 0x06000008 RID: 8 RVA: 0x0000220B File Offset: 0x0000040B
        public bool Toggle(ItemDrop.ItemData.SharedData item)
        {
            bool result = this._markedCategories.XAdd(item.m_name);
            this.Save();
            return result;
        }

        // Token: 0x06000009 RID: 9 RVA: 0x00002224 File Offset: 0x00000424
        public bool IsMarked(ItemDrop.ItemData.SharedData item)
        {
            return this._markedCategories.Contains(item.m_name);
        }

        // Token: 0x04000001 RID: 1
        private string _configPath = string.Empty;

        // Token: 0x04000002 RID: 2
        private List<Tuple<int, int>> _markedItems;

        // Token: 0x04000003 RID: 3
        private List<string> _markedCategories;

        // Token: 0x04000004 RID: 4
        private long _uid;

        // Token: 0x04000005 RID: 5
        private static BinaryFormatter _bf = new BinaryFormatter();
    }
}
