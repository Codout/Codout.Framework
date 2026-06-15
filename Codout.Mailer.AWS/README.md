# Codout.Mailer.AWS

Dispatcher de e-mails via Amazon SES (Simple Email Service v2) para a biblioteca [Codout.Mailer](https://www.nuget.org/packages/Codout.Mailer), implementando `IMailerDispatcher` com a classe `AWSDispatcher`.

## Instalação

```bash
dotnet add package Codout.Mailer.AWS
```

Requer .NET 10 (`net10.0`).

## Uso

Registre o mailer com o dispatcher do SES usando a extensão `AddMailerWithAws` (que já chama `AddMailer` internamente e registra `AWSDispatcher` como `IMailerDispatcher`):

```csharp
using Codout.Mailer.AWS.Configuration;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMailerWithAws(builder.Configuration);
```

Configure as credenciais no `appsettings.json`, na seção `AWSSettings` (classe `AWSSettings`, propriedades `RegionEndpoint`, `AccessKey` e `SecretKey` — todas obrigatórias):

```json
{
  "MailerSettings": {
    "DefaultFromName": "Minha Empresa",
    "DefaultFromEmail": "noreply@minhaempresa.com"
  },
  "AWSSettings": {
    "RegionEndpoint": "us-east-1",
    "AccessKey": "sua-access-key",
    "SecretKey": "sua-secret-key"
  }
}
```

O envio normalmente é feito por um serviço que herda de `MailerServiceBase` (veja o README do Codout.Mailer), mas o dispatcher também pode ser injetado e usado diretamente via `IMailerDispatcher.Send(from, to, subject, htmlContent, plainTextContent, attachments)`, que retorna um `MailerResponse` com `Sent` e `ErrorMessages`. Anexos (`System.Net.Mail.Attachment[]`) são montados via MimeKit e enviados como mensagem raw ao SES.

## Pacotes relacionados

- [Codout.Mailer](https://www.nuget.org/packages/Codout.Mailer) — núcleo (`IMailerService`, `IMailerDispatcher`, `ITemplateEngine`, `MailerServiceBase`).
- [Codout.Mailer.Razor](https://www.nuget.org/packages/Codout.Mailer.Razor) — template engine Razor para renderizar os e-mails.
- [Codout.Mailer.SendGrid](https://www.nuget.org/packages/Codout.Mailer.SendGrid) — dispatcher alternativo via SendGrid.

---
Parte do [Codout.Framework](https://github.com/Codout/Codout.Framework) — licença MIT.
