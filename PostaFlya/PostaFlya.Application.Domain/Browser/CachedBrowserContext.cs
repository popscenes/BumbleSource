using System.Runtime.Caching;
using WebSite.Application.Binding;
using WebSite.Application.Caching;
using WebSite.Application.Caching.Command;
using WebSite.Infrastructure.Command;

namespace PostaFlya.Application.Domain.Browser
{
    public static class CachedBrowserContext
    {
        public const string Region = "browser";
        public const string Browser = "brow";
        public const string Identity = "id";
    }
}