using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace Codout.Multitenancy.Tests;

public class TenantContextTests
{
    [Fact]
    public void Construtor_DeveGerarIdUnicoEExporOTenant()
    {
        var tenant = new TestTenant { TenantKey = "t1" };

        var context1 = new TenantContext(tenant);
        var context2 = new TenantContext(tenant);

        context1.Tenant.Should().BeSameAs(tenant);
        context1.Id.Should().NotBeNullOrWhiteSpace();
        context1.Id.Should().NotBe(context2.Id);
        Guid.TryParse(context1.Id, out _).Should().BeTrue();
    }

    [Fact]
    public void Properties_IniciaVazioEAceitaItens()
    {
        var context = new TenantContext(new TestTenant());

        context.Properties.Should().BeEmpty();

        context.Properties["chave"] = 123;
        context.Properties["chave"].Should().Be(123);
    }

    [Fact]
    public void Dispose_DeveDescartarTenantDescartavel()
    {
        var tenant = new DisposableTenant();
        var context = new TenantContext(tenant);

        context.Dispose();

        tenant.Disposed.Should().BeTrue();
    }

    [Fact]
    public void Dispose_DeveDescartarPropriedadesDescartaveis()
    {
        var disposable = new DisposableTenant();
        var context = new TenantContext(new TestTenant());
        context.Properties["recurso"] = disposable;
        context.Properties["naoDescartavel"] = "texto";

        context.Dispose();

        disposable.Disposed.Should().BeTrue();
    }

    [Fact]
    public void Dispose_Duplo_NaoLancaExcecao()
    {
        var context = new TenantContext(new DisposableTenant());

        context.Dispose();
        var act = () => context.Dispose();

        act.Should().NotThrow();
    }

    [Fact]
    public void Dispose_ComPropriedadeJaDescartada_EngoleObjectDisposedException()
    {
        var context = new TenantContext(new TestTenant());
        context.Properties["stream"] = new ThrowOnDisposeStream();

        var act = () => context.Dispose();

        act.Should().NotThrow();
    }

    private sealed class ThrowOnDisposeStream : IDisposable
    {
        public void Dispose()
        {
            throw new ObjectDisposedException("stream");
        }
    }
}

public class TenantWrapperTests
{
    [Fact]
    public void Value_RetornaOTenantInformado()
    {
        var tenant = new TestTenant { TenantKey = "abc" };

        new TenantWrapper<TestTenant>(tenant).Value.Should().BeSameAs(tenant);
    }
}

public class MemoryCacheTenantResolverOptionsTests
{
    [Fact]
    public void Padrao_HabilitaEvictAllEDispose()
    {
        var options = new MemoryCacheTenantResolverOptions();

        options.EvictAllEntriesOnExpiry.Should().BeTrue();
        options.DisposeOnEviction.Should().BeTrue();
    }
}

public class MultitenancyHttpContextExtensionsTests
{
    [Fact]
    public void SetGetTenantContext_FazRoundtripPeloItems()
    {
        var httpContext = new DefaultHttpContext();
        var tenantContext = new TenantContext(new TestTenant { TenantKey = "t1" });

        httpContext.SetTenantContext(tenantContext);

        httpContext.GetTenantContext().Should().BeSameAs(tenantContext);
    }

    [Fact]
    public void GetTenantContext_SemContexto_RetornaNulo()
    {
        new DefaultHttpContext().GetTenantContext().Should().BeNull();
    }

    [Fact]
    public void GetTenant_RetornaOTenantTipado()
    {
        var httpContext = new DefaultHttpContext();
        var tenant = new TestTenant { TenantKey = "t1" };
        httpContext.SetTenantContext(new TenantContext(tenant));

        httpContext.GetTenant<TestTenant>().Should().BeSameAs(tenant);
    }

    [Fact]
    public void GetTenant_SemContexto_RetornaDefault()
    {
        new DefaultHttpContext().GetTenant<TestTenant>().Should().BeNull();
    }

    [Fact]
    public void GetTenantContext_ComValorDeOutroTipoNoItems_RetornaNulo()
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Items["softprime.TenantContext"] = "não é um TenantContext";

        httpContext.GetTenantContext().Should().BeNull();
    }
}
