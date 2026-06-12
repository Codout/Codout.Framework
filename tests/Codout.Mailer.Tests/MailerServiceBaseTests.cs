using System.Net.Mail;
using Codout.Mailer.Configuration;
using Codout.Mailer.Interfaces;
using Codout.Mailer.Models;
using Codout.Mailer.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Codout.Mailer.Tests;

public class MailerServiceBaseTests
{
    private const string FromEmail = "noreply@codout.com";
    private const string FromName = "Codout";

    private readonly Mock<IMailerDispatcher> _dispatcher = new();
    private readonly Mock<ITemplateEngine> _templateEngine = new();

    private sealed class TestModel : MailerModelBase
    {
        public string Nome { get; set; } = string.Empty;
    }

    private sealed class TestMailerService(
        IOptions<MailerSettings> mailerSettings,
        IMailerDispatcher dispatcher,
        ITemplateEngine templateEngine)
        : MailerServiceBase(mailerSettings, dispatcher, templateEngine, NullLogger<MailerServiceBase>.Instance);

    private TestMailerService CreateService()
    {
        var settings = Options.Create(new MailerSettings
        {
            DefaultFromEmail = FromEmail,
            DefaultFromName = FromName
        });

        return new TestMailerService(settings, _dispatcher.Object, _templateEngine.Object);
    }

    [Fact]
    public async Task Send_DeveRenderizarTemplateEDespacharComRemetentePadrao()
    {
        var model = new TestModel { To = new MailAddress("dest@exemplo.com", "Destinatário"), Nome = "Fulano" };

        _templateEngine
            .Setup(t => t.RenderAsync("welcome", model))
            .ReturnsAsync("<p>Olá Fulano</p>");

        MailAddress? capturedFrom = null;
        MailAddress? capturedTo = null;
        string? capturedSubject = null;
        string? capturedHtml = null;
        string? capturedPlainText = null;

        _dispatcher
            .Setup(d => d.Send(It.IsAny<MailAddress>(), It.IsAny<MailAddress>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Attachment[]>()))
            .Callback<MailAddress, MailAddress, string, string, string, Attachment[]>(
                (from, to, subject, html, plain, _) =>
                {
                    capturedFrom = from;
                    capturedTo = to;
                    capturedSubject = subject;
                    capturedHtml = html;
                    capturedPlainText = plain;
                })
            .ReturnsAsync(new MailerResponse { Sent = true });

        var resultado = await CreateService().Send("welcome", model, "Bem-vindo");

        resultado.Sent.Should().BeTrue();
        capturedFrom!.Address.Should().Be(FromEmail);
        capturedFrom.DisplayName.Should().Be(FromName);
        capturedTo.Should().BeSameAs(model.To);
        capturedSubject.Should().Be("Bem-vindo");
        capturedHtml.Should().Be("<p>Olá Fulano</p>");
        capturedPlainText.Should().Contain("Olá Fulano").And.NotContain("<p>");
    }

    [Fact]
    public async Task Send_DeveRepassarAttachmentsParaODispatcher()
    {
        var model = new TestModel { To = new MailAddress("dest@exemplo.com") };
        var attachments = new[] { new Attachment(new MemoryStream([1, 2, 3]), "arquivo.bin") };

        _templateEngine.Setup(t => t.RenderAsync("tpl", model)).ReturnsAsync("<p>x</p>");
        _dispatcher
            .Setup(d => d.Send(It.IsAny<MailAddress>(), It.IsAny<MailAddress>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<string>(), attachments))
            .ReturnsAsync(new MailerResponse { Sent = true })
            .Verifiable();

        var resultado = await CreateService().Send("tpl", model, "Assunto", attachments);

        resultado.Sent.Should().BeTrue();
        _dispatcher.Verify();
    }

    [Fact]
    public async Task Send_QuandoTemplateEngineLanca_DeveRetornarRespostaComErro()
    {
        var model = new TestModel { To = new MailAddress("dest@exemplo.com") };

        _templateEngine
            .Setup(t => t.RenderAsync("tpl", model))
            .ThrowsAsync(new InvalidOperationException("template não encontrado"));

        var resultado = await CreateService().Send("tpl", model, "Assunto");

        resultado.Sent.Should().BeFalse();
        resultado.ErrorMessages.Should().ContainSingle().Which.Should().Be("template não encontrado");
        _dispatcher.Verify(d => d.Send(It.IsAny<MailAddress>(), It.IsAny<MailAddress>(), It.IsAny<string>(),
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Attachment[]>()), Times.Never);
    }

    [Fact]
    public async Task Send_QuandoDispatcherLanca_DeveRetornarRespostaComErro()
    {
        var model = new TestModel { To = new MailAddress("dest@exemplo.com") };

        _templateEngine.Setup(t => t.RenderAsync("tpl", model)).ReturnsAsync("<p>x</p>");
        _dispatcher
            .Setup(d => d.Send(It.IsAny<MailAddress>(), It.IsAny<MailAddress>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Attachment[]>()))
            .ThrowsAsync(new Exception("falha no envio"));

        var resultado = await CreateService().Send("tpl", model, "Assunto");

        resultado.Sent.Should().BeFalse();
        resultado.ErrorMessages.Should().ContainSingle().Which.Should().Be("falha no envio");
    }

    [Fact]
    public async Task Send_ComModelSemDestinatario_LancaNullReference()
    {
        // BUG?: model.To.Address é acessado no log ANTES do bloco try, então um model
        // sem destinatário derruba o chamador com NullReferenceException em vez de
        // retornar MailerResponse { Sent = false }. Teste de caracterização.
        var model = new TestModel { To = null! };

        var acao = () => CreateService().Send("tpl", model, "Assunto");

        await acao.Should().ThrowAsync<NullReferenceException>();
    }
}
