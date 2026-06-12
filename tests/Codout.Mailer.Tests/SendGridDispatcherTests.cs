using Codout.Mailer.Interfaces;
using Codout.Mailer.SendGrid;
using Codout.Mailer.SendGrid.Configuration;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;

namespace Codout.Mailer.Tests;

/// <summary>
/// O SendGridDispatcher instancia o SendGridClient internamente (não injetável),
/// então não é possível testar a montagem do SendGridMessage nem o tratamento da
/// resposta HTTP sem chamada de rede real (o SDK tem ctor que recebe HttpClient,
/// mas o dispatcher não o expõe). Aqui são testados o registro de DI e o binding
/// das settings. Veja tests/FINDINGS-C.md.
/// </summary>
public class SendGridDispatcherTests
{
    [Fact]
    public void AddMailerWithSendGrid_DeveRegistrarSendGridDispatcherComoIMailerDispatcher()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["SendGridSettings:ApiKey"] = "SG.test-key",
                ["SendGridSettings:StandBox"] = "true"
            })
            .Build();

        var provider = new ServiceCollection()
            .AddLogging()
            .AddMailerWithSendGrid(configuration)
            .BuildServiceProvider();

        using var scope = provider.CreateScope();

        scope.ServiceProvider.GetRequiredService<IMailerDispatcher>()
            .Should().BeOfType<SendGridDispatcher>();

        var settings = scope.ServiceProvider.GetRequiredService<IOptions<SendGridSettings>>().Value;
        settings.ApiKey.Should().Be("SG.test-key");

        // Observação: a propriedade chama-se "StandBox" (provável typo de "Sandbox")
        // e não é utilizada em nenhum lugar do dispatcher.
        settings.StandBox.Should().BeTrue();
    }

    [Fact]
    public void SendGridSettings_SectionName_DeveSerSendGridSettings()
    {
        SendGridSettings.SectionName.Should().Be("SendGridSettings");
    }

    [Fact]
    public void SendGridDispatcher_DeveSerConstruivelComSettingsVazias()
    {
        // O dispatcher não valida ApiKey na construção (validação só ocorreria
        // na chamada de rede, que não é exercitada em testes unitários).
        var dispatcher = new SendGridDispatcher(Options.Create(new SendGridSettings()));

        dispatcher.Should().NotBeNull();
    }
}
