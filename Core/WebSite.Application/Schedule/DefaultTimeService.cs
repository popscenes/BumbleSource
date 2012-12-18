using System;

namespace Website.Application.Schedule
{
    public class DefaultTimeService : TimeServiceInterface
    {
        public DateTimeOffset GetCurrentTime()
        {
            return DateTimeOffset.UtcNow;
        }
    }
}