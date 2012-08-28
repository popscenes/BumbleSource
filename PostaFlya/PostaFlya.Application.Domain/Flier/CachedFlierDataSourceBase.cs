using System;
using System.Runtime.Caching;
using Website.Application.Binding;
using Website.Application.Caching;
using Website.Application.Caching.Command;
using Website.Infrastructure.Caching;
using Website.Infrastructure.Command;

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