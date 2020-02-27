using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;

namespace Codout.Multitenancy
{
    public abstract class MemoryCacheTenantResolver : ITenantResolver
    {
        protected readonly IMemoryCache Cache;
        protected readonly MemoryCacheTenantResolverOptions Options;

        public MemoryCacheTenantResolver(IMemoryCache cache)
            : this(cache, new MemoryCacheTenantResolverOptions())
        {
        }

        public MemoryCacheTenantResolver(IMemoryCache cache, MemoryCacheTenantResolverOptions options)
        {
            Cache = cache;
            Options = options;
        }

        protected virtual MemoryCacheEntryOptions CreateCacheEntryOptions()
        {
            return new MemoryCacheEntryOptions().SetSlidingExpiration(new TimeSpan(1, 0, 0));
        }

        protected virtual void DisposeTenantContext(object cacheKey, TenantContext tenantContext)
        {
            tenantContext?.Dispose();
        }

        protected abstract string GetContextIdentifier(HttpContext context);
        protected abstract string GetTenantIdentifier(TenantContext context);
        protected abstract Task<TenantContext> ResolveAsync(HttpContext context);

        async Task<TenantContext> ITenantResolver.ResolveAsync(HttpContext context)
        {
            var cacheKey = GetContextIdentifier(context);

            if (cacheKey == null)
            {
                return null;
            }

            var tenantContext = Cache.Get(cacheKey) as TenantContext;

            if (tenantContext == null)
            {
                tenantContext = await ResolveAsync(context);

                if (tenantContext != null)
                {
                    var tenantIdentifier = GetTenantIdentifier(tenantContext);

                    if (!string.IsNullOrWhiteSpace(tenantIdentifier))
                    {
                        var cacheEntryOptions = GetCacheEntryOptions();

                        Cache.Set(tenantIdentifier, tenantContext, cacheEntryOptions);
                    }
                }
            }
            
            return tenantContext;
        }

        private MemoryCacheEntryOptions GetCacheEntryOptions()
        {
            var cacheEntryOptions = CreateCacheEntryOptions();

            if (Options.EvictAllEntriesOnExpiry)
            {
                var tokenSource = new CancellationTokenSource();

                cacheEntryOptions
                    .RegisterPostEvictionCallback(
                        (key, value, reason, state) =>
                        {
                            tokenSource.Cancel();
                        })
                    .AddExpirationToken(new CancellationChangeToken(tokenSource.Token));
            }

            if (Options.DisposeOnEviction)
            {
                cacheEntryOptions
                    .RegisterPostEvictionCallback(
                        (key, value, reason, state) =>
                        {
                            DisposeTenantContext(key, value as TenantContext);
                        });
            }

            return cacheEntryOptions;
        }
    }
}
