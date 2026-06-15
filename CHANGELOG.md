# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

A partir desta entrada cada pacote NuGet é versionado individualmente
(`<Version>` declarado em cada `.csproj` em vez de centralizado em
`Directory.Build.props`). As entradas abaixo identificam o pacote e a
versão afetados.

## 2026-06-15

> Bump **minor** coordenado de todos os pacotes publicáveis, preparando o
> release das melhorias descritas em 2026-06-12 (testes, nullable, SourceLink,
> READMEs, package validation, correção de vulnerabilidade do Mongo). **Ainda
> não publicado** — as tags só serão criadas após merge na master e revisão.
> O `PackageValidationBaselineVersion` de cada pacote permanece apontando para
> a última versão publicada (6.3.0 / 6.4.0), garantindo a checagem de
> compatibilidade no `pack`.

### Versionamento

- `6.3.0 → 6.4.0`: Api, Api.Client, Api.Dto, Application, Common, DynamicLinq, Image.Extensions, Mailer, Mailer.AWS, Mailer.Razor, Mailer.SendGrid, Mongo, Multitenancy, Softprime.Multitenancy, NH, Security.Core, Security.Argon2, Security.Bcrypt, Security.Scrypt, Storage, Storage.Azure.
- `6.4.0 → 6.5.0`: Data, Domain, EF.
- `Codout.Framework.Mcp` segue o próprio ciclo (`mcp-release.yml`) e não foi bumpado aqui.

## 2026-06-12

> Mudanças abaixo ainda **não publicadas** no NuGet.org — os bumps de versão
> acontecerão na próxima release de cada pacote.

### Tests

- Fase 4 do ROADMAP concluída: **921 testes** em 18 projetos sob `tests/`, cobrindo os 24 pacotes publicáveis (Common, Security, Image, Data, Domain, EF com SQLite, Mongo com mongod efêmero + replica set, NH com SQLite, Mailer + AWS/SendGrid/Razor com mocks e Razor real, Storage, Storage.Azure, DynamicLinq, Api.Dto, Application, Api.Client, Api, Multitenancy). Bugs pré-existentes encontrados foram **caracterizados** (testes documentam o comportamento atual, sem alterá-lo) e catalogados em `tests/FINDINGS-{A..E}.md` para triagem.

### Build

- `Directory.Build.props`: ligados globalmente `Nullable`, `TreatWarningsAsErrors`, analyzers .NET (`latest-recommended`), `GenerateDocumentationFile` (CS1591 suprimido até completar as docs por pacote), SourceLink (GitHub), símbolos `snupkg`, `PackageLicenseExpression MIT` e build determinístico em CI. `.editorconfig` calibra regras CA de estilo/perf como suggestion (subir severidade é trabalho incremental).
- **Package validation**: todos os 24 csproj publicáveis têm `EnablePackageValidation` + `PackageValidationBaselineVersion` apontando para a última versão no NuGet.org — o `pack` falha se a API pública quebrar vs. a baseline. A validação já reverteu duas quebras acidentais durante a anotação nullable (CP0001 em `LimitedList.Enumerator`, CP0021 em `JsonExtensions`).
- `Directory.Build.targets`: `README.md` de cada pasta de pacote é embarcado automaticamente como `PackageReadmeFile` no nupkg.
- Anotações **nullable** em todos os pacotes refletindo o contrato real (ex.: `IRepository<T>.GetAsync`/`LoadAsync` agora declaram `Task<T?>`). Sem mudança de comportamento — apenas metadados; consumidores com nullable habilitado podem ver warnings novos (e verdadeiros).

### Codout.Framework.Mongo (não publicado)

#### Fixed
- `MongoDB.Driver` 3.7.0 → 3.9.0, eliminando vulnerabilidades conhecidas nos transitivos `SharpCompress` 0.30.1 (moderada) e `Snappier` 1.0.0 (alta).

### Repository

- Removidas as árvores legadas `NetFull/`, `NetCore/`, `src/NetCore/` (Cosmos/DocumentDB em `netcoreapp2.0`, EOL), `Codout.Framework.DP` (quebrado, referenciava `Codout.Framework.DAL` inexistente), `Shared/Codout.Framework.Shared.Commom` e `Shared.msbuild`. Nenhum desses projetos era publicado no NuGet (`IsPackable=false` ou fora de `.github/release-packages.json`); o histórico permanece no git. O typo "Commom" deixa de existir no repositório.
- Removido `appveyor.yml` (referenciava Visual Studio 2012; o pipeline real é GitHub Actions).
- `Codout.Framework.Api.Dto` e `Softprime.Multitenancy` adicionados à `Codout.Framework.sln`; `Softprime.Multitenancy` passou a usar `obj`/`bin` isolados para coexistir com `Codout.Multitenancy` na mesma pasta sem corromper o restore.
- Testes movidos para a pasta `tests/` na raiz e incluídos na solution — `dotnet test Codout.Framework.sln` (CI de PR e gate do `release.yml`) agora executa os testes de verdade. `core-release.yml` atualizado para o novo caminho.

### Build

- `dotnet.yml`: SDK do CI atualizado de 9.0.x para 10.0.x, alinhado ao `TargetFramework` `net10.0` do `Directory.Build.props`.

