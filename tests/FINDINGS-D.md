# FINDINGS — suítes de teste NH / Mongo (sessão D)

Achados registrados durante a criação das suítes `Codout.Framework.NH.Tests` e
`Codout.Framework.Mongo.Tests`. Nenhum código de produção foi alterado; os
comportamentos abaixo estão cobertos por testes de caracterização (marcados
com `// BUG?:` quando aplicável).

## Ambiente / decisões de infraestrutura

- **Mongo — caminho adotado: EphemeralMongo FUNCIONOU.** O pacote
  `EphemeralMongo 3.2.0` baixou e subiu o `mongod` neste ambiente Linux,
  inclusive com `UseSingleNodeReplicaSet = true` (necessário para as
  transações do `MongoUnitOfWork`). Os testes de integração rodaram de
  verdade (0 skipped). A fixture (`tests/Codout.Framework.Mongo.Tests/TestInfrastructure.cs`)
  continua resiliente: se o `mongod` não subir em outro ambiente, os testes de
  integração (`[SkippableFact]`) são marcados como SKIPPED com a razão da
  falha, e os testes de unidade (validação de opções, null-checks, nome de
  coleção, DI) rodam sem servidor.
- **NH — o NHibernate 5.6.0 NÃO possui `MicrosoftDataSqliteDriver`** embutido
  (apenas `SQLite20Driver`, que reflete sobre `System.Data.SQLite`, e
  `CsharpSqliteDriver`). Foi criado um driver **somente de teste**
  (`MicrosoftDataSqliteTestDriver : ReflectionBasedDriver`) apontando para
  `Microsoft.Data.Sqlite`.
- **Microsoft.Data.Sqlite não implementa `GetSchema("DataTypes")`**, usado
  pelo auto-import de keywords do NHibernate no build da SessionFactory
  (`SchemaMetadataUpdater`). Os testes desligam isso com
  `hbm2ddl.keywords = none`. Quem for usar o pacote `Codout.Framework.NH`
  com Microsoft.Data.Sqlite em produção esbarra no mesmo problema.
- SQLite `:memory:` não funciona com NHibernate na configuração padrão
  (conexão é aberta/fechada por sessão e o banco evapora); os testes usam um
  arquivo temporário por fixture.

## Codout.Framework.Mongo

- **`Codout.Framework.Mongo/MongoRepository.cs` — `WherePaged` retorna `total` errado.**
  O `total` (out) é calculado **depois** de aplicar `Skip/Take`, ou seja,
  devolve o tamanho da página (≤ `size`) e não o total de registros que
  satisfazem o filtro. O `NHRepository.WherePaged` conta **antes** de paginar
  (comportamento correto); um paginador construído sobre o `total` do Mongo
  verá sempre "1 página". `// BUG?:` caracterizado em
  `MongoRepositoryQueryTests.WherePaged_Caracterizacao_TotalRetornaTamanhoDaPaginaENaoOTotalDoFiltro`.

- **`MongoRepository.Get(object key)` / `GetAsync` / `Load` só aceitam chaves ObjectId.**
  Qualquer chave que não parseie como `ObjectId` (int, Guid, string arbitrária
  — formatos legítimos de `_id` no MongoDB) retorna `null` **silenciosamente,
  sem consultar o servidor** e sem lançar erro. Entidades com Id não-ObjectId
  ficam inacessíveis por chave. `// BUG?:` caracterizado em
  `MongoRepositoryUnitTests.Get_ComChaveNaoObjectId_RetornaNullSemConsultarOServidor`.

- **`MongoRepository` não participa da transação do `MongoUnitOfWork`.**
  O repositório nunca usa `MongoUnitOfWork.CurrentSession` (`IClientSessionHandle`);
  todas as operações executam fora da sessão transacional. Consequência:
  `uow.BeginTransaction(); repo.Save(x); uow.Rollback();` **não desfaz** o
  `Save` — o documento permanece no banco. O UnitOfWork só tem efeito sobre
  operações feitas manualmente com a sessão. `// BUG?:` caracterizado em
  `MongoUnitOfWorkTests.Rollback_Caracterizacao_NaoDesfazOperacoesDoRepositorio`.

- **`Get(predicate)` sync e async divergem.** A versão síncrona usa
  `SingleOrDefault` (lança `InvalidOperationException` com múltiplos
  resultados); `GetAsync(predicate)` usa `FirstOrDefault` (retorna o primeiro
  sem validar unicidade). Mesmo contrato, semânticas diferentes. `// BUG?:`
  caracterizado em
  `MongoRepositoryAsyncTests.GetAsync_PorPredicadoComMultiplosResultados_NaoLanca_DiferenteDoSync`.

- Observação (comportamento documentado, não necessariamente bug):
  `Refresh`/`RefreshAsync` devolvem uma **nova** instância lida do banco e não
  atualizam a instância passada (diferente do NH, que recarrega in-place) —
  coberto em `MongoRepositoryCrudTests.Refresh_DeveRecarregarDoBanco`.

## Codout.Framework.NH

- **`Codout.Framework.NH/NHRepository.cs` — `AllReadOnly`/`WhereReadOnly` têm efeito colateral permanente na sessão.**
  Ambos setam `Session.DefaultReadOnly = true` e **nunca restauram** o valor:
  a sessão inteira vira read-only dali em diante. Qualquer entidade carregada
  depois (mesmo via `All()`/`Get()`) sai do dirty-check e alterações são
  silenciosamente ignoradas no `Flush`. `// BUG?:` caracterizado em
  `NHRepositoryQueryTests.AllReadOnly_EfeitoColateral_EntidadesCarregadasDepoisNaoSaoPersistidas`.

- **`NHRepository.WherePaged` não exige ordenação** — `Skip/Take` sem
  `ORDER BY` produz paginação não determinística em SQL (o mesmo vale para a
  versão Mongo). Comentário `// BUG?:` em
  `NHRepositoryQueryTests.WherePaged_SemOrdenacaoExplicita_NaoFalha`.

- `tests/Codout.Framework.NH.Tests/TestInfrastructure.cs` cobre o mapeamento
  FluentNHibernate básico (`ClassMap` de `Widget`, PK identity, SchemaExport)
  — o build da factory + CRUD completo passa sobre SQLite.
