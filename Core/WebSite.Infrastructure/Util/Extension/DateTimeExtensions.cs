using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Website.Infrastructure.Util.Extension
{
    public static class DateTimeExtensions
    {
        public static DateTime AsUnspecified(this DateTime source)
        {
            if (source.Kind == DateTimeKind.Unspecified)
                return source;

            return new DateTime(source.Ticks, DateTimeKind.Unspecified);
        }

        public static DateTimeOffset AsUnspecifiedDateTimeOffset(this DateTime source)
        {
            return new DateTimeOffset(source.AsUnspecified(), TimeSpan.Zero);
        }

        public static DateTimeOffset SetOffsetMinutes(this DateTimeOffset source, int minutes)
        {
            return new DateTimeOffset(source.DateTime, new TimeSpan(0, minutes, 0));
        }

    }
}
