using System;
using System.Collections.Generic;
using System.Linq;
using WebSite.Infrastructure.Domain;

namespace WebSite.Azure.Common.TableStorage
{
    public class EdmSerializer
    {
        public Type InternalTyp { get; set; }
        public string EdmTyp { get; set; }
        public Func<KeyValuePair<EdmProp, object>, object> Deserializer { get; set; }
        public Func<object, KeyValuePair<EdmProp, object>> Serializer { get; set; }
    }


    public class DefaultPropertyGroupTableSerializer : PropertyGroupTableSerializerInterface
    {
        private readonly Dictionary<string, EdmSerializer> _customSerializer;
        private readonly Dictionary<Type, string> _customTypes;

        private const int GroupNameIndex = 0;
        private const int PropertyNameIndex = 1;
        private const int PropertyTypeIndex = 2;
        private const int VersionIndex = 3;
        private const string PropertyGroupFormat = "{0}_{1}_{2}_{3}";

        public DefaultPropertyGroupTableSerializer(IEnumerable<KeyValuePair<string, EdmSerializer>> customTypes)
        {
            _customTypes = new Dictionary<Type, string>();
            _customSerializer = new Dictionary<string, EdmSerializer>();
            foreach (var kv in customTypes)
            {
                _customTypes.Add(kv.Value.InternalTyp, kv.Key);
                _customSerializer[kv.Key] = kv.Value;            
            }
        }

        public void LoadProperties(PropertyGroupCollection propertyGroup, ExtendableTableEntry tableEntry)
        {
            foreach (var prop in tableEntry.GetAllProperties().Where(kv => kv.Key.Name.Contains("_")))
            {
                DeserializePropertyGroupVal(propertyGroup, prop);
            }
            
        }

        public void MergeProperties(ExtendableTableEntry tableEntry, PropertyGroupCollection propertyGroup)
        {
            foreach (var props in propertyGroup.Groups)
            {
                foreach (var prop in props.Properties)
                {
                    SerializePropertyGroupVal(props.Name, prop.Key, prop.Value, tableEntry);
                }
            }
        }

        private void SerializePropertyGroupVal(string groupName, string key, object value, ExtendableTableEntry tableEntry)
        {
            if(value == null)
                return;

            if(Edm.IsEdmTyp(value.GetType()))
            {
                var tableKey = GetPropertyNameString(groupName, key);
                tableEntry[tableKey] = value;
            }
            else
            {
                var typestring = _customTypes[value.GetType()];
                if(typestring == null)
                    throw new Exception(string.Format("Unable to serialize type {0} for property group" +
                                                      ", please define a serializer", value.GetType()));

                var tablekey = _customSerializer[typestring].Serializer(value);
                tablekey.Key.Name = GetPropertyNameString(groupName, key, typestring);
                tableEntry[tablekey.Key.Name, tablekey.Key.EdmTyp] = tablekey.Value;
            }
        }

        public static string GetPropertyNameString(string groupName, string propertyName, string propertyTyp = "", string version = "")
        {
            return string.Format(PropertyGroupFormat, groupName, propertyName, propertyTyp, version);
        }


        /// <summary>
        /// Will load properties in the format groupname_property[_customtype]
        /// from the prop.Key.Name
        /// </summary>
        /// <param name="propertyGroup"></param>
        /// <param name="prop"></param>
        private void DeserializePropertyGroupVal(PropertyGroupCollection propertyGroup, KeyValuePair<EdmProp, object> prop)
        {
            var parts = prop.Key.Name.Split('_');
            if (parts.Length < VersionIndex)
                return;

            var group = GetGroup(propertyGroup, parts[GroupNameIndex]);
            DeserializeProperty(group, parts, prop);
        }

        private void DeserializeProperty(PropertyGroup propertyGroup
            , string[] parts
            , KeyValuePair<EdmProp, object> prop)
        {
            if (!string.IsNullOrEmpty(parts[PropertyTypeIndex]))
            {
                DeserializeCustomPropertyTyp(parts[PropertyNameIndex], parts[PropertyTypeIndex], prop, propertyGroup);
                return;
            }

            propertyGroup[parts[PropertyNameIndex]] = prop.Value;

        }

        private void DeserializeCustomPropertyTyp(string propertyName
            , string customTyp
            , KeyValuePair<EdmProp, object> prop
            , PropertyGroup propertyGroup)
        {
            var serializer = _customSerializer[customTyp];
            if(serializer == null)
                throw new Exception(string.Format("Unable to deserialize type {0} for property group" +
                                                      ", please define a serializer", customTyp));

            var val = serializer.Deserializer(prop);
            propertyGroup[propertyName] = val;
        }

        private static PropertyGroup GetGroup(PropertyGroupCollection propertyGroups, string groupName)
        {
            var group = propertyGroups[groupName];
            if(group == null)
            {
                group = new PropertyGroup(){Name = groupName};
                propertyGroups[groupName] = group;
            }
            return group;
        }

        
    }
}