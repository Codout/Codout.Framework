using Codout.Mailer.AWS;
using Codout.Mailer.AWS.Configuration;
using Codout.Mailer.Interfaces;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;

namespace Codout.Mailer.Tests;

/// <summary>
/// O AWSDispatcher instancia o AmazonSimpleEmailServiceV2Client internamente
/// (não injetável), então não é possível testar a montagem da mensagem nem o
/// tratamento de resposta sem chamada de rede real. Aqui são testadas as
/// validações de configuração (que ocorrem antes de qualquer I/O) e o registro
/// no container de DI. Veja tests/FINDINGS-C.md.
/// </summary>
public class AwsDispatcherTests
{
    private static AWSDispatcher CreateDispatcher(string? accessKey, string? secretKey, string? region)
    {
        return new AWSDispatcher(Options.Create(new AWSSettings
        {
            AccessKey = accessKey!,
            SecretKey = secretKey!,
            RegionEndpoint = region!
        }));
    }

    private static System.Net.Mail.MailAddress Address(string email) => new(email);

    [Fact]
    public async Task Send_SemAccessKey_LancaInvalidOperationException()
    {
        // BUG?: a validação de configuração lança exceção, enquanto qualquer outra
        // falha dentro do try é convertida em MailerResponse { Sent = false } —
        // comportamento inconsistente para o chamador. Caracterização do atual.
        var dispatcher = CreateDispatcher(null, "secret", "us-east-1");

        var acao = () => dispatcher.Send(Address("from@x.com"), Address("to@x.com"), "s", "<p>x</p>");

        (await acao.Should().ThrowAsync<InvalidOperationException>())
            .WithMessage("AWS Access Key is not configured.");
    }

    [Fact]
    public async Task Send_SemRegionEndpoint_LancaInvalidOperationException()
    {
        var dispatcher = CreateDispatcher("access", "secret", "  ");

        var acao = () => dispatcher.Send(Address("from@x.com"), Address("to@x.com"), "s", "<p>x</p>");

        (await acao.Should().ThrowAsync<InvalidOperationException>())
            .WithMessage("AWS Region Endpoint is not configured.");
    }

    [Fact]
    public async Task Send_SemSecretKey_LancaInvalidOperationException()
    {
        var dispatcher = CreateDispatcher("access", "", "us-east-1");

        var acao = () => dispatcher.Send(Address("from@x.com"), Address("to@x.com"), "s", "<p>x</p>");

        (await acao.Should().ThrowAsync<InvalidOperationException>())
            .WithMessage("AWS Secret Key is not configured.");
    }

    [Fact]
    public async Task Send_ComTodasSettingsVazias_ValidaAccessKeyPrimeiro()
    {
        var dispatcher = CreateDispatcher(null, null, null);

        var acao = () => dispatcher.Send(Address("from@x.com"), Address("to@x.com"), "s", "<p>x</p>");

        (await acao.Should().ThrowAsync<InvalidOperationException>())
            .WithMessage("AWS Access Key is not configured.");
    }

    [Fact]
    public void AddMailerWithAws_DeveRegistrarAwsDispatcherComoIMailerDispatcher()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["AWSSettings:AccessKey"] = "AKIA-TEST",
                ["AWSSettings:SecretKey"] = "secret-test",
                ["AWSSettings:RegionEndpoint"] = "sa-east-1"
            })
            .Build();

        var provider = new ServiceCollection()
            .AddLogging()
            .AddMailerWithAws(configuration)
            .BuildServiceProvider();

        using var scope = provider.CreateScope();

        scope.ServiceProvider.GetRequiredService<IMailerDispatcher>()
            .Should().BeOfType<AWSDispatcher>();

        var settings = scope.ServiceProvider.GetRequiredService<IOptions<AWSSettings>>().Value;
        settings.AccessKey.Should().Be("AKIA-TEST");
        settings.SecretKey.Should().Be("secret-test");
        settings.RegionEndpoint.Should().Be("sa-east-1");
    }

    [Fact]
    public void AwsSettings_SectionName_DeveSerAWSSettings()
    {
        AWSSettings.SectionName.Should().Be("AWSSettings");
    }
}
