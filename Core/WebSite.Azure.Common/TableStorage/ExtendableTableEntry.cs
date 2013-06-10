using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.WindowsAzure.Storage.Table.DataServices;
using Website.Infrastructure.Util;

namespace Website.Azure.Common.TableStorage
{
    public class ExtendableTableEntry : TableServiceEntity
        , StorageTableEntryInterface
    {

        public bool SetProperty(string propertyName, object value, Type type = null, bool throwEx = true)
        {
            string edmTyp = Edm.String;//default to string for null
            if (value != null || type != null)
                edmTyp = Edm.GetEdmTyp(type ?? value.GetType());

            return SetProperty(propertyName, value, edmTyp, throwEx);
        }

        public bool SetProperty(string propertyName, object value, string edmtype, bool throwEx = true)
        {
            if (!Edm.IsEdmTyp(edmtype))
            {
                if (throwEx)
                    throw new ArgumentException("ExtendableTableEntry Invalid Edm Type for SetProperty");
                return false;
            }
                

            var prop = new EdmProp()
            {
                EdmTyp = edmtype,
                Name = propertyName
            };

            if (_extendedProperties.ContainsKey(prop))
                _extendedProperties.Remove(prop);

            _extendedProperties.Add(prop, value);
            return true;
        }

        public IEnumerable<KeyValuePair<EdmProp, object>> GetAllProperties()
        {
            return _extendedProperties.AsEnumerable();
        }

        public IEnumerable<KeyValuePair<EdmProp, object>> GetAllPropertiesWithPrefix(string keyPrefix)
        {
            return GetAllProperties().Where(kv => kv.Key.Name.StartsWith(keyPrefix)).AsEnumerable();
        }

        public Dictionary<string, object> GetAsStringDict()
        {
            return GetAllProperties().ToDictionary(kv => kv.Key.Name, kv => kv.Value);    
        }

        public static ExtendableTableEntry CreateFromDict(Dictionary<string, object> dictionary)
        {
            var ret = new ExtendableTableEntry();
            ret.AddDict(dictionary);
            return ret;
        }

        public void AddDict(Dictionary<string, object> dictionary)
        {
            foreach (var kv in dictionary.Where(kv => kv.Value != null))
            {
                SetProperty(kv.Key, kv.Value, (Type)null, false);
            }
        }
        public ExtendableTableEntry ExtractFromKeyPrefix(string keyPrefix)
        {
            var ret = new ExtendableTableEntry();

            foreach (var property in GetAllPropertiesWithPrefix(keyPrefix))
            {
                ret._extendedProperties.Add(new EdmProp()
                {
                    Name = property.Key.Name.Substring(keyPrefix.Length),
                    EdmTyp = property.Key.EdmTyp
                }, property.Value);
            }

            return ret;
        }

        public void MergeProperties(ExtendableTableEntry source
            , string keyPrefix = ""
            , bool replaceDuplicates = false)
        {
            foreach (var property in source.GetAllProperties())
            {
                var key = new EdmProp()
                              {
                                  Name = keyPrefix != null ? keyPrefix + property.Key.Name : property.Key.Name,
                                  EdmTyp = property.Key.EdmTyp
                              };

                if (_extendedProperties.ContainsKey(key))
                {
                    if (replaceDuplicates)
                        _extendedProperties.Remove(key);
                    else
                        continue;
                }

                _extendedProperties.Add(key, property.Value);
            }
        }

        public object this[string key]
        {
            get
            {
                object ret = null;
                this._extendedProperties.TryGetValue(new EdmProp() {Name = key}, out ret);
                return ret;
            }
            set { SetProperty(key, value); }
        }

        public RetType Get<RetType>(string key)
        {
            return (RetType) (this[key] ?? default(RetType));
        }

        public object this[string key, Type type]
        {
            set { SetProperty(key, value, type); }
        }

        public object this[string key, string edmtype]
        {
            set { SetProperty(key, value, edmtype); }
        }

        private readonly Dictionary<EdmProp, object> _extendedProperties = new Dictionary<EdmProp, object>();

        //implement properties from map
        public override string PartitionKey
        {
            get
            {
                return this["PartitionKey"] as string;
            }
            set
            {
                this["PartitionKey"] = value;
            }
        }

        public override string RowKey
        {
            get
            {
                return this["RowKey"] as string;
            }
            set
            {
                this["RowKey"] = value;
            }
        }

        public int PartitionClone
        {
            get
            {
                return (int)(this["PartitionClone"] ?? 0);
            }
            set
            {
                this["PartitionClone"] = value;
            }
        }
        public bool KeyChanged
        {
            get
            {
                return (bool)(this["KeyChanged"] ?? false);
            }
            set
            {
                this["KeyChanged"] = value;
            }
        }

        public void Init(object source)
        {
            var tableEntry = source as ExtendableTableEntry;
            if (tableEntry != null)
            {
                MergeProperties(tableEntry, "", true);
            }
            else
            {
                var dictionary = new Dictionary<string, object>();

                source.PropertiesToDictionary(dictionary);

                AddDict(dictionary);
            }
        }

        public void UpdateEntry()
        {
        }

        public object GetEntity(Type entityTyp = null)
        {
            var ret = entityTyp == null || entityTyp == this.GetType() 
                ? this 
                : Activator.CreateInstance(entityTyp, true);

            if (ret == this)
                return ret;

            var tableEntry = ret as ExtendableTableEntry;
            if (tableEntry != null)
            {
                tableEntry.MergeProperties(this, "", true);
            }
            else
            {
                var dictionary = GetAsStringDict();
                SerializeUtil.PropertiesFromDictionary(ret, dictionary);
            }
            return ret;
        } 
    }
}
