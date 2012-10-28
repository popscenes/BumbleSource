using System;
using System.Runtime.Caching;
using Website.Application.Binding;
using Website.Application.Caching.Command;
using PostaFlya.Domain.Flier;
using PostaFlya.Domain.Flier.Command;
using Website.Infrastructure.Command;
using Website.Infrastructure.Binding;
using Website.Application.Domain.Command;

namespace PostaFlya.Application.Domain.Flier.Command
{
    internal class CachedFlierRepository : CachedRepositoryWithBrowser,
                                            FlierRepositoryInterface
    {

        public CachedFlierRepository([SourceDataSource]FlierRepositoryInterface flierRepository
                , ObjectCache cacheProvider)
            : base(cacheProvider, CachedFlierContext.Region, flierRepository)
        {
        }

    }
}