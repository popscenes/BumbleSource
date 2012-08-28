using System;
using System.Collections.Generic;

namespace Website.Infrastructure.Util.Extension
{
    public static class DictionaryExtensions
    {
        public static ConvType GetOrDefault<KeyType, ValType, ConvType>(this Dictionary<KeyType, ValType> dictionary, KeyType key, ConvType defaultVal)
        {
            ValType val;
            if (dictionary.TryGetValue(key, out val))
                return (ConvType)Convert.ChangeType(val, defaultVal.GetType());
            return defaultVal;
        }
    }
}
