# FINDINGS — Bugs pré-existentes encontrados pela Fase 4 (triagem)

Durante a escrita das suítes de teste (2026-06-12), os bugs abaixo foram
encontrados no código de produção. **Nenhum foi corrigido**: a política da
fase foi caracterizar o comportamento atual (testes marcados com `// BUG?:`)
para não alterar contrato sem decisão explícita. Detalhes completos nos
arquivos `FINDINGS-A.md` a `FINDINGS-E.md` nesta pasta.

## Alta severidade (corrigir primeiro; provável bump patch/minor)

| # | Pacote | Bug | Detalhe |
|---|--------|-----|---------|
| 1 | Security.Argon2 | `NeedsRehash` compara com parâmetros errados — todo hash criado via `Strength` se reporta `SuccessRehashNeeded` para sempre (rehash a cada login) | FINDINGS-A |
| 2 | Mongo | `WherePaged` calcula `total` depois de Skip/Take — retorna o tamanho da página, não o total do filtro | FINDINGS-D |
| 3 | Mongo | `MongoRepository` não participa da transação do `MongoUnitOfWork` — `Rollback` não desfaz escritas | FINDINGS-D |
| 4 | EF | Interceptor de auditoria declara cópias locais de `IAuditable`/`ICurrentUserProvider` que sombreiam as de `Data.Auditing` — entidades auditáveis são ignoradas silenciosamente | FINDINGS-B |
| 5 | Api | `ApiExceptionMiddleware` nunca seta `Response.StatusCode` — exceções respondem `200 OK` | FINDINGS-E |
| 6 | NH | `AllReadOnly`/`WhereReadOnly` setam `Session.DefaultReadOnly=true` permanentemente — escritas posteriores na mesma sessão são ignoradas no Flush | FINDINGS-D |
| 7 | Multitenancy | `MemoryCacheTenantResolver` lê o cache pela chave de contexto e grava pela de tenant — se diferirem, o cache nunca acerta | FINDINGS-E |

## Média severidade

| # | Pacote | Bug | Detalhe |
|---|--------|-----|---------|
| 8 | Mailer | `MailerServiceBase` loga `model.To.Address` antes do try — NRE escapa em vez de `MailerResponse{Sent=false}` | FINDINGS-C |
| 9 | DynamicLinq | Overloads de `ToDataSourceResult` sem `aggregates`/`group` lançam `ArgumentNullException`; filtro sem `Logic` é ignorado | FINDINGS-E |
| 10 | Application | `UpdateAsync` nunca chama `Repository.Update`; `SaveAsync(null)` lança NRE com mensagem `"TDto"` | FINDINGS-E |
| 11 | Api.Client | Erros HTTP de DELETE silenciosamente ignorados (binding ao método de instância do HttpClient); `ApiClientException` engolida pelo próprio catch | FINDINGS-E |
| 12 | Mongo | `Get`/`GetAsync`/`Load` com chave não-ObjectId retornam null sem consultar; sync usa `SingleOrDefault`, async usa `FirstOrDefault` | FINDINGS-D |
| 13 | Storage.Azure | URI de CDN não faz URL-encoding do fileName; `AddAzureStorage` instancia eagerly | FINDINGS-C |
| 14 | EF | `RefreshAsync(entity, ct)` executa `Reload()` síncrono e ignora o token; `Commit()` × `CommitAsync()` assimétricos sem transação | FINDINGS-B |
| 15 | Common | `RemoveAccents` quebrado no .NET moderno (codepage não registrada); `GetAge` erra no dia do aniversário; `IsEmail("")` retorna true | FINDINGS-A |

## Baixa severidade / observações de design

- `MailerHealthCheck` sempre Healthy, campo `_dispatcher` morto (FINDINGS-C).
- `SendGridSettings.StandBox` (typo) sem uso; `EnableCache` do Razor sem efeito (FINDINGS-C).
- `Api.Dto` vive no namespace `Codout.Framework.Api.Client` (FINDINGS-E).
- `TenantPipelineMiddleware` sem tenant não chama `_next` (FINDINGS-E).
- `FileSystemStorageOptions`/`AwsStorageOptions` existem sem implementação (FINDINGS-C).
- `Get(predicate)` do Mongo: divergência sync/async de semântica (FINDINGS-D).
- `Chop`, `HtmlEncode` (duplo escape), `EnumHelper.GetLocalizedName` lendo `Description` (FINDINGS-A).

## Como corrigir com segurança

Cada correção deve: (1) ajustar o characterization test correspondente para o
comportamento correto, (2) passar no package validation (baseline NuGet), e
(3) entrar no CHANGELOG do pacote com bump apropriado (a maioria é patch;
itens que mudam contrato observável, como #2 e #5, merecem minor + nota).
