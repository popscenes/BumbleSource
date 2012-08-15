using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Text;
using WebSite.Infrastructure.Command;

namespace WebSite.Application.Caching
{
    internal class InvalidateCacheDataCommandHandler : CommandHandlerInterface<InvalidateCacheDataCommand>
    {
        private readonly ObjectCache _objectCache;
        public InvalidateCacheDataCommandHandler(ObjectCache objectCache)
        {
            _objectCache = objectCache;
        }

        public object Handle(InvalidateCacheDataCommand command)
        {
           return _objectCache.Remove(command.Key, command.Region);
        }
    }
}
