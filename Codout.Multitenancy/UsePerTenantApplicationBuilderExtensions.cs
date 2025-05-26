using System;
using Codout.Multitenancy.Internal;
using Microsoft.AspNetCore.Builder;

namespace Codout.Multitenancy;

public static class UsePerTenantApplicationBuilderExtensions
{
    public static IApplicationBuilder UsePerTenant<TTenant>(this IApplicationBuilder app,
        Action<TenantPipelineBuilderContext<TTenant>, IApplicationBuilder> configuration)
        where TTenant : IAppTenant
    {
        app.Use(next => new TenantPipelineMiddleware<TTenant>(next, app, configuration).Invoke);
        return app;
    }
}