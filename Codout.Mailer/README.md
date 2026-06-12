# Codout.Mailer

Núcleo de envio de e-mails para .NET: define os contratos `IMailerService`, `IMailerDispatcher` e `ITemplateEngine`, a classe base `MailerServiceBase` e o registro de configuração e health check via `AddMailer`.

## Instalação

```bash
dotnet add package Codout.Mailer
```

## Uso

Registre as configurações com `AddMailer` (lê a seção `MailerSettings`, com `DefaultFromName` e `DefaultFromEmail`):

```csharp
using Codout.Mailer.Configuration;

builder.Services.AddMailer(builder.Configuration);
```

Crie seu serviço herdando de `MailerServiceBase`, que renderiza o template via `ITemplateEngine`, converte o HTML em texto plano e envia pelo `IMailerDispatcher` registrado:

```csharp
using Codout.Mailer.Models;
using Codout.Mailer.Services;

public class BoasVindasModel : MailerModelBase
{
    public string Nome { get; set; }
}

public class MeuMailerService(
    IOptions<MailerSettings> settings,
    IMailerDispatcher dispatcher,
    ITemplateEngine templateEngine,
    ILogger<MailerServiceBase> logger)
    : MailerServiceBase(settings, dispatcher, templateEngine, logger);

MailerResponse resposta = await mailerService.Send("BoasVindas", model, "Bem-vindo!");
```

Este pacote não envia e-mails sozinho: é necessário registrar uma implementação de `IMailerDispatcher` (provedor) e de `ITemplateEngine` (renderização de templates).

## Pacotes relacionados

- `Codout.Mailer.SendGrid` — dispatcher para SendGrid.
- `Codout.Mailer.AWS` — dispatcher para Amazon SES.
- `Codout.Mailer.Razor` — template engine Razor para os e-mails.

Parte do [Codout.Framework](https://github.com/Codout/Codout.Framework) — licença MIT.
