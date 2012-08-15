using System.Runtime.Caching;
using WebSite.Application.Binding;
using WebSite.Application.Caching;
using WebSite.Application.Caching.Command;
using WebSite.Infrastructure.Command;

namespace PostaFlya.Application.Domain.Content
{
    public static class CachedImageContext
    {
        public const string Region = "image";
        public const string Image = "img";
        public const string Browser = "brows";
    }
}