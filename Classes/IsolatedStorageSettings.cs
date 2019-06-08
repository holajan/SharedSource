using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace System.IO.IsolatedStorage
{
    /// <summary>Provides a <see cref="T:System.Collections.Generic.Dictionary`2" /> that stores key-value pairs in isolated storage. </summary>
    /// <exception cref="T:System.ArgumentNullException">This exception is thrown when you attempt to reference an instance of the class by using an indexer and the variable you pass in for the key value is null.</exception>
    internal sealed class IsolatedStorageSettings : IDictionary<string, object>, ICollection<KeyValuePair<string, object>>, IEnumerable<KeyValuePair<string, object>>, IDictionary, ICollection, IEnumerable
    {
        #region member varible and default property initialization
        private IsolatedStorageFile _appStore;
        private Dictionary<string, object> _settings;
        private static IsolatedStorageSettings s_appSettings;
        #endregion

        #region constructors and destructors
        private IsolatedStorageSettings()
        {
            var assembly = System.Reflection.Assembly.GetEntryAssembly();
            if (assembly == null)
            {
                throw new IsolatedStorageException("Enable to determine application entry assembly.");
            }
            this._appStore = IsolatedStorageFile.GetStore(IsolatedStorageScope.Machine | IsolatedStorageScope.Assembly, null, null, assembly.Evidence, null);
            this.Reload();
        }

        /// <summary>
        /// IsolatedStorageSettings finalizer
        /// </summary>
        ~IsolatedStorageSettings()
        {
            if (this._appStore != null)
            {
                try
                {
                    this.Save();
                }
                catch (Exception)
                {
                }
                this._appStore.Dispose();
            }
        }
        #endregion

        #region action methods
        /// <summary>Gets a value for the specified key.</summary>
        /// <returns>true if the specified key is found; otherwise, false.</returns>
        /// <param name="key">The key of the value to get.</param>
        /// <param name="value">When this method returns, the value associated with the specified key if the key is found; otherwise, the default value for the type of the <paramref name="value" /> parameter. This parameter is passed uninitialized.</param>
        /// <typeparam name="T">The <see cref="T:System.Type" /> of the <paramref name="value" /> parameter.</typeparam>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="key" /> is null.</exception>
        public bool TryGetValue<T>(string key, out T value)
        {
            object obj2;
            this.CheckNullKey(key);
            if (this._settings.TryGetValue(key, out obj2))
            {
                value = (T)obj2;
                return true;
            }
            value = default(T);
            return false;
        }

        /// <summary>Adds an entry to the dictionary for the key-value pair.</summary>
        /// <param name="key">The key for the entry to be stored.</param>
        /// <param name="value">The value to be stored.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="key" /> is null.</exception>
        /// <exception cref="T:System.ArgumentException">
        /// <paramref name="key" /> already exists in the dictionary.</exception>
        public void Add(string key, object value)
        {
            this.CheckNullKey(key);
            this._settings.Add(key, value);
        }

        /// <summary>Resets the count of items stored in <see cref="T:System.IO.IsolatedStorage.IsolatedStorageSettings" /> to zero and releases all references to elements in the collection.</summary>
        public void Clear()
        {
            this._settings.Clear();
            this.Save();
        }

        /// <summary>Determines if the application settings dictionary contains the specified key.</summary>
        /// <returns>true if the dictionary contains the specified key; otherwise, false.</returns>
        /// <param name="key">The key for the entry to be located.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="key" /> is null.</exception>
        public bool Contains(string key)
        {
            this.CheckNullKey(key);
            return this._settings.ContainsKey(key);
        }

        /// <summary>Removes the entry with the specified key.</summary>
        /// <returns>true if the specified key was removed; otherwise, false.</returns>
        /// <param name="key">The key for the entry to be deleted.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="key" /> is null.</exception>
        public bool Remove(string key)
        {
            this.CheckNullKey(key);
            return this._settings.Remove(key);
        }

        /// <summary>Saves data written to the current <see cref="T:System.IO.IsolatedStorage.IsolatedStorageSettings" /> object.</summary>
        /// <exception cref="T:System.IO.IsolatedStorage.IsolatedStorageException">The <see cref="T:System.IO.IsolatedStorage.IsolatedStorageFile" /> does not have enough available free space.</exception>
        public void Save()
        {
            using (IsolatedStorageFileStream stream = this._appStore.OpenFile("__LocalSettings", FileMode.OpenOrCreate))
            {
                using (MemoryStream stream2 = new MemoryStream())
                {
                    Dictionary<Type, bool> dictionary = new Dictionary<Type, bool>();
                    StringBuilder builder = new StringBuilder();
                    foreach (object obj2 in this._settings.Values)
                    {
                        if (obj2 != null)
                        {
                            Type type = obj2.GetType();
                            if (!type.IsPrimitive && (type != typeof(string)))
                            {
                                dictionary[type] = true;
                                if (builder.Length > 0)
                                {
                                    builder.Append('\0');
                                }
                                builder.Append(type.AssemblyQualifiedName);
                            }
                        }
                    }
                    builder.Append(Environment.NewLine);
                    byte[] bytes = Encoding.UTF8.GetBytes(builder.ToString());
                    stream2.Write(bytes, 0, bytes.Length);
                    new DataContractSerializer(typeof(Dictionary<string, object>), dictionary.Keys).WriteObject(stream2, this._settings);
                    if (stream2.Length > (this._appStore.AvailableFreeSpace + stream.Length))
                    {
                        throw new IsolatedStorageException("Not enough space in isolated storage.");
                    }
                    stream.SetLength(0L);
                    byte[] buffer = stream2.ToArray();
                    stream.Write(buffer, 0, buffer.Length);
                }
            }
        }

        void ICollection<KeyValuePair<string, object>>.Add(KeyValuePair<string, object> item)
        {
            ((ICollection<KeyValuePair<string, object>>)this._settings).Add(item);
        }

        void ICollection<KeyValuePair<string, object>>.Clear()
        {
            this._settings.Clear();
        }

        bool ICollection<KeyValuePair<string, object>>.Contains(KeyValuePair<string, object> item)
        {
            return ((ICollection<KeyValuePair<string, object>>)this._settings).Contains(item);
        }

        void ICollection<KeyValuePair<string, object>>.CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
        {
            ((ICollection<KeyValuePair<string, object>>)this._settings).CopyTo(array, arrayIndex);
        }

        bool ICollection<KeyValuePair<string, object>>.Remove(KeyValuePair<string, object> item)
        {
            return ((ICollection<KeyValuePair<string, object>>)this._settings).Remove(item);
        }

        bool IDictionary<string, object>.ContainsKey(string key)
        {
            this.CheckNullKey(key);
            return this._settings.ContainsKey(key);
        }

        bool IDictionary<string, object>.TryGetValue(string key, out object value)
        {
            this.CheckNullKey(key);
            return this._settings.TryGetValue(key, out value);
        }

        /// <summary>For a description of this member, see <see cref="M:System.Collections.ICollection.CopyTo(System.Array,System.Int32)" />.</summary>
        /// <param name="array">The one-dimensional <see cref="T:System.Array" /> that is the destination of the elements copied from <see cref="T:System.Collections.ICollection" />. The array must have zero-based indexing.</param>
        /// <param name="index">The zero-based index in <paramref name="array" /> at which copying begins.</param>
        void ICollection.CopyTo(Array array, int index)
        {
            ((IDictionary)this._settings).CopyTo(array, index);
        }

        /// <summary>For a description of this member, see <see cref="M:System.Collections.IDictionary.Add(System.Object,System.Object)" />.</summary>
        /// <param name="key">The object to use as the key of the element to add.</param>
        /// <param name="value">The object to use as the value of the element to add.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="key" /> is null.</exception>
        void IDictionary.Add(object key, object value)
        {
            this.CheckNullKey(key);
            ((IDictionary)this._settings).Add(key, value);
        }

        /// <summary>For a description of this member, see <see cref="M:System.Collections.IDictionary.Clear" />.</summary>
        void IDictionary.Clear()
        {
            this._settings.Clear();
        }

        /// <summary>For a description of this member, see <see cref="M:System.Collections.IDictionary.Contains(System.Object)" />.</summary>
        /// <returns>true if the <see cref="T:System.Collections.IDictionary" /> contains an element with the specified <paramref name="key" />; otherwise, false. </returns>
        /// <param name="key">The key to locate in the <see cref="T:System.Collections.IDictionary" />.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="key" /> is null.</exception>
        bool IDictionary.Contains(object key)
        {
            this.CheckNullKey(key);
            return ((IDictionary)this._settings).Contains(key);
        }

        /// <summary>For a description of this member, see <see cref="M:System.Collections.IDictionary.Remove(System.Object)" />.</summary>
        /// <param name="key">The key for the entry to be deleted.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="key" /> is null.</exception>
        void IDictionary.Remove(object key)
        {
            this.CheckNullKey(key);
            ((IDictionary)this._settings).Remove(key);
        }

        /// <summary>For a description of this member, see <see cref="M:System.Collections.IEnumerable.GetEnumerator" />.</summary>
        /// <returns>An <see cref="T:System.Collections.Generic.IEnumerator`1" /> object that can be used to iterate through the collection.</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this._settings.GetEnumerator();
        }

        IEnumerator<KeyValuePair<string, object>> IEnumerable<KeyValuePair<string, object>>.GetEnumerator()
        {
            return this._settings.GetEnumerator();
        }

        /// <summary>For a description of this member, see <see cref="M:System.Collections.IDictionary.GetEnumerator" />.</summary>
        /// <returns>An <see cref="T:System.Collections.Generic.IEnumerator`1" /> object that can be used to iterate through the collection.</returns>
        IDictionaryEnumerator IDictionary.GetEnumerator()
        {
            return this._settings.GetEnumerator();
        }
        #endregion

        #region property getters/setters
        /// <summary>Gets an instance of <see cref="T:System.IO.IsolatedStorage.IsolatedStorageSettings" /> that contains the contents of the application's <see cref="T:System.IO.IsolatedStorage.IsolatedStorageFile" />, scoped at the application level, or creates a new instance of <see cref="T:System.IO.IsolatedStorage.IsolatedStorageSettings" /> if one does not exist.</summary>
        /// <returns>An <see cref="T:System.IO.IsolatedStorage.IsolatedStorageSettings" /> object that contains the contents of the application's <see cref="T:System.IO.IsolatedStorage.IsolatedStorageFile" />, scoped at the application level. If an instance does not already exist, a new instance is created.</returns>
        public static IsolatedStorageSettings ApplicationSettings
        {
            get
            {
                if (s_appSettings == null)
                {
                    s_appSettings = new IsolatedStorageSettings();
                }
                return s_appSettings;
            }
        }

        /// <summary>Gets the number of key-value pairs that are stored in the dictionary.</summary>
        /// <returns>The number of key-value pairs that are stored in the dictionary.</returns>
        public int Count
        {
            get { return this._settings.Count; }
        }

        /// <summary>Gets or sets the value associated with the specified key.</summary>
        /// <returns>The value associated with the specified key. If the specified key is not found, a get operation throws a <see cref="T:System.Collections.Generic.KeyNotFoundException" />, and a set operation creates a new element that has the specified key.</returns>
        /// <param name="key">The key of the item to get or set.</param>
        public object this[string key]
        {
            get
            {
                this.CheckNullKey(key);
                return this._settings[key];
            }
            set
            {
                this.CheckNullKey(key);
                this._settings[key] = value;
            }
        }

        /// <summary>Gets a collection that contains the keys in the dictionary.</summary>
        /// <returns>A <see cref="T:System.Collections.Generic.Dictionary`2.KeyCollection" /> that contains the keys in the <see cref="T:System.Collections.Generic.Dictionary`2" />.</returns>
        public ICollection Keys
        {
            get { return this._settings.Keys; }
        }

        bool ICollection<KeyValuePair<string, object>>.IsReadOnly
        {
            get { return false; }
        }

        ICollection<string> IDictionary<string, object>.Keys
        {
            get { return this._settings.Keys; }
        }

        ICollection<object> IDictionary<string, object>.Values
        {
            get { return this._settings.Values; }
        }

        /// <summary>For a description of this member, see <see cref="P:System.Collections.ICollection.Count" />.</summary>
        /// <returns>The number of elements that are contained in the <see cref="T:System.Collections.ICollection" />.</returns>
        int ICollection.Count
        {
            get { return this._settings.Count; }
        }

        /// <summary>For a description of this member, see <see cref="P:System.Collections.ICollection.IsSynchronized" />.</summary>
        /// <returns>true if access to the <see cref="T:System.Collections.ICollection" /> is synchronized (thread safe); otherwise, false. In the default implementation of <see cref="T:System.Collections.Generic.Dictionary`2" />, this property always returns false.</returns>
        bool ICollection.IsSynchronized
        {
            get
            {
                return ((ICollection)this._settings).IsSynchronized;
            }
        }

        /// <summary>For a description of this member, see <see cref="P:System.Collections.ICollection.SyncRoot" />.</summary>
        /// <returns>An object that can be used to synchronize access to the <see cref="T:System.Collections.ICollection" />.</returns>
        object ICollection.SyncRoot
        {
            get
            {
                return ((ICollection)this._settings).SyncRoot;
            }
        }

        /// <summary>For a description of this member, see <see cref="P:System.Collections.IDictionary.IsFixedSize" />.</summary>
        /// <returns>true if the <see cref="T:System.Collections.IDictionary" /> has a fixed size; otherwise, false. In the default implementation of <see cref="T:System.Collections.IDictionary" />, this property always returns false.</returns>
        bool IDictionary.IsFixedSize
        {
            get { return false; }
        }

        /// <summary>For a description of this member, see <see cref="P:System.Collections.IDictionary.IsReadOnly" />.</summary>
        /// <returns>true if the <see cref="T:System.Collections.IDictionary" /> is read-only; otherwise, false. In the default implementation of <see cref="T:System.Collections.IDictionary" />, this property always returns false.</returns>
        bool IDictionary.IsReadOnly
        {
            get { return false; }
        }

        /// <summary>For a description of this member, see <see cref="P:System.Collections.IDictionary.Item(System.Object)" />.</summary>
        /// <returns>The value associated with the specified <paramref name="key" />.</returns>
        /// <param name="key">The key of the value to get or set.</param>
        object IDictionary.this[object key]
        {
            get
            {
                this.CheckNullKey(key);
                return ((IDictionary)this._settings)[key];
            }
            set
            {
                this.CheckNullKey(key);
                ((IDictionary)this._settings)[key] = value;
            }
        }

        /// <summary>Gets a collection that contains the values in the dictionary.</summary>
        /// <returns>A <see cref="T:System.Collections.Generic.Dictionary`2.ValueCollection" /> that contains the values in the <see cref="T:System.Collections.Generic.Dictionary`2" />.</returns>
        public ICollection Values
        {
            get { return this._settings.Values; }
        }
        #endregion

        #region private member functions
        private void CheckNullKey(object key)
        {
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }
        }

        private void Reload()
        {
            using (IsolatedStorageFileStream stream = this._appStore.OpenFile("__LocalSettings", FileMode.OpenOrCreate))
            {
                using (StreamReader reader = new StreamReader(stream))
                {
                    if (stream.Length > 0L)
                    {
                        try
                        {
                            List<Type> knownTypes = new List<Type>();
                            string str = reader.ReadLine();
                            foreach (string str2 in str.Split(new char[1]))
                            {
                                Type item = Type.GetType(str2, false);
                                if (item != null)
                                {
                                    knownTypes.Add(item);
                                }
                            }
                            stream.Position = str.Length + Environment.NewLine.Length;
                            DataContractSerializer serializer = new DataContractSerializer(typeof(Dictionary<string, object>), knownTypes);
                            this._settings = (Dictionary<string, object>)serializer.ReadObject(stream);
                        }
                        catch
                        {
                            this._settings = new Dictionary<string, object>();
                        }
                    }
                    else
                    {
                        this._settings = new Dictionary<string, object>();
                    }
                }
            }
        }
        #endregion
    }
}