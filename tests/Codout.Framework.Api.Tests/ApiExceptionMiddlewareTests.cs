using System.Text;
using System.Text.Json;
using Codout.Framework.Api.Middleware;
using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Codout.Framework.Api.Tests;

public class ApiExceptionMiddlewareTests
{
    private static DefaultHttpContext CreateContext()
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        return context;
    }

    private static string ReadBody(HttpContext context)
    {
        context.Response.Body.Seek(0, SeekOrigin.Begin);
        return new StreamReader(context.Response.Body, Encoding.UTF8).ReadToEnd();
    }

    [Fact]
    public async Task QuandoNaoHaExcecao_DeixaORequestPassar()
    {
        var nextCalled = false;
        var middleware = new ApiExceptionMiddleware(
            _ =>
            {
                nextCalled = true;
                return Task.CompletedTask;
            },
            NullLogger<ApiExceptionMiddleware>.Instance);

        var context = CreateContext();
        await middleware.InvokeAsync(context);

        nextCalled.Should().BeTrue();
        ReadBody(context).Should().BeEmpty();
    }

    [Fact]
    public async Task QuandoHaExcecao_EscreveApiExceptionComoJson()
    {
        var middleware = new ApiExceptionMiddleware(
            _ => throw new InvalidOperationException("algo deu errado"),
            NullLogger<ApiExceptionMiddleware>.Instance);

        var context = CreateContext();
        await middleware.InvokeAsync(context);

        context.Response.ContentType.Should().StartWith("application/json");

        using var document = JsonDocument.Parse(ReadBody(context));
        document.RootElement.GetProperty("message").GetString().Should().Be("algo deu errado");
        document.RootElement.GetProperty("errors")[0].GetProperty("errorMessage").GetString()
            .Should().Be("algo deu errado");
    }

    [Fact]
    public async Task QuandoHaExcecao_StatusCodePermanece200()
    {
        // BUG?: o middleware nunca altera Response.StatusCode — ele serializa o status
        // vigente (200) dentro do corpo e responde `200 OK` para qualquer exceção não
        // tratada, em vez de 500.
        var middleware = new ApiExceptionMiddleware(
            _ => throw new InvalidOperationException("falha interna"),
            NullLogger<ApiExceptionMiddleware>.Instance);

        var context = CreateContext();
        await middleware.InvokeAsync(context);

        context.Response.StatusCode.Should().Be(StatusCodes.Status200OK);

        using var document = JsonDocument.Parse(ReadBody(context));
        document.RootElement.GetProperty("statusCode").GetInt32().Should().Be(200);
    }

    [Fact]
    public async Task ConfigureExceptionMiddleware_RegistraOMiddlewareNoPipeline()
    {
        var services = new ServiceCollection()
            .AddLogging()
            .BuildServiceProvider();

        var app = new ApplicationBuilder(services);
        app.ConfigureExceptionMiddleware();
        app.Run(_ => throw new ApplicationException("estourou no pipeline"));
        var pipeline = app.Build();

        var context = CreateContext();
        context.RequestServices = services;
        await pipeline(context);

        using var document = JsonDocument.Parse(ReadBody(context));
        document.RootElement.GetProperty("message").GetString().Should().Be("estourou no pipeline");
    }
}
