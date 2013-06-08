using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Website.Azure.Common.TableStorage
{
    public static class StorageKeyUtil
    {
        private const char EscapeChar = '*';
        private const string EscapeEscape = "**";
        private static readonly HashSet<char> _invalidKeyChars = new HashSet<char>()
            {
                '\\', '/','?','#','[',']','*'
            };

        public static string EscapeValForKey(this string key)
        {
            return key.ToCharArray()
                .Aggregate(new StringBuilder(), (builder, c) =>
                    {
                        if (c == EscapeChar) builder.Append(EscapeEscape);
                        else if (_invalidKeyChars.Contains(c)) builder.Append(EscapeChar);
                        else builder.Append(c);
                        return builder;
                    }).ToString();
        }

        public static string GetTableKeyForTerms(this IEnumerable<string> terms)
        {
            return terms.Select(s => s.Trim().ToLowerInvariant().EscapeValForKey())
                 .OrderBy(s => s)
                 .Aggregate(new StringBuilder(), (builder, s) =>
                     {
                         builder.Append('[');
                         builder.Append(s);
                         builder.Append(']');
                         return builder;
                     }).ToString();
        }

        public static string CheckValidForKey(this string forKey)
        {
            if(forKey.ToCharArray().Any(c => _invalidKeyChars.Contains(c)))
                throw new InvalidOperationException("invalid valid for table key: " + forKey);
            return forKey;
        }

        public static string ExtractEntityIdFromRowKey(this string key)
        {
            var start = key.IndexOf(']');
            return start < 0 ? key : key.Substring(start + 1);
        }

        public static string CreateRowKeyForEntityId(this string entityId, string idPrefix)
        {
            if (String.IsNullOrWhiteSpace(idPrefix))
                return entityId;
            const string format = "[{0}]{1}";
            return string.Format(format, idPrefix.EscapeValForKey(), entityId.CheckValidForKey());
        }

        //var dattimedesc = (DateTime.MaxValue.Ticks - DateTime.UtcNow.Ticks).ToString("D20");
        //var dattimeasc = (DateTime.UtcNow.Ticks).ToString("D20");
        public static string GetTimestampAscending(this DateTime dateTime)
        {
            return dateTime.Ticks.ToString("D20");
        }

        public static string GetTimestampDescending(this DateTime dateTime)
        {
            return (DateTime.MaxValue.Ticks - dateTime.Ticks).ToString("D20");
        }

        public static string ToAscendingTimeKey(this string id, DateTime dateTime)
        {
            return id.CreateRowKeyForEntityId(dateTime.GetTimestampAscending());
        }

        public static string ToDescendingTimeKey(this string id, DateTime dateTime)
        {
            return id.CreateRowKeyForEntityId(dateTime.GetTimestampDescending());
        }

        public static string GetTimestampAscending(this DateTimeOffset dateTime)
        {
            return dateTime.Ticks.ToString("D20");
        }

        public static string GetTimestampDescending(this DateTimeOffset dateTime)
        {
            return (DateTime.MaxValue.Ticks - dateTime.Ticks).ToString("D20");
        }

        public static string ToAscendingTimeKey(this string id, DateTimeOffset dateTime)
        {
            return id.CreateRowKeyForEntityId(dateTime.GetTimestampAscending());
        }

        public static string ToDescendingTimeKey(this string id, DateTimeOffset dateTime)
        {
            return id.CreateRowKeyForEntityId(dateTime.GetTimestampDescending());
        }

    }
}