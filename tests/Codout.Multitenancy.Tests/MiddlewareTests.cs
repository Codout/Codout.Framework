using Codout.Multitenancy.Internal;
using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Codout.Multitenancy.Tests;

public class TenantResolutionMiddlewareTests
{
    [Fact]
    public async Task Invoke_QuandoResolverEncontraTenant_GravaNoHttpContext()
    {
        var tenantContext = new TenantContext(new TestTenant { TenantKey = "t1" });
        var resolver = new Mock<ITenantResolver>();
        resolver.Setup(r => r.ResolveAsync(It.IsAny<HttpContext>())).ReturnsAsync(tenantContext);

        var nextCalled = false;
        var middleware = new TenantResolutionMiddleware(_ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        });

        var httpContext = new DefaultHttpContext();
        await middleware.Invoke(httpContext, resolver.Object);

        httpContext.GetTenantContext().Should().BeSameAs(tenantContext);
        nextCalled.Should().BeTrue();
    }

    [Fact]
    public async Task Invoke_QuandoResolverNaoEncontra_SegueSemTenantContext()
    {
        var resolver = new Mock<ITenantResolver>();
        resolver.Setup(r => r.ResolveAsync(It.IsAny<HttpContext>())).ReturnsAsync((TenantContext?)null);

        var nextCalled = false;
        var middleware = new TenantResolutionMiddleware(_ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        });

        var httpContext = new DefaultHttpContext();
        await middleware.Invoke(httpContext, resolver.Object);

        httpContext.GetTenantContext().Should().BeNull();
        nextCalled.Should().BeTrue();
    }
}

public class TenantUnresolvedRedirectMiddlewareTests
{
    [Fact]
    public async Task Invoke_SemTenant_RedirecionaTemporariamente()
    {
        var middleware = new TenantUnresolvedRedirectMiddleware<TestTenant>(
            _ => Task.CompletedTask, "https://landing.example.test/", false);

        var httpContext = new DefaultHttpContext();
        await middleware.Invoke(httpContext);

        httpContext.Response.StatusCode.Should().Be(StatusCodes.Status302Found);
        httpContext.Response.Headers.Location.ToString().Should().Be("https://landing.example.test/");
    }

    [Fact]
    public async Task Invoke_SemTenant_ComRedirectPermanente_Retorna301()
    {
        var middleware = new TenantUnresolvedRedirectMiddleware<TestTenant>(
            _ => Task.CompletedTask, "https://landing.example.test/", true);

        var httpContext = new DefaultHttpContext();
        await middleware.Invoke(httpContext);

        httpContext.Response.StatusCode.Should().Be(StatusCodes.Status301MovedPermanently);
    }

    [Fact]
    public async Task Invoke_ComTenant_ChamaOProximoMiddleware()
    {
        var nextCalled = false;
        var middleware = new TenantUnresolvedRedirectMiddleware<TestTenant>(
            _ =>
            {
                nextCalled = true;
                return Task.CompletedTask;
            }, "https://landing.example.test/", false);

        var httpContext = new DefaultHttpContext();
        httpContext.SetTenantContext(new TenantContext(new TestTenant()));
        await middleware.Invoke(httpContext);

        nextCalled.Should().BeTrue();
        httpContext.Response.Headers.Location.Should().BeEmpty();
    }
}

public class PrimaryHostnameRedirectMiddlewareTests
{
    [Fact]
    public async Task Invoke_ComHostDiferenteDoPrimario_Redireciona()
    {
        var middleware = new PrimaryHostnameRedirectMiddleware<TestTenant>(
            _ => Task.CompletedTask, _ => "primario.example.test", false);

        var httpContext = HttpContextFactory.WithHost("alias.example.test");
        httpContext.SetTenantContext(new TenantContext(new TestTenant()));
        await middleware.Invoke(httpContext);

        httpContext.Response.Headers.Location.ToString()
            .Should().Be("http://primario.example.test/");
        httpContext.Response.StatusCode.Should().Be(StatusCodes.Status302Found);
    }

