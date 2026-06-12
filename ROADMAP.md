# ROADMAP — Plano de qualidade "nota 10"

Plano de implementação para elevar o Codout.Framework da avaliação atual
(**6,5/10**, jun/2026) até **10/10**. **Status 2026-06-12: Fases 1-6 executadas;
Fases 7-8 quase completas — pendências marcadas abaixo.** As fases estão em ordem de execução
recomendada: cada uma cria a base da seguinte. Os ganhos de nota são
estimativas para acompanhamento, não ciência exata.

## Princípios

- **Não quebrar consumidores**: pacotes já publicados no NuGet.org seguem
  SemVer. Mudança que altere API pública exige bump major e aviso no
  CHANGELOG.
- **Cada pacote anda no próprio ritmo**: bumps de versão apenas nos pacotes
  efetivamente tocados (ver CLAUDE.md).
- **CI primeiro**: nenhuma fase é considerada concluída sem o `dotnet.yml`
  verde validando o resultado.

---

## Fase 1 — Quick wins de CI e higiene (esforço: pequeno · ganho: +0,25)

Correções de baixo risco que destravam o resto.

- [x] `dotnet.yml`: atualizar `dotnet-version` de `9.0.x` para `10.0.x`
      (o core targeta `net10.0` via `Directory.Build.props`).
- [x] Adicionar `Codout.Framework.Api.Dto` e `Softprime.Multitenancy` à
      `Codout.Framework.sln` (estão no `release-packages.json` mas fora da
      solution — hoje o CI de PR não os builda).
- [x] Remover `appveyor.yml` (referencia Visual Studio 2012; o pipeline real
      é GitHub Actions).
- [x] Corrigir badge de build do README (aponta para `build.yml`/`dotnet.yml`
      de forma inconsistente).

**Critério de aceite**: `dotnet build` da solution verde no CI com SDK 10.

## Fase 2 — Testes na solution (esforço: pequeno · ganho: +0,25)

Hoje os 4 projetos de teste existem fora da solution; o `dotnet test
Codout.Framework.sln` do `release.yml` roda zero testes.

- [x] Criar pasta `tests/` na raiz.
- [x] Migrar `NetCore/Codout.Framework.EF.Tests` para
      `tests/Codout.Framework.EF.Tests` e atualizar `TEST_PROJECT` no
      `core-release.yml` (linha 39).
- [x] Adicionar os projetos de teste ativos (EF.Tests e, se viável por
      caminho, Mcp.Tests) à `Codout.Framework.sln`.
- [x] Remover do escopo os testes legados (`NetFull/*.Tests`,
      `NetCore/Codout.Framework.NetCore.Tests`) — dependem de infraestrutura
      externa (appsettings com conexões reais) e serão substituídos na Fase 4.

**Critério de aceite**: `dotnet test Codout.Framework.sln` executa testes de
verdade (> 0) no CI de PR e no gate do `release.yml`.

## Fase 3 — Limpeza do legado (esforço: médio · ganho: +1,0)

Remover ~2.000 linhas de código morto que distorcem a leitura do repo e
impedem políticas globais de qualidade (Fase 5).

- [x] **Decisão prévia (usuário)**: deletar ou arquivar em branch
      `archive/legacy` antes de remover. Recomendação: branch de arquivo +
      remoção do master.
- [x] Remover `NetFull/` (EF6, .NET Framework — EOL).
- [x] Remover `NetCore/` (netcoreapp2.x — EOL; testes já migrados na Fase 2).
- [x] Remover `src/NetCore/` (Cosmos e DocumentDB em `netcoreapp2.0` com SDK
      `Microsoft.Azure.DocumentDB.Core` descontinuado e anti-padrão
      `AllAsync().Result` em `CosmosRepository.cs`). Se houver demanda real
      por Cosmos, recriar do zero como `Codout.Framework.Cosmos` com SDK
      `Microsoft.Azure.Cosmos` — fora do escopo deste plano.
