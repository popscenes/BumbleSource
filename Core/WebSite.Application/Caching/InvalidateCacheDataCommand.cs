using System;
using Website.Infrastructure.Command;

namespace Website.Application.Caching
{
    [Serializable]
    public class InvalidateCacheDataCommand : CommandInterface
    {
        public string Key { get; set; }
        public string Region { get; set; }
        public string CommandId { get; set; }
    }
}