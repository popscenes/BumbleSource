using System;
using System.Runtime.Caching;
using WebSite.Application.Binding;
using WebSite.Application.Caching;
using WebSite.Application.Caching.Command;
using WebSite.Infrastructure.Caching;
using WebSite.Infrastructure.Command;

namespace PostaFlya.Application.Domain.Flier
{
    public static class CachedFlierContext
    {
        public const string Region = "flier";
        public const string Likes = "lik";
        public const string Comments = "comm";
        public const string Browser = "brows";
        public const string Flier = "flier";

    }
}