using System.Diagnostics;
using System.Net.Mail;
using Codout.Mailer.Interfaces;
using Codout.Mailer.Razor.Configuration;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace Codout.Mailer.Razor.Tests;

/// <summary>
/// Renderização Razor REAL (compilação em runtime) sem host ASP.NET Core completo:
/// monta-se um ServiceProvider mínimo com IWebHostEnvironment/DiagnosticListener
/// fakes e usa-se o AddMailerRazor do pacote com o template embarcado neste
/// assembly de teste (requer PreserveCompilationContext no csproj de teste).
/// </summary>
public class RazorRenderingIntegrationTests
{
    private static ServiceProvider BuildProvider()
    {
        var services = new ServiceCollection();

        var environment = new FakeWebHostEnvironment();
        services.AddSingleton<IWebHostEnvironment>(environment);
        services.AddSingleton<IHostEnvironment>(environment);

        var diagnosticListener = new DiagnosticListener("Microsoft.AspNetCore");
        services.AddSingleton<DiagnosticSource>(diagnosticListener);
        services.AddSingleton(diagnosticListener);

        services.AddLogging();

        services.AddMailerRazor(options =>
        {
            options.TemplateAssembly = typeof(RazorRenderingIntegrationTests).Assembly;
            options.RootNamespace = "Codout.Mailer.Razor.Tests";
        });

        return services.BuildServiceProvider();
    }

    [Fact]
    public async Task RenderAsync_DeveCompilarERenderizarTemplateEmbarcadoComModel()
    {
        await using var provider = BuildProvider();
        using var scope = provider.CreateScope();
        var engine = scope.ServiceProvider.GetRequiredService<ITemplateEngine>();

        var model = new WelcomeModel
        {
            Nome = "Maria",
            To = new MailAddress("maria@exemplo.com")
        };

        var html = await engine.RenderAsync("/Templates/Welcome.cshtml", model);

        html.Should().Contain("Olá, Maria!");
        html.Should().Contain("maria@exemplo.com");
        html.Should().Contain("<html>");
    }

    [Fact]
    public async Task RenderAsync_TemplateInexistente_LancaInvalidOperationException()
    {
        await using var provider = BuildProvider();
        using var scope = provider.CreateScope();
        var engine = scope.ServiceProvider.GetRequiredService<ITemplateEngine>();

        var acao = () => engine.RenderAsync("/Templates/NaoExiste.cshtml", new WelcomeModel());

        (await acao.Should().ThrowAsync<InvalidOperationException>())
            .WithMessage("*'/Templates/NaoExiste.cshtml'*não encontrado*");
    }

    private sealed class FakeWebHostEnvironment : IWebHostEnvironment
    {
        public string ApplicationName { get; set; } =
            typeof(RazorRenderingIntegrationTests).Assembly.GetName().Name!;

        public string EnvironmentName { get; set; } = "Production";
        public string ContentRootPath { get; set; } = AppContext.BaseDirectory;
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
        public string WebRootPath { get; set; } = AppContext.BaseDirectory;
        public IFileProvider WebRootFileProvider { get; set; } = new NullFileProvider();
    }
}
