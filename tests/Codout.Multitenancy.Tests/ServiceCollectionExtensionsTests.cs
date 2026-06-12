using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Codout.Multitenancy.Tests;

public class MultitenancyServiceCollectionExtensionsTests
{
    [Fact]
    public void AddMultitenancy_RegistraOResolverComoScoped()
    {
        var services = new ServiceCollection();

        services.AddMultitenancy<HostTenantResolver>();
        using var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();

        scope.ServiceProvider.GetService<ITenantResolver>().Should().BeOfType<HostTenantResolver>();
    }

    [Fact]
    public void AddMultitenancy_ComResolverDeMemoryCache_RegistraIMemoryCache()
    {
        var services = new ServiceCollection();

        services.AddMultitenancy<HostTenantResolver>();
        using var provider = services.BuildServiceProvider();

        provider.GetService<IMemoryCache>().Should().NotBeNull();
    }

    [Fact]
    public void AddMultitenancy_ComResolverSimples_NaoRegistraIMemoryCache()
    {
        var services = new ServiceCollection();

        services.AddMultitenancy<SimpleResolver>();
        using var provider = services.BuildServiceProvider();

        provider.GetService<IMemoryCache>().Should().BeNull();
    }

    [Fact]
    public void AddMultitenancy_TenantContextVemDoHttpContextAtual()
    {
        var services = new ServiceCollection();
        services.AddMultitenancy<HostTenantResolver>();
        using var provider = services.BuildServiceProvider();

        var tenant = new TestTenant { TenantKey = "t1" };
        var tenantContext = new TenantContext(tenant);
        var httpContext = new DefaultHttpContext();
        httpContext.SetTenantContext(tenantContext);
        provider.GetRequiredService<IHttpContextAccessor>().HttpContext = httpContext;

        using var scope = provider.CreateScope();

        scope.ServiceProvider.GetService<TenantContext>().Should().BeSameAs(tenantContext);
        scope.ServiceProvider.GetService<IAppTenant>().Should().BeSameAs(tenant);
        scope.ServiceProvider.GetRequiredService<ITenant<IAppTenant>>().Value.Should().BeSameAs(tenant);
    }

    [Fact]
    public void AddMultitenancy_SemHttpContext_ResolveTenantContextNulo()
    {
        var services = new ServiceCollection();
        services.AddMultitenancy<HostTenantResolver>();
        using var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();

        scope.ServiceProvider.GetService<TenantContext>().Should().BeNull();
        scope.ServiceProvider.GetService<IAppTenant>().Should().BeNull();
    }

    private sealed class SimpleResolver : ITenantResolver
    {
        public Task<TenantContext> ResolveAsync(HttpContext context)
        {
            return Task.FromResult<TenantContext>(null!);
        }
    }
}

public class MultitenancyOptionsTests
{
    [Fact]
    public void Tenants_PodeSerAtribuidoELido()
    {
        var options = new MultitenancyOptions<TestTenant>
        {
            Tenants = [new TestTenant { TenantKey = "t1" }]
        };

        options.Tenants.Should().ContainSingle(t => t.TenantKey == "t1");
    }
}

public class DataBaseTypeTests
{
    [Theory]
    [InlineData(DataBaseType.Postgres, "Postgres")]
    [InlineData(DataBaseType.MsSql, "Mssql")]
    [InlineData(DataBaseType.Oracle, "Oracle")]
    public void Valores_TemDisplayNameEDescription(DataBaseType value, string expected)
    {
        var member = typeof(DataBaseType).GetMember(value.ToString()).Single();

        member.GetCustomAttributes(typeof(DisplayAttribute), false)
            .Cast<DisplayAttribute>().Single().Name.Should().Be(expected);
        member.GetCustomAttributes(typeof(DescriptionAttribute), false)
            .Cast<DescriptionAttribute>().Single().Description.Should().Be(expected);
    }
}
