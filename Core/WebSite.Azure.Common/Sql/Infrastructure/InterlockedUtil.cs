using System.Collections.Generic;
using System.Threading;

namespace Website.Azure.Common.Sql.Infrastructure
{
    public static class InterlockedUtil
    {
        public static void SafeDictionaryAdd<TKey, TValue>(ref Dictionary<TKey, TValue> replace, TKey key, TValue value)
        {
            Dictionary<TKey, TValue> snapshot, newCache;
            do
            {
                snapshot = replace;
                newCache = new Dictionary<TKey, TValue>(replace);
                newCache[key] = value;

            } while (!ReferenceEquals(
                Interlocked.CompareExchange(ref replace, newCache, snapshot), snapshot));
        }

        public static void SafeDictionarySwap<TKey, TValue>(ref Dictionary<TKey, TValue> replace, Dictionary<TKey, TValue> source)
        {
            Dictionary<TKey, TValue> snapshot, newCache;
            do
            {
                snapshot = replace;
                newCache = source;

            } while (!ReferenceEquals(
                Interlocked.CompareExchange(ref replace, newCache, snapshot), snapshot));
        }
    }
}