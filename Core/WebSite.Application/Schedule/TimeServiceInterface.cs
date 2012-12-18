using System;

namespace Website.Application.Schedule
{
    public interface TimeServiceInterface
    {
        DateTimeOffset GetCurrentTime();
    }
}