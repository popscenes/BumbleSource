using System;

namespace Website.Azure.Common.TableStorage
{
    public static class StorageKeyUtil
    {
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
            return string.Format(format, idPrefix, entityId);
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