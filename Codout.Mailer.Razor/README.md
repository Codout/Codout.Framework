# Codout.Mailer.Razor

Implementação de `ITemplateEngine` para a biblioteca [Codout.Mailer](https://www.nuget.org/packages/Codout.Mailer) usando a engine Razor nativa do ASP.NET Core (classe `RazorViewTemplateEngine`), com templates embarcados como recursos do assembly.

## Instalação

```bash
dotnet add package Codout.Mailer.Razor
```

Requer .NET 10 (`net10.0`) e o framework `Microsoft.AspNetCore.App`.

## Uso

Registre o template engine com a extensão `AddMailerRazor`, depois de `AddMailer()` (ou de `AddMailerWithSendGrid`/`AddMailerWithAws`). As opções `TemplateAssembly` e `RootNamespace` de `RazorMailerOptions` são obrigatórias:

```csharp
using Codout.Mailer.Razor.Configuration;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMailerWithSendGrid(builder.Configuration);

builder.Services.AddMailerRazor(options =>
{
    options.TemplateAssembly = typeof(Program).Assembly;
    options.RootNamespace = "MeuProjeto.Templates";
    options.EnableCache = true; // padrão: true
});
```

Os templates `.cshtml` devem ser embarcados no assembly indicado:

```xml
<ItemGroup>
  <EmbeddedResource Include="Templates\*.cshtml" />
</ItemGroup>
```

A renderização é feita por `RenderAsync<T>(string templateKey, T model)` — chamada automaticamente por `MailerServiceBase.Send`, mas também utilizável de forma direta:

```csharp
using Codout.Mailer.Interfaces;

public class PreviaService(ITemplateEngine templateEngine)
{
    public Task<string> GerarHtmlAsync(BoasVindasModel model) =>
        templateEngine.RenderAsync("BoasVindas", model);
}
```
Se o template não for encontrado, é lançada `InvalidOperationException` listando os locais pesquisados.

## Pacotes relacionados

- [Codout.Mailer](https://www.nuget.org/packages/Codout.Mailer) — núcleo (`IMailerService`, `IMailerDispatcher`, `ITemplateEngine`, `MailerServiceBase`).
- [Codout.Mailer.SendGrid](https://www.nuget.org/packages/Codout.Mailer.SendGrid) — dispatcher via SendGrid.
- [Codout.Mailer.AWS](https://www.nuget.org/packages/Codout.Mailer.AWS) — dispatcher via Amazon SES.

---
Parte do [Codout.Framework](https://github.com/Codout/Codout.Framework) — licença MIT.
