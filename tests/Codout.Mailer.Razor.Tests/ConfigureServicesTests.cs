using System.Reflection;
using Codout.Mailer.Interfaces;
using Codout.Mailer.Razor;
using Codout.Mailer.Razor.Configuration;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Codout.Mailer.Razor.Tests;

public class ConfigureServicesTests
{
    [Fact]
    public void AddMailerRazor_SemTemplateAssembly_LancaInvalidOperationException()
    {
        var acao = () => new ServiceCollection().AddMailerRazor(options =>
        {
            options.RootNamespace = "X";
        });

        acao.Should().Throw<InvalidOperationException>()
            .WithMessage("TemplateAssembly deve ser definido*");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void AddMailerRazor_SemRootNamespace_LancaInvalidOperationException(string? rootNamespace)
    {
        var acao = () => new ServiceCollection().AddMailerRazor(options =>
        {
            options.TemplateAssembly = typeof(ConfigureServicesTests).Assembly;
            options.RootNamespace = rootNamespace!;
        });

        acao.Should().Throw<InvalidOperationException>()
            .WithMessage("RootNamespace deve ser definido*");
    }

    [Fact]
    public void AddMailerRazor_ComOpcoesValidas_DeveRegistrarTemplateEngineScoped()
    {
        var services = new ServiceCollection();

        services.AddMailerRazor(options =>
        {
            options.TemplateAssembly = typeof(ConfigureServicesTests).Assembly;
            options.RootNamespace = "Codout.Mailer.Razor.Tests";
        });

        services.Should().ContainSingle(d =>
            d.ServiceType == typeof(ITemplateEngine) &&
            d.ImplementationType == typeof(RazorViewTemplateEngine) &&
            d.Lifetime == ServiceLifetime.Scoped);
    }

    [Fact]
    public void RazorMailerOptions_DeveTerCacheHabilitadoPorPadrao()
    {
        var options = new RazorMailerOptions();

        options.EnableCache.Should().BeTrue();
        options.TemplateAssembly.Should().BeNull();
        options.RootNamespace.Should().BeNull();
    }

    [Fact]
    public void TemplateDeveEstarEmbarcadoNoAssemblyDeTeste()
    {
        // Sanidade: garante que o recurso embarcado usado nos testes de integração
        // existe com o nome esperado pelo EmbeddedFileProvider.
        var nomes = typeof(ConfigureServicesTests).Assembly.GetManifestResourceNames();

        nomes.Should().Contain("Codout.Mailer.Razor.Tests.Templates.Welcome.cshtml");
    }
}