- [x] Remover `Codout.Framework.DP` (quebrado: referencia
      `Codout.Framework.DAL` inexistente; `IsPackable=false`).
- [x] Remover/renomear `Shared/Codout.Framework.Shared.Commom` (typo
      "Commom") e avaliar se `Shared/` ainda tem consumidor.
- [x] Atualizar CLAUDE.md (seção "Pacotes excluídos do release automatizado")
      e CHANGELOG (`### Repository`) refletindo as remoções.

**Critério de aceite**: nenhum csproj com target EOL no repo; busca por
"Commom" retorna vazio.

## Fase 4 — Cobertura de testes (esforço: grande · ganho: +1,5)

O maior gap: 19 de 23 pacotes publicáveis sem teste algum. Priorizar por
risco × facilidade, em `tests/<Pacote>.Tests`:

| Onda | Pacotes | Abordagem |
|------|---------|-----------|
| 1 | Data, Domain, Common, DynamicLinq, Security.* | Unit puro (sem I/O) — maior retorno imediato |
| 2 | EF (ampliar), Mongo, NH | EF: SQLite in-memory; Mongo: Testcontainers ou EphemeralMongo; NH: SQLite |
| 3 | Mailer, Mailer.Razor, Api.Client, Storage | Fakes/mocks (HttpMessageHandler fake, template rendering, contratos de abstração) |
| 4 | Storage.Azure, Mailer.AWS, Mailer.SendGrid, Api, Application, Multitenancy | Azurite para Azure; demais via mocks dos SDKs |

- [x] Onda 1 concluída e no CI.
- [x] Onda 2 concluída (Testcontainers exige Docker no runner — `ubuntu-latest` já tem).
- [x] Onda 3 concluída.
- [x] Onda 4 concluída.
- [ ] Coleta de cobertura com `coverlet.collector` + publicação de relatório
      no CI (artifact ou Codecov + badge no README).
- [ ] Meta de cobertura: ≥ 70 % nos pacotes core (Data/Domain/Common/EF),
      ≥ 50 % nos demais.

**Critério de aceite**: todo pacote publicável tem projeto de teste; metas de
cobertura atingidas e visíveis no CI.

## Fase 5 — Qualidade estática (esforço: médio · ganho: +0,75)

Aplicar via `Directory.Build.props` (depois da Fase 3, para não gastar
esforço em código morto):

- [x] `<Nullable>enable</Nullable>` global, corrigindo projetos em ondas
      (13/37 já têm). Para projetos ainda não migrados, opt-out temporário
      explícito no csproj com comentário `TODO`.
- [x] `<TreatWarningsAsErrors>true</TreatWarningsAsErrors>` +
      `<EnableNETAnalyzers>true</EnableNETAnalyzers>` +
      `<AnalysisLevel>latest-recommended</AnalysisLevel>`.
- [x] Expandir `.editorconfig` com convenções C# (indentação, naming,
      `dotnet_diagnostic.*` para calibrar severidade dos analyzers).
- [x] Resolver os `[Obsolete]` pendentes em `Codout.Framework.Common`
      (Crypto, NumericExtensions, WebPageFetcher): remover no próximo bump
      major ou documentar substitutos.
- [x] Atualizar `System.ComponentModel.Annotations` 5.0.0 → versão atual (ou
      remover a referência onde o shared framework já cobre).

**Critério de aceite**: `dotnet build` sem warnings na solution inteira;
nullable ativo em 100 % dos projetos publicáveis.

## Fase 6 — Empacotamento profissional (esforço: médio · ganho: +0,75)

Tudo no `Directory.Build.props`, exceto onde indicado:

- [x] SourceLink: `Microsoft.SourceLink.GitHub`, `PublishRepositoryUrl`,
      `EmbedUntrackedSources`, `IncludeSymbols` + `SymbolPackageFormat=snupkg`.
