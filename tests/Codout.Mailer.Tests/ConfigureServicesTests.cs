using Codout.Mailer.Configuration;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using Xunit;

namespace Codout.Mailer.Tests;

public class ConfigureServicesTests
{
    private static IConfiguration BuildConfiguration(IDictionary<string, string?>? values = null)
    {
        return new ConfigurationBuilder()
            .AddInMemoryCollection(values ?? new Dictionary<string, string?>())
            .Build();
    }

    [Fact]
    public void AddMailer_DeveBindarMailerSettingsDaConfiguracao()
    {
        var configuration = BuildConfiguration(new Dictionary<string, string?>
        {
            ["MailerSettings:DefaultFromName"] = "Codout",
            ["MailerSettings:DefaultFromEmail"] = "noreply@codout.com"
        });

        var provider = new ServiceCollection()
            .AddLogging()
            .AddMailer(configuration)
            .BuildServiceProvider();

        var settings = provider.GetRequiredService<IOptions<MailerSettings>>().Value;

        settings.DefaultFromName.Should().Be("Codout");
        settings.DefaultFromEmail.Should().Be("noreply@codout.com");
    }

    [Fact]
    public void AddMailer_DeveRegistrarHealthCheckComNomeMailer()
    {
        var provider = new ServiceCollection()
            .AddLogging()
            .AddMailer(BuildConfiguration())
            .BuildServiceProvider();

        var healthOptions = provider.GetRequiredService<IOptions<HealthCheckServiceOptions>>().Value;

        healthOptions.Registrations.Should().ContainSingle(r => r.Name == "mailer");
    }

    [Fact]
    public void AddMailer_DeveAplicarDelegateDeOpcoesSobreAConfiguracao()
    {
        var delegateInvocado = false;

        var provider = new ServiceCollection()
            .AddLogging()
            .AddMailer(BuildConfiguration(), _ => delegateInvocado = true)
            .BuildServiceProvider();

        _ = provider.GetRequiredService<IOptions<MailerOptions>>().Value;

        delegateInvocado.Should().BeTrue();
    }

    [Fact]
    public async Task MailerHealthCheck_DeveRetornarHealthy()
    {
        // MailerHealthCheck está no namespace global e não usa o dispatcher
        // (campo _dispatcher nunca é atribuído) — sempre retorna Healthy.
        var healthCheck = new MailerHealthCheck();

        var resultado = await healthCheck.CheckHealthAsync(new HealthCheckContext());

        resultado.Status.Should().Be(HealthStatus.Healthy);
        resultado.Description.Should().Be("Mailer service is healthy");
    }
}
