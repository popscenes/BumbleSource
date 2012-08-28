using System.Runtime.Caching;
using Website.Infrastructure.Binding;
using Website.Infrastructure.Command;
using Website.Application.Domain.Command;
using Website.Domain.Content.Command;

namespace Website.Application.Domain.Content.Command
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