- [x] `ContinuousIntegrationBuild=true` nos workflows de release (build
      determinístico).
- [x] `PackageReadmeFile`: criar `README.md` curto por pacote (o que é,
      instalação, exemplo mínimo) e empacotar — hoje 19/20 pacotes têm página
      vazia no NuGet.org.
- [ ] `Description` e `PackageTags` individualizados em cada csproj.
- [ ] Publicar os snupkg no fluxo de release (NuGet.org aceita no mesmo
      `dotnet nuget push`).
- [ ] Republicar pacotes com bump **patch** (mudança só de empacotamento) —
      confirmar com o usuário antes, pois publicar exige tag.

**Critério de aceite**: página de cada pacote no NuGet.org com README, ícone
e símbolos depuráveis (F12 no código do framework a partir de um consumidor).

## Fase 7 — Documentação (esforço: médio · ganho: +0,5)

- [x] Reescrever a tabela de módulos do README: remover "Zenvia", "DAL",
      "Kendo.DynamicLinq", "DP", "Shared"; incluir Security.*, Storage,
      Storage.Azure, Application, Mcp, Image.Extensions.
- [ ] Seção "Qual pacote eu instalo?" com cenários comuns (EF + API,
      Mongo, multitenancy, mailer).
- [x] `GenerateDocumentationFile=true` global (via `Directory.Build.props`)
      e completar XML docs nos tipos públicos de `Domain`, `Common`, `Data`
      e `Api` — com `TreatWarningsAsErrors` da Fase 5, o CS1591 força a
      conclusão (calibrar severidade se necessário).
- [x] Atualizar instruções de build/teste do README (caminhos reais da
      pasta `tests/`).

**Critério de aceite**: README sem referência a módulo inexistente; XML docs
completos nos 4 pacotes core.

## Fase 8 — Hardening de CI/CD (esforço: pequeno · ganho: +0,5)

- [x] Dependabot (`.github/dependabot.yml`) para NuGet + GitHub Actions,
      agrupando updates minor/patch.
- [x] CodeQL (`github/codeql-action`) para análise de segurança em PR.
- [ ] Gate de cobertura no `dotnet.yml` (falhar se cair abaixo da meta).
- [ ] Branch protection em `master`: exigir CI verde + 1 review (config no
      GitHub, não no repo).
- [x] Concurrency/cancel-in-progress nos workflows de PR para economizar
      runner.

**Critério de aceite**: PRs só mergeiam com CI + CodeQL verdes; dependências
atualizadas automaticamente via PRs do Dependabot.

---

## Resumo de progressão

| Fase | Tema | Esforço | Nota acumulada (alvo) |
|------|------|---------|----------------------|
| — | Estado atual | — | 6,5 |
| 1 | Quick wins CI | P | 6,75 |
| 2 | Testes na solution | P | 7,0 |
| 3 | Limpeza do legado | M | 8,0 |
| 4 | Cobertura de testes | G | 9,5 *(maior fase — pode rodar em paralelo às 5–8)* |
| 5 | Qualidade estática | M | +0,75 dentro do teto |
| 6 | Empacotamento | M | +0,75 dentro do teto |
| 7 | Documentação | M | +0,5 dentro do teto |
| 8 | Hardening CI/CD | P | **10,0** |

## Pontos de decisão (perguntar ao usuário antes de executar)

1. **Fase 3**: deletar legado direto ou arquivar em branch `archive/legacy`
   primeiro? (Recomendação: arquivar e deletar.)
2. **Fase 4, onda 2**: Testcontainers (fiel, requer Docker) vs. bibliotecas
   in-memory (mais rápido, menos fiel) para Mongo.
3. **Fase 6**: republicar todos os pacotes com bump patch após SourceLink/
   README, ou deixar para o próximo release natural de cada pacote?
4. **Fase 5**: `[Obsolete]` de `Common` — remover em bump major agora ou
   manter até 7.0?