    [Fact]
    public async Task Invoke_ComHostPrimario_NaoRedireciona()
    {
        var nextCalled = false;
        var middleware = new PrimaryHostnameRedirectMiddleware<TestTenant>(
            _ =>
            {
                nextCalled = true;
                return Task.CompletedTask;
            }, _ => "primario.example.test", false);

        var httpContext = HttpContextFactory.WithHost("PRIMARIO.example.test");
        httpContext.SetTenantContext(new TenantContext(new TestTenant()));
        await middleware.Invoke(httpContext);

        nextCalled.Should().BeTrue("a comparação de host é case-insensitive");
    }

    [Fact]
    public async Task Invoke_SemTenant_NaoRedireciona()
    {
        var nextCalled = false;
        var middleware = new PrimaryHostnameRedirectMiddleware<TestTenant>(
            _ =>
            {
                nextCalled = true;
                return Task.CompletedTask;
            }, _ => "primario.example.test", false);

        await middleware.Invoke(HttpContextFactory.WithHost("alias.example.test"));

        nextCalled.Should().BeTrue();
    }
}

public class TenantPipelineMiddlewareTests
{
    private static (RequestDelegate Pipeline, List<string> Log) BuildPipeline()
    {
        var services = new ServiceCollection().BuildServiceProvider();
        var app = new ApplicationBuilder(services);
        var log = new List<string>();

        app.UsePerTenant<TestTenant>((context, branch) =>
        {
            branch.Use(async (http, next) =>
            {
                log.Add($"branch:{context.Tenant.TenantKey}");
                await next();
            });
        });

        app.Run(_ =>
        {
            log.Add("root");
            return Task.CompletedTask;
        });

        return (app.Build(), log);
    }

    [Fact]
    public async Task UsePerTenant_ComTenant_ExecutaOBranchEDepoisORoot()
    {
        var (pipeline, log) = BuildPipeline();

        var httpContext = new DefaultHttpContext();
        httpContext.SetTenantContext(new TenantContext(new TestTenant { TenantKey = "t1" }));
        await pipeline(httpContext);

        log.Should().ContainInOrder("branch:t1", "root");
    }

    [Fact]
    public async Task UsePerTenant_SemTenant_CurtoCircuitaSemExecutarORoot()
    {
        // Observação de caracterização: sem TenantContext o middleware não chama _next,
        // então o restante do pipeline (inclusive o root) nunca executa.
        var (pipeline, log) = BuildPipeline();

        await pipeline(new DefaultHttpContext());

        log.Should().BeEmpty();
    }

    [Fact]
    public async Task UsePerTenant_ReutilizaOPipelinePorTenant()
    {
        var services = new ServiceCollection().BuildServiceProvider();
        var app = new ApplicationBuilder(services);
        var builds = 0;

        app.UsePerTenant<TestTenant>((_, _) => builds++);
        app.Run(_ => Task.CompletedTask);
        var pipeline = app.Build();

        var tenant = new TestTenant { TenantKey = "t1" };
        var context1 = new DefaultHttpContext();
        context1.SetTenantContext(new TenantContext(tenant));
        var context2 = new DefaultHttpContext();
        context2.SetTenantContext(new TenantContext(tenant));

        await pipeline(context1);
        await pipeline(context2);

        builds.Should().Be(1, "o branch é construído uma única vez por tenant");
    }
}

public class MultitenancyApplicationBuilderExtensionsTests
{
    [Fact]
    public async Task UseMultitenancy_ResolveEGravaOTenantNoContexto()
    {
        var tenantContext = new TenantContext(new TestTenant { TenantKey = "t1" });
        var resolver = new Mock<ITenantResolver>();
        resolver.Setup(r => r.ResolveAsync(It.IsAny<HttpContext>())).ReturnsAsync(tenantContext);

        var services = new ServiceCollection()
            .AddScoped(_ => resolver.Object)
            .BuildServiceProvider();

        var app = new ApplicationBuilder(services);
        app.UseMultitenancy();
        app.Run(_ => Task.CompletedTask);
        var pipeline = app.Build();

        var httpContext = new DefaultHttpContext { RequestServices = services };
        await pipeline(httpContext);

        httpContext.GetTenantContext().Should().BeSameAs(tenantContext);
    }
}
