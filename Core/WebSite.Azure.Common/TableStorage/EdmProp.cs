using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml;

namespace Website.Azure.Common.TableStorage
{
    public class EdmProp
    {
        public string EdmTyp { get; set; }
        public string Name { get; set; }
        public EdmProp()
        {
            EdmTyp = "";
            Name = "";
        }
        public override bool Equals(object obj)
        {
            var edmKey = obj as EdmProp;
            return edmKey != null && Name.Equals(edmKey.Name);
        }
        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }
    }

    public static class Edm
    {
        public const string String = "Edm.String";
        public const string Int32 = "Edm.Int32";
        public const string Int64 = "Edm.Int64";
        public const string Double = "Edm.Double";
        public const string Boolean = "Edm.Boolean";
        public const string DateTime = "Edm.DateTime";
        public const string Binary = "Edm.Binary";
        public const string Guid = "Edm.Guid";

        public static bool IsString(string type)
        {
            return String.Equals(type);
        }

        public static object ConvertFromEdmValue(string type, string value, bool isnull)
        {
            if (isnull) return null;
            if (string.IsNullOrEmpty(type)) return value;

            switch (type)
            {
                case String: return value;
                case DateTime: return XmlConvert.ToDateTime(value, XmlDateTimeSerializationMode.RoundtripKind);
                case Binary: return Convert.FromBase64String(value);
                case Guid: return new Guid(value);
                default:
                    Type typeOf;
                    if (!EdmDesToTyp.TryGetValue(type, out typeOf))
                        throw new NotSupportedException("Not supported type " + type);
                    return Convert.ChangeType(value, typeOf);
            }
        }

        public static object ConvertToEdmValue(string type, object value)
        {
            if (value == null) return null;
            if (string.IsNullOrEmpty(type)) return value;

            switch (type)
            {
                case Binary:
                    var byteval = value as byte[] ?? new byte[0];
                    return Convert.ToBase64String(byteval);
                default:
                    Type typeOf;
                    if (!EdmDesToTyp.TryGetValue(type, out typeOf))
                        throw new NotSupportedException("Not supported type " + type);
                    return Convert.ChangeType(value, typeOf);
            }
        }

        public static bool IsEdmTyp(string edmTyp)
        {
            if (string.IsNullOrWhiteSpace(edmTyp))
                return false;
            return EdmDesToTyp.ContainsKey(edmTyp);
        }

        public static bool IsEdmTyp(object value)
        {
            if (value == null) return false;
            return IsEdmTyp(value.GetType());
        }

        public static bool IsEdmTyp(Type type)
        {
            if (type == null) return false;
            return GetEdmTyp(type) != null;
        }

        public static Type GetDefaultForEdmTyp(string edmTyp)
        {
            Type typeOf;
            return !EdmDesToTyp.TryGetValue(edmTyp, out typeOf) ? null : typeOf;
        }

        public static MethodInfo GetToMethodMatchingContructorForEdmTyp(Type type)
        {
            var constructors = GetEdmConstuctors(type);
            return constructors.Count == 0 ? null : GetToMethodMatchingContructorParam(type, constructors.FirstOrDefault());
        }

        public static MethodInfo GetToMethodMatchingContructorParam(Type construcTyp, ConstructorInfo info)
        {
            var type = info.GetParameters()[0].ParameterType;
            return construcTyp.GetMethods().SingleOrDefault(mi =>
                                           mi.Name.StartsWith("To")
                                           && mi.GetParameters().Length == 0
                                           && mi.ReturnType == type);
        }

        private static readonly Func<ConstructorInfo, bool> HasEdmConstuctor =
            info => info.GetParameters().Length == 1 && Edm.IsEdmTyp(info.GetParameters()[0].ParameterType);
        public static IList<ConstructorInfo> GetEdmConstuctors(Type type)
        {
            return type.GetConstructors().Where(HasEdmConstuctor).ToList();
        }

        public static bool HasEdmConstuctors(Type type)
        {
            return GetEdmConstuctors(type).Count > 0;
        }

        public static string GetEdmTyp(Type type)
        {
            string ret;
            return !EdmTypToDes.TryGetValue(type, out ret) ? null : ret;
        }

        private static readonly Dictionary<Type, string> EdmTypToDes
            = new Dictionary<Type, string>()
            {
                //{typeof(byte), "Edm.Byte"}, not supported
                //{typeof(sbyte), "Edm.SByte"}, not supported
                //{typeof(short), "Edm.Int16"}, not supported
                //{typeof(decimal), "Edm.Decimal"}, not supported
                //{typeof(float), "Edm.Single"}, not supported
                {typeof(byte), Int32}, 
                {typeof(sbyte), Int32}, 
                {typeof(short), Int32}, 
                {typeof(float), Double}, 
                {typeof(decimal), Double},

                {typeof(string), String},
                {typeof(int), Int32},
                {typeof(long), Int64},
                {typeof(double), Double},               
                {typeof(bool), Boolean},             
                {typeof(DateTime), DateTime},
                {typeof(byte[]), Binary},
                {typeof(Guid), Guid},
            };

        private static readonly Dictionary<string, Type> EdmDesToTyp
            = new Dictionary<string, Type>()
            {
                {String, typeof(string)},
                {Int32, typeof(int)},
                {Int64, typeof(long)},
                {Double, typeof(double)},               
                {Boolean, typeof(bool)},             
                {DateTime, typeof(DateTime)},
                {Binary, typeof(byte[])},
                {Guid, typeof(Guid)},                                                              
            };
    }
}