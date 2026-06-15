# Codout.Mailer.SendGrid

Dispatcher de e-mails via SendGrid para a biblioteca [Codout.Mailer](https://www.nuget.org/packages/Codout.Mailer), implementando `IMailerDispatcher` com a classe `SendGridDispatcher`.

## Instalação

```bash
dotnet add package Codout.Mailer.SendGrid
```

Requer .NET 10 (`net10.0`).

## Uso

Registre o mailer com o dispatcher do SendGrid usando a extensão `AddMailerWithSendGrid` (que já chama `AddMailer` internamente e registra `SendGridDispatcher` como `IMailerDispatcher`):

```csharp
using Codout.Mailer.SendGrid.Configuration;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMailerWithSendGrid(builder.Configuration);
```

Configure a chave de API no `appsettings.json`, na seção `SendGridSettings` (classe `SendGridSettings`, propriedade `ApiKey`):

```json
{
  "MailerSettings": {
    "DefaultFromName": "Minha Empresa",
    "DefaultFromEmail": "noreply@minhaempresa.com"
  },
  "SendGridSettings": {
    "ApiKey": "SG.sua-api-key"
  }
}
```

O envio normalmente é feito por um serviço que herda de `MailerServiceBase` (veja o README do Codout.Mailer), mas o dispatcher também pode ser injetado e usado diretamente via `IMailerDispatcher.Send(from, to, subject, htmlContent, plainTextContent, attachments)`, que retorna um `MailerResponse` com `Sent` (verdadeiro quando o SendGrid responde `202 Accepted`) e `ErrorMessages`. Anexos (`System.Net.Mail.Attachment[]`) são convertidos para Base64 e adicionados à mensagem.

## Pacotes relacionados

- [Codout.Mailer](https://www.nuget.org/packages/Codout.Mailer) — núcleo (`IMailerService`, `IMailerDispatcher`, `ITemplateEngine`, `MailerServiceBase`).
- [Codout.Mailer.Razor](https://www.nuget.org/packages/Codout.Mailer.Razor) — template engine Razor para renderizar os e-mails.
- [Codout.Mailer.AWS](https://www.nuget.org/packages/Codout.Mailer.AWS) — dispatcher alternativo via Amazon SES.

---
Parte do [Codout.Framework](https://github.com/Codout/Codout.Framework) — licença MIT.
