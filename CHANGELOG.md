# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [6.3.0] - 2026-05-12

### Fixed
- **Codout.Framework.EF** — `EFRepository.SaveOrUpdate` / `SaveOrUpdateAsync` decidiam insert vs. update via `IEntity.IsTransient()`, o que quebrava qualquer entidade que pré-atribuísse o `Id` no construtor (ex.: `Id = Guid.NewGuid()`). A entidade era marcada como `Modified` e o `SaveChanges` falhava ao tentar `UPDATE` em uma linha inexistente. Agora a decisão é feita inspecionando `Context.Entry(entity).State` e, para entidades `Detached` com chave atribuída, consultando o banco via `Find` / `FindAsync`.
- **Codout.Framework.EF** — `SaveOrUpdateAsync` deixou de bloquear a thread chamando `Find` síncrono; agora usa `FindAsync` e respeita o `CancellationToken` recebido.
- **Codout.Framework.EF** — Quando o `SaveOrUpdate` precisa atualizar uma entidade existente, os valores são copiados via `Entry(existing).CurrentValues.SetValues(entity)`, preservando os `OriginalValues` (e portanto tokens de concorrência como `[Timestamp]`/`RowVersion`) trazidos do banco.

### Changed
- **Codout.Framework.EF** — `SaveOrUpdate` em entidade já rastreada com estado `Unchanged` não força mais a transição para `Modified`. Confia no change tracker para detectar mutações. Consumidores que dependiam do "force full update" devem chamar `Update(entity)` explicitamente.
- **Codout.Framework.EF** — `SaveOrUpdate` em entidade `Detached` com chave não-default agora executa um roundtrip ao banco (`Find` / `FindAsync`) para distinguir insert de update. Em hot paths onde a intenção é conhecida, prefira `Save(entity)` ou `Update(entity)` para evitar a query.

## [6.2.0] - 2025-07-17

### Added
- **Codout.Mailer.Razor** — novo pacote satélite que implementa `ITemplateEngine` usando a engine Razor nativa do ASP.NET Core (`IRazorViewEngine` + `RuntimeCompilation` + `EmbeddedFileProvider`).
- Extension method `AddMailerRazor()` para registro do template engine Razor via DI.
- Classe `RazorMailerOptions` com opções `TemplateAssembly`, `RootNamespace` e `EnableCache`.

### Changed
- **Codout.Mailer** — o core agora é agnóstico ao template engine. A interface `ITemplateEngine` permanece, mas nenhuma implementação é registrada automaticamente.
- `AddMailer()` não registra mais `ITemplateEngine` por padrão — o consumidor deve registrar via `AddMailerRazor()` ou implementação própria.
- `MailerOptions` simplificado — removidas propriedades `TemplateRootType`, `RazorLight` e a classe `RazorLightOptions`.
- Dependências `Microsoft.Extensions.*` agora são referenciadas explicitamente no `Codout.Mailer.csproj` (antes eram transitivas via RazorLight).

### Removed
- **RazorLight** — removida dependência do pacote `RazorLight 2.3.1` do core `Codout.Mailer`.
- Classe `RazorTemplateEngine` removida do core (substituída por `RazorViewTemplateEngine` no pacote `Codout.Mailer.Razor`).
- Classe `RazorLightOptions` removida do core.

### Migration Guide

**Antes (v6.x):**
```csharp
services.AddMailer(configuration, options =>
{
    options.TemplateRootType = typeof(Program);
    options.RazorLight.DefaultNamespace = "MyApp.Templates";
});
```

**Depois (v7.0):**
```csharp
services.AddMailer(configuration);
services.AddMailerRazor(options =>
{
    options.TemplateAssembly = typeof(Program).Assembly;
    options.RootNamespace = "MyApp.Templates";
});
```

## [6.0.1] - 2025-07-01

- Previous release targeting .NET 10 with RazorLight-based template engine.
