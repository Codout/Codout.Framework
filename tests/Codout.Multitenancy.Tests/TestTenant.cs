using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;

namespace Codout.Multitenancy.Tests;

public class TestTenant : IAppTenant
{
    public string TenantKey { get; set; } = string.Empty;

    public DataBaseType DataBaseType { get; set; }

    public string ConnectionString { get; set; } = string.Empty;
}

public class DisposableTenant : TestTenant, IDisposable
{
    public bool Disposed { get; private set; }

    public void Dispose()
    {
        Disposed = true;
    }
}

/// <summary>
///     Resolver concreto para exercitar MemoryCacheTenantResolver: resolve o tenant
///     pelo host da requisição a partir de um dicionário em memória.
/// </summary>
public class HostTenantResolver(IMemoryCache cache, MemoryCacheTenantResolverOptions options)
    : MemoryCacheTenantResolver(cache, options)
{
    public HostTenantResolver(IMemoryCache cache)
        : this(cache, new MemoryCacheTenantResolverOptions())
    {
    }

    public Dictionary<string, TestTenant> Tenants { get; } = new();

    public int ResolveCalls { get; private set; }

    /// <summary>
    ///     Permite simular o cenário em que o identificador do contexto (chave de busca)
    ///     difere do identificador do tenant (chave de gravação).
    /// </summary>
    public Func<TenantContext, string>? TenantIdentifierOverride { get; set; }

    protected override string GetContextIdentifier(HttpContext context)
    {
        var host = context.Request.Host.Value;
        return string.IsNullOrEmpty(host) ? null! : host;
    }

    protected override string GetTenantIdentifier(TenantContext context)
    {
        return TenantIdentifierOverride != null
            ? TenantIdentifierOverride(context)
            : ((TestTenant)context.Tenant).TenantKey;
    }

    protected override Task<TenantContext> ResolveAsync(HttpContext context)
    {
        ResolveCalls++;

        return Task.FromResult(
            Tenants.TryGetValue(context.Request.Host.Value!, out var tenant)
                ? new TenantContext(tenant)
                : null!);
    }
}

public static class HttpContextFactory
{
    public static DefaultHttpContext WithHost(string host)
    {
        var context = new DefaultHttpContext();
        context.Request.Scheme = "http";
        context.Request.Host = new HostString(host);
        return context;
    }
}
