using System.Net.Mail;
using Codout.Mailer.Configuration;
using Codout.Mailer.Diagnostics;
using Codout.Mailer.Models;
using FluentAssertions;
using Xunit;

namespace Codout.Mailer.Tests;

public class ModelsTests
{
    [Fact]
    public void MailerResponse_PorPadrao_NaoEnviadoESemErros()
    {
        var resposta = new MailerResponse();

        resposta.Sent.Should().BeFalse();

        // Observação: ErrorMessages não é inicializada — consumidores precisam
        // checar null antes de iterar (em respostas de sucesso ela costuma vir null).
        resposta.ErrorMessages.Should().BeNull();
    }

    [Fact]
    public void MailerModelBase_DevePermitirDefinirDestinatario()
    {
        var model = new MailerModelBase { To = new MailAddress("a@b.com", "Nome") };

        model.To.Address.Should().Be("a@b.com");
        model.To.DisplayName.Should().Be("Nome");
    }

    [Fact]
    public void MailerSettings_SectionName_DeveSerMailerSettings()
    {
        MailerSettings.SectionName.Should().Be("MailerSettings");
    }

    [Fact]
    public void MailerOptions_Validate_NaoDeveLancar()
    {
        var acao = () => new MailerOptions().Validate();

        acao.Should().NotThrow();
    }

    [Fact]
    public void MailerActivitySource_DeveExporNomeEVersao()
    {
        MailerActivitySource.ActivitySourceName.Should().Be("Codout.Mailer");
        MailerActivitySource.Version.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void MailerActivitySource_SemListeners_StartActivityRetornaNull()
    {
        var activity = MailerActivitySource.StartActivity("Teste.SemListener");

        activity.Should().BeNull();
    }
}
