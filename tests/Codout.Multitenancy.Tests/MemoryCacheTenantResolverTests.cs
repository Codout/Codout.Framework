using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Xunit;

namespace Codout.Multitenancy.Tests;

public class MemoryCacheTenantResolverTests : IDisposable
{
    private readonly MemoryCache _cache = new(new MemoryCacheOptions());

    public void Dispose()
    {
        _cache.Dispose();
    }

    private HostTenantResolver CreateResolver(MemoryCacheTenantResolverOptions? options = null)
    {
        var resolver = options == null
            ? new HostTenantResolver(_cache)
            : new HostTenantResolver(_cache, options);

        resolver.Tenants["tenant1.example.test"] = new TestTenant
        {
            TenantKey = "tenant1.example.test",
            ConnectionString = "Server=db1",
            DataBaseType = DataBaseType.Postgres
        };

        return resolver;
    }

    [Fact]
    public async Task ResolveAsync_ComHostConhecido_RetornaOTenantContext()
    {
        ITenantResolver resolver = CreateResolver();

        var context = await resolver.ResolveAsync(HttpContextFactory.WithHost("tenant1.example.test"));

        context.Should().NotBeNull();
        ((TestTenant)context!.Tenant).ConnectionString.Should().Be("Server=db1");
    }

    [Fact]
    public async Task ResolveAsync_ComHostDesconhecido_RetornaNulo()
    {
        ITenantResolver resolver = CreateResolver();

        var context = await resolver.ResolveAsync(HttpContextFactory.WithHost("desconhecido.example.test"));

        context.Should().BeNull();
    }

    [Fact]
    public async Task ResolveAsync_SemIdentificadorDeContexto_RetornaNuloSemResolver()
    {
        var resolver = CreateResolver();

        // DefaultHttpContext sem Host definido → GetContextIdentifier retorna null.
        var context = await ((ITenantResolver)resolver).ResolveAsync(new DefaultHttpContext());

        context.Should().BeNull();
        resolver.ResolveCalls.Should().Be(0);
    }

    [Fact]
    public async Task ResolveAsync_SegundaChamada_UsaOCache()
    {
        var resolver = CreateResolver();
        ITenantResolver tenantResolver = resolver;

        var first = await tenantResolver.ResolveAsync(HttpContextFactory.WithHost("tenant1.example.test"));
        var second = await tenantResolver.ResolveAsync(HttpContextFactory.WithHost("tenant1.example.test"));

        resolver.ResolveCalls.Should().Be(1, "a segunda chamada deve vir do cache");
        second.Should().BeSameAs(first);
    }

    [Fact]
    public async Task ResolveAsync_QuandoChaveDeBuscaDifereDaChaveDeGravacao_NuncaUsaOCache()
    {
        // BUG?: o resolver busca no cache pela chave de GetContextIdentifier, mas grava
        // pela chave de GetTenantIdentifier. Se as duas convenções diferirem, o cache
        // nunca é consultado com a chave gravada e o tenant é re-resolvido a cada request.
        var resolver = CreateResolver();
        resolver.TenantIdentifierOverride = ctx => $"tenant:{((TestTenant)ctx.Tenant).TenantKey}";
        ITenantResolver tenantResolver = resolver;

        await tenantResolver.ResolveAsync(HttpContextFactory.WithHost("tenant1.example.test"));
        await tenantResolver.ResolveAsync(HttpContextFactory.WithHost("tenant1.example.test"));

        resolver.ResolveCalls.Should().Be(2);
        _cache.Get("tenant:tenant1.example.test").Should().NotBeNull("a gravação usa o tenant identifier");
        _cache.Get("tenant1.example.test").Should().BeNull("a busca usa o context identifier, que nunca foi gravado");
    }

    [Fact]
    public async Task ResolveAsync_TenantNaoResolvido_NaoGravaNoCache()
    {
        var resolver = CreateResolver();
        ITenantResolver tenantResolver = resolver;

        await tenantResolver.ResolveAsync(HttpContextFactory.WithHost("desconhecido.example.test"));
        await tenantResolver.ResolveAsync(HttpContextFactory.WithHost("desconhecido.example.test"));

        resolver.ResolveCalls.Should().Be(2, "tenants não resolvidos não são cacheados");
    }

    [Fact]
    public async Task Evicao_ComDisposeOnEviction_DescartaOTenantContext()
    {
        var resolver = CreateResolver();
        var disposableTenant = new DisposableTenant { TenantKey = "tenant2.example.test" };
        resolver.Tenants["tenant2.example.test"] = disposableTenant;
        ITenantResolver tenantResolver = resolver;

        await tenantResolver.ResolveAsync(HttpContextFactory.WithHost("tenant2.example.test"));
        _cache.Remove("tenant2.example.test");

        // Callbacks de evicção do MemoryCache executam em background.
        var deadline = DateTime.UtcNow.AddSeconds(10);
        while (!disposableTenant.Disposed && DateTime.UtcNow < deadline)
            await Task.Delay(50);

        disposableTenant.Disposed.Should().BeTrue();
    }

    [Fact]
    public async Task Evicao_ComDisposeOnEvictionDesligado_NaoDescartaOTenant()
    {
        var resolver = CreateResolver(new MemoryCacheTenantResolverOptions
        {
            DisposeOnEviction = false,
            EvictAllEntriesOnExpiry = false
        });
        var disposableTenant = new DisposableTenant { TenantKey = "tenant3.example.test" };
        resolver.Tenants["tenant3.example.test"] = disposableTenant;
        ITenantResolver tenantResolver = resolver;

        await tenantResolver.ResolveAsync(HttpContextFactory.WithHost("tenant3.example.test"));
        _cache.Remove("tenant3.example.test");

        await Task.Delay(200);

        disposableTenant.Disposed.Should().BeFalse();
    }
}
