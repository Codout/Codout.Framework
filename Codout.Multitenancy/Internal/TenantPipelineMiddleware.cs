using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace Codout.Multitenancy.Internal;

public class TenantPipelineMiddleware<TTenant> where TTenant : IAppTenant
{
    private readonly Action<TenantPipelineBuilderContext<TTenant>, IApplicationBuilder> _configuration;
    private readonly RequestDelegate _next;

    private readonly ConcurrentDictionary<TTenant, Lazy<RequestDelegate>> _pipelines = new();

    private readonly IApplicationBuilder _rootApp;

    public TenantPipelineMiddleware(
        RequestDelegate next,
        IApplicationBuilder rootApp,
        Action<TenantPipelineBuilderContext<TTenant>, IApplicationBuilder> configuration)
    {
        _next = next;
        _rootApp = rootApp;
        _configuration = configuration;
    }

    public async Task Invoke(HttpContext context)
    {
        var tenantContext = context.GetTenantContext();

        if (tenantContext != null)
        {
            var tenantPipeline = _pipelines.GetOrAdd(
                (TTenant)tenantContext.Tenant,
                new Lazy<RequestDelegate>(() => BuildTenantPipeline(tenantContext)));

            await tenantPipeline.Value(context);
        }
    }

    private RequestDelegate BuildTenantPipeline(TenantContext tenantContext)
    {
        var branchBuilder = _rootApp.New();

        var builderContext = new TenantPipelineBuilderContext<TTenant>
        {
            TenantContext = tenantContext,
            Tenant = (TTenant)tenantContext.Tenant
        };

        _configuration(builderContext, branchBuilder);

        // register root pipeline at the end of the tenant branch
        branchBuilder.Run(_next);

        return branchBuilder.Build();
    }
}