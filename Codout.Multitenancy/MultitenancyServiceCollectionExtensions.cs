using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Codout.Multitenancy;

public static class MultitenancyServiceCollectionExtensions
{
    public static IServiceCollection AddMultitenancy<TResolver>(this IServiceCollection services)
        where TResolver : class, ITenantResolver
    {
        services.AddScoped<ITenantResolver, TResolver>();
        services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();
        services.AddScoped(prov => prov.GetService<IHttpContextAccessor>()?.HttpContext?.GetTenantContext());
        services.AddScoped(prov => prov.GetService<TenantContext>()?.Tenant);
        services.AddScoped<ITenant<IAppTenant>>(prov => new TenantWrapper<IAppTenant>(prov.GetService<IAppTenant>()));

        var resolverType = typeof(TResolver);
        if (typeof(MemoryCacheTenantResolver).IsAssignableFrom(resolverType))
            services.AddMemoryCache();

        return services;
    }
}