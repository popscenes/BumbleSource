using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.WindowsAzure.StorageClient;

namespace WebSite.Azure.Common.TableStorage
{
    public class ExtendableTableEntry : TableServiceEntity
        , StorageTableEntryInterface
    {

        public void SetProperty(string propertyName, object value, Type type = null)
        {
            string edmTyp = Edm.String;//default to string for null
            if (value != null || type != null)
                edmTyp = Edm.GetEdmTyp(type ?? value.GetType());   

            SetProperty(propertyName, value, edmTyp);
        }

        public void SetProperty(string propertyName, object value, string edmtype)
        {
            if (!Edm.IsEdmTyp(edmtype))
                throw new ArgumentException("ExtendableTableEntry Invalid Edm Type for SetProperty");

            var prop = new EdmProp()
            {
                EdmTyp = edmtype,
                Name = propertyName
            };

            if (_extendedProperties.ContainsKey(prop))
                _extendedProperties.Remove(prop);

            _extendedProperties.Add(prop, value);
        }

        public IEnumerable<KeyValuePair<EdmProp, object>> GetAllProperties()
        {
            return _extendedProperties.AsEnumerable();
        }

        public IEnumerable<KeyValuePair<EdmProp, object>> GetAllPropertiesWithPrefix(string keyPrefix)
        {
            return GetAllProperties().Where(kv => kv.Key.Name.StartsWith(keyPrefix)).AsEnumerable();
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

    }
}