## 2026-05-12

### Build

- **Repositório** — `<Version>` removido de `Directory.Build.props` e movido para cada `.csproj` publicável. Cada pacote agora evolui independentemente; alterações em um pacote não disparam mais um build novo dos demais.
- **Baseline alignment** — todos os pacotes publicáveis do `Codout.Framework` foram alinhados em `6.3.0` como ponto zero do esquema per-pacote. A partir daqui cada pacote diverge conforme suas mudanças reais. Pacotes que receberam alteração real nesta release: `Codout.Framework.EF` (fix do `SaveOrUpdate`, ver abaixo) e `Codout.Framework.Data` (bump técnico para satisfazer dependência do EF 6.3.0). Os outros 22 pacotes (`Codout.DynamicLinq`, `Codout.Framework.Api`, `Codout.Framework.Api.Client`, `Codout.Framework.Api.Dto`, `Codout.Framework.Application`, `Codout.Framework.Common`, `Codout.Framework.Domain`, `Codout.Framework.Mongo`, `Codout.Framework.NH`, `Codout.Framework.Storage`, `Codout.Framework.Storage.Azure`, `Codout.Image.Extensions`, `Codout.Mailer`, `Codout.Mailer.AWS`, `Codout.Mailer.Razor`, `Codout.Mailer.SendGrid`, `Codout.Multitenancy`, `Softprime.Multitenancy`, `Codout.Security.Argon2`, `Codout.Security.Bcrypt`, `Codout.Security.Core`, `Codout.Security.Scrypt`) sobem para `6.3.0` sem mudança de código ou contrato — alinhamento técnico para que toda referência cruzada no ecosystem tenha `>= 6.3.0` disponível como floor consistente. `Codout.Framework.Mcp` segue o próprio ciclo via `mcp-release.yml` e não foi incluído.
- **CI** — Workflows `release.yml` e `mcp-release.yml` passaram a usar **somente** `-p:PackageVersion=` no `dotnet pack`. O `-p:Version=` anterior cascateava para todos os projetos do build graph (incluindo `ProjectReference`), o que reescrevia silenciosamente a versão das dependências no `.nuspec` e produzia pacotes com referências quebradas — sintoma observado em `Codout.Framework.EF 6.3.0`, publicado declarando `Codout.Framework.Data >= 6.3.0` enquanto Data estava em 6.2.2 no csproj.
- **Repositório** — Pacotes `Codout.Framework.DP`, `Codout.Framework.NetCore.Repository.Cosmos` e `Codout.Framework.NetCore.Repository.DocumentDB` marcados com `<IsPackable>false</IsPackable>` e removidos de `.github/release-packages.json`. Os três estão abandonados (DP implementa um `IRepository<T>` antigo e referencia a pasta `Codout.Framework.DAL` que não existe; Cosmos/DocumentDB targetam `netcoreapp2.0` e referenciam projetos `NetStandard.*` ausentes). Mass-release agora ignora os três; consulte `CLAUDE.md` antes de tentar revivê-los.

### Codout.Framework.Data 6.3.0

#### Changed
- Bump de versão técnico para alinhar com `Codout.Framework.EF 6.3.0`. **Não há alterações de código ou contrato** em `Codout.Framework.Data` — esta versão existe para satisfazer a dependência `Codout.Framework.Data >= 6.3.0` declarada (incorretamente) pelo pacote EF 6.3.0 já publicado. Pacotes futuros do EF voltarão a depender da versão minor real de Data, agora que o workflow não cascateia mais o `-p:Version=`.

### Codout.Framework.EF 6.3.0

#### Fixed
- `EFRepository.SaveOrUpdate` / `SaveOrUpdateAsync` decidiam insert vs. update via `IEntity.IsTransient()`, o que quebrava qualquer entidade que pré-atribuísse o `Id` no construtor (ex.: `Id = Guid.NewGuid()`). A entidade era marcada como `Modified` e o `SaveChanges` falhava ao tentar `UPDATE` em uma linha inexistente. Agora a decisão é feita inspecionando `Context.Entry(entity).State` e, para entidades `Detached` com chave atribuída, consultando o banco via `Find` / `FindAsync`.
- `SaveOrUpdateAsync` deixou de bloquear a thread chamando `Find` síncrono; agora usa `FindAsync` e respeita o `CancellationToken` recebido.
- Quando o `SaveOrUpdate` precisa atualizar uma entidade existente, os valores são copiados via `Entry(existing).CurrentValues.SetValues(entity)`, preservando os `OriginalValues` (e portanto tokens de concorrência como `[Timestamp]`/`RowVersion`) trazidos do banco.

#### Changed
- `SaveOrUpdate` em entidade já rastreada com estado `Unchanged` não força mais a transição para `Modified`. Confia no change tracker para detectar mutações. Consumidores que dependiam do "force full update" devem chamar `Update(entity)` explicitamente.
- `SaveOrUpdate` em entidade `Detached` com chave não-default agora executa um roundtrip ao banco (`Find` / `FindAsync`) para distinguir insert de update. Em hot paths onde a intenção é conhecida, prefira `Save(entity)` ou `Update(entity)` para evitar a query.

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
