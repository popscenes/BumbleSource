using System;
using System.Runtime.Caching;
using PostaFlya.Application.Domain.Command;
using WebSite.Application.Binding;
using WebSite.Application.Caching.Command;
using PostaFlya.Domain.Content;
using PostaFlya.Domain.Content.Command;
using WebSite.Infrastructure.Command;
using WebSite.Infrastructure.Binding;

namespace PostaFlya.Application.Domain.Content.Command
{
    internal class CachedImageRepository : CachedRepositoryWithBrowser,
                                           ImageRepositoryInterface
    {
        public CachedImageRepository([SourceDataSource]ImageRepositoryInterface imageRepository
                , ObjectCache cacheProvider)
            : base(cacheProvider, CachedImageContext.Region, imageRepository)
        {
        }
    }
}