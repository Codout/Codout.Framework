# FINDINGS-B — Achados durante a escrita das suítes de teste

Achados registrados durante a criação/ampliação das suítes de
`Codout.Framework.Data.Tests`, `Codout.Framework.Domain.Tests` e
`Codout.Framework.EF.Tests`. Nenhum código de produção foi alterado; cada item
abaixo tem um characterization test marcado com `// BUG?:` documentando o
comportamento ATUAL.

## 1. `IAuditable` duplicada — o `AuditableInterceptor` ignora a abstração pública

- **Caminho:** `Codout.Framework.EF/Interceptors/AuditableInterceptor.cs` (duplicata) e
  `Codout.Framework.Data/Auditing/IAuditable.cs` (abstração pública).
- **Descrição:** existem duas interfaces `IAuditable` com a mesma forma:
  `Codout.Framework.Data.Auditing.IAuditable` e
  `Codout.Framework.EF.Interceptors.IAuditable` (declarada no fim de
  `AuditableInterceptor.cs`). Embora o arquivo do interceptor importe
  `Codout.Framework.Data.Auditing`, a resolução de nomes do C# prioriza o tipo do
  namespace corrente — o check `e.Entity is IAuditable` usa a duplicata LOCAL.
  Resultado: entidades que implementam somente a abstração pública
  `Data.Auditing.IAuditable` são **silenciosamente ignoradas** pela auditoria
  automática (CreatedAt/CreatedBy/UpdatedAt/UpdatedBy nunca são preenchidos).
  O mesmo arquivo também duplica `ICurrentUserProvider`
  (vs. `Codout.Framework.Data/Auditing/ICurrentUserProvider.cs`), de modo que o
  ctor do interceptor exige a duplicata local, não a abstração do pacote Data.
- **Teste:** `tests/Codout.Framework.EF.Tests/InterceptorTests.cs` →
  `Entity_implementing_only_the_Data_Auditing_IAuditable_is_not_audited`.
- **Sugestão (não aplicada):** remover as interfaces duplicadas de
  `AuditableInterceptor.cs` e usar as de `Codout.Framework.Data.Auditing`
  (mudança potencialmente breaking para quem referencia os tipos do namespace
  `Codout.Framework.EF.Interceptors` — avaliar bump minor/major).

## 2. `EFRepository.RefreshAsync(entity, cancellationToken)` é síncrono e ignora o token

- **Caminho:** `Codout.Framework.EF/EFRepository.cs`.
- **Descrição:** o overload `RefreshAsync(T entity, CancellationToken cancellationToken)`
  faz `Task.FromResult(Refresh(entity))`, ou seja, executa `Reload()` SÍNCRONO na
  thread chamadora e ignora completamente o `CancellationToken` — mesmo um token
  já cancelado não interrompe a operação. Inconsistente com o overload sem token,
  que usa `ReloadAsync()`, e com o resto da superfície async (fix análogo já foi
  feito no `SaveOrUpdateAsync` na 6.3.0).
- **Teste:** `tests/Codout.Framework.EF.Tests/EFRepositoryAsyncTests.cs` →
  `RefreshAsync_with_token_currently_ignores_cancellation`.
- **Sugestão (não aplicada):** `await Context.Entry(entity).ReloadAsync(cancellationToken)`.

## 3. Assimetria `Commit()` × `CommitAsync()` sem transação ativa

- **Caminho:** `Codout.Framework.EF/EFUnitOfWork.cs`.
- **Descrição:** `Commit()` sem transação ativa faz apenas `SaveChanges()`
  (auto-commit do EF Core), mas `CommitAsync()` na mesma situação lança
  `InvalidOperationException("Nenhuma transação ativa para commit...")`.
  Código que funciona na via síncrona quebra ao migrar para a assíncrona.
- **Teste:** `tests/Codout.Framework.EF.Tests/EFUnitOfWorkTests.cs` →
  `CommitAsync_without_transaction_throws_unlike_sync_Commit`.
- **Sugestão (não aplicada):** alinhar os dois contratos (provavelmente fazer o
  `CommitAsync` aceitar auto-commit via `SaveChangesAsync`).

## Observações menores (sem teste dedicado)

- `EFUnitOfWork.Commit(IsolationLevel)` ignora o parâmetro `isolationLevel`
  (há comentário no código explicando que o isolation level pertence ao
  `BeginTransaction`); a existência do overload no contrato `IUnitOfWork` induz
  o consumidor a achar que tem efeito.
- Há também duas `ISoftDeletable` no ecossistema:
  `Codout.Framework.Data.Auditing.ISoftDeletable` (IsDeleted/DeletedAt/DeletedBy,
  usada pelo `SoftDeleteInterceptor`) e
  `Codout.Framework.Domain.Interfaces.ISoftDeletable` (apenas `DeletedAt`).
  Entidades que implementem só a versão do Domain não recebem soft delete
  automático — mesmo padrão de armadilha do achado nº 1.
