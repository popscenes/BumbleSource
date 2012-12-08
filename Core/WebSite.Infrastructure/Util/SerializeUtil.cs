using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace Website.Infrastructure.Util
{
    public static class SerializeUtil
    {
        public static byte[] ToByteArray<ObjectType>(ObjectType source) where ObjectType : class
        {
            if (source == null)
                return null;

            var formatter = new BinaryFormatter();
            using (var s = new MemoryStream())
            {
                formatter.Serialize(s, source);
                return s.ToArray();
            }
        }

        public static ObjectType FromByteArray<ObjectType>(byte[] array) where ObjectType : new()
        {
            if (array == null || array.Length == 0)
                return new ObjectType();

            var formatter = new BinaryFormatter();
            using (var s = new MemoryStream(array))
            {
                dynamic returnobject = formatter.Deserialize(s);
                return returnobject;
            }
        }

        public static object FromByteArray(byte[] array)
        {
            if (array == null || array.Length == 0)
                return null;

            var formatter = new BinaryFormatter();
            using (var s = new MemoryStream(array))
            {
                dynamic returnobject = formatter.Deserialize(s);
                return returnobject;
            }
        }

        public static void PropertiesFromDictionary(object target, IDictionary<string, object> dictionary)
        {
            foreach (var o in dictionary)
            {
                var prop = target.GetType().GetProperty(o.Key);
                if (prop == null || !prop.CanWrite || o.Value == null) continue;//for now assume that don't need to set null
                
                if(prop.PropertyType != o.Value.GetType())
                {
                    try
                    {
                        prop.SetValue(target, Convert.ChangeType(o.Value, prop.PropertyType), null);  
                    }
                    catch (Exception)
                    {
                        continue;
                    }
                    
                }
                else
                {
                    prop.SetValue(target, o.Value, null); 
                }
            }
        }

        public static void PropertiesToDictionary(object source
            , IDictionary<string, object> dictionary
            , ISet<Type> allowedTypes = null
            , ISet<string> exclude = null
            , bool includenull = true)
        {
            foreach (var prop in source.GetType().GetProperties())
            {
                if (exclude != null && exclude.Contains(prop.Name)) continue;
                if (allowedTypes != null && !allowedTypes.Contains(prop.PropertyType)) continue;
                if (prop.GetIndexParameters().Length > 0) continue;//not supporting collections
                
                var ob = prop.GetValue(source, null);
                if (ob != null || includenull)
                    dictionary.Add(prop.Name, ob);
            }
        }

        public static object GetPropertyVal(this object source, string property)
        {
            var prop = source.GetType().GetProperty(property);
            if (prop == null)
                return null;
            return prop.GetValue(source, null);
        }

        public static PropertyInfo GetPropertyWithAttribute(Type source, Type attribute)
        {
            return GetPropertiesWithAttribute(source, attribute).SingleOrDefault();
        }

        public static IList<PropertyInfo> GetPropertiesWithAttribute(Type source, Type attribute)
        {
            var ret = source.GetProperties().Where(
                p => HasAttribute(p, attribute));
            return ret.ToList();
        }

        public static bool HasAttribute(PropertyInfo source, Type attribute)
        {
            return source.GetCustomAttributes(true).Any(a
                                                        => a.GetType() == attribute);
        }

        public static object ConvertVal(object result, Type needed)
        {
            if (result == null)
                return null;

            if (needed == result.GetType())
                return result;

            try
            {
                var converter = TypeDescriptor.GetConverter(needed);
                if (converter.CanConvertFrom(result.GetType()))
                    return converter.ConvertFrom(result);

                converter = TypeDescriptor.GetConverter(result.GetType());
                if (converter.CanConvertTo(needed))
                    return converter.ConvertTo(result, needed);

                return Convert.ChangeType(result, needed);
            }
            catch (Exception e)
            {
                Trace.TraceError("Error converting from {0} to {1} : {2}", result.GetType().FullName, needed.FullName, e.Message);
                return null;

            }
        }

        public static bool IsGreaterThan<SourceType>(this SourceType value, SourceType other) where SourceType : IComparable
        {
            return value.CompareTo(other) > 0;
        }

        public static bool IsGreaterThanOrEqualTo<SourceType>(this SourceType value, SourceType other) where SourceType : IComparable
        {
            return value.CompareTo(other) >= 0;
        }

        public static bool IsLessThan<SourceType>(this SourceType value, SourceType other) where SourceType : IComparable
        {
            return value.CompareTo(other) < 0;
        }

        public static bool IsLessThanOrEqualTo<SourceType>(this SourceType value, SourceType other) where SourceType : IComparable
        {
            return value.CompareTo(other) <= 0;
        }
    }
}
