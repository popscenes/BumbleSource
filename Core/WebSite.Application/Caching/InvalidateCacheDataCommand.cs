using System;
using WebSite.Infrastructure.Command;

namespace WebSite.Application.Caching
{
    [Serializable]
    public class InvalidateCacheDataCommand : CommandInterface
    {
        public string Key { get; set; }
        public string Region { get; set; }
        public string CommandId { get; set; }
    }
}