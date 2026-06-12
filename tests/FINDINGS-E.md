# FINDINGS — suítes de teste DynamicLinq / Api.Dto / Application / Api.Client / Api / Multitenancy (sessão E)

Achados registrados durante a criação das suítes `Codout.DynamicLinq.Tests`,
`Codout.Framework.Api.Dto.Tests`, `Codout.Framework.Application.Tests`,
`Codout.Framework.Api.Client.Tests`, `Codout.Framework.Api.Tests` e
`Codout.Multitenancy.Tests`. Nenhum código de produção foi alterado; os
comportamentos abaixo estão cobertos por testes de caracterização (marcados
com `// BUG?:` quando aplicável).

## Codout.DynamicLinq

- **`Codout.DynamicLinq/QueryableExtensions.cs`**
  - O overload "simples" `ToDataSourceResult(take, skip, sort, filter)` repassa
    `aggregates`/`group` nulos para o overload completo, e tanto `Aggregates()`
    quanto o corpo principal fazem `xs as T[] ?? xs.ToArray()` — com argumento
    nulo, `ToArray()` lança `ArgumentNullException`. O mesmo vale para `sort`
    nulo (em `Sort()`) quando não há grupos. Consequência: o overload de 4
    parâmetros e o overload `ToDataSourceResult(DataSourceRequest)` com um
    request recém-criado (propriedades nulas) **sempre lançam exceção**. Para
    funcionar, o chamador precisa passar coleções vazias.
    Coberto em `ToDataSourceResultSortPageTests.OverloadSemAggregatesEGroups_LancaArgumentNullException`,
    `OverloadComDataSourceRequestPadrao_LancaArgumentNullException` e
    `SortNulo_SemGrupos_LancaArgumentNullException`.
  - Um `Filter` simples (sem `Logic`) é **ignorado silenciosamente**: `Filters()`
    só aplica o filtro quando `filter.Logic != null`, então todo filtro de uma
    condição precisa ser embrulhado em um composto (`Logic = "and"`). Coberto em
    `ToDataSourceResultFilterTests.Filtro_SemLogic_EhIgnoradoSilenciosamente`.

- **`Codout.DynamicLinq/EnumerableExtensions.cs`**
  - `GroupByMany` repassa `Group.Aggregates` (nulo por padrão) para
    `QueryableExtensions.Aggregates`, que lança `ArgumentNullException` na
    enumeração dos grupos (a avaliação é deferida — a exceção só aparece quando
    `result.Groups` é enumerado). Todo `Group` precisa de `Aggregates = []`.
    Coberto em `AggregatesAndGroupTests.Group_SemAggregates_LancaAoEnumerarOsGrupos`.

## Codout.Framework.Api.Dto

- Os tipos do pacote (via shared project `Codout.Framework.Dto.Shared`) são
  declarados no namespace **`Codout.Framework.Api.Client`**, e não em
  `Codout.Framework.Api.Dto` — observação de contrato, documentada em
  `DtoContractTests.TiposDoPacote_EstaoNoNamespaceCodoutFrameworkApiClient`.

## Codout.Framework.Application

- **`Codout.Framework.Application/CrudAppServiceBase.cs`**
  - `SaveAsync(null)` lança `NullReferenceException` (em vez de
    `ArgumentNullException`) e a mensagem usa `nameof(TDto)`, que vira o texto
    literal `"TDto"` em vez do nome real do DTO. Coberto em
    `CrudAppServiceBaseTests.SaveAsync_ComEntradaNula_LancaNullReferenceException`.
  - `GetAllAsync(new DataSourceRequest())` lança `ArgumentNullException` (mesma
    causa raiz do bug de DynamicLinq acima): um consumidor que poste um request
    "vazio" na API recebe erro. Coberto em
    `CrudAppServiceBaseTests.GetAllAsync_ComRequestPadrao_LancaArgumentNullException`.
  - `UpdateAsync` nunca chama `Repository.Update/SaveOrUpdate` — depende de a
    entidade estar rastreada pelo ORM e do `UnitOfWork.Commit()`. Funciona com
    EF/NH, mas não com repositórios sem change tracking (observação,
    caracterizada em `UpdateAsync_NaoChamaUpdateDoRepositorio`).

- **`Codout.Framework.Application/MapperProfile.cs`**
  - O `ReverseMap()` de `CreateMap(typeof(Entity<>), typeof(EntityDto<>))` não
    consegue materializar entidades concretas: ao mapear `EntityDto<Guid>` →
    `Customer`, o AutoMapper tenta instanciar a classe **abstrata**
    `Entity<Guid>` e lança `ArgumentException`. Na prática,
    `Mapper.Map<TEntity>(input)` — usado por `CrudAppServiceBase.SaveAsync` —
    falha se o consumidor não registrar um mapa próprio DTO→Entidade. Coberto em
    `MappingProfileTests.MappingProfile_MapaReverso_NaoConsegueCriarEntityConcreta`.

## Codout.Framework.Api.Client

- **`Codout.Framework.Api.Client/Extensions/HttpClientExtensions.cs`**
  - `GetExceptionAsync` lança `new ApiClientException(...)` **dentro do próprio
    bloco try**; o `catch` genérico logo abaixo a engole e relança
    `new Exception(corpo)`. Resultado: o `ApiClientException` tipado (com o
    `ApiException` estruturado) **nunca chega ao chamador** — toda resposta de
    erro vira `Exception` com o corpo bruto como mensagem. Coberto em
    `HttpClientExtensionsTests.RespostaDeErro_ComCorpoDeApiException_LancaExceptionGenericaComOCorpo`.

- **`Codout.Framework.Api.Client/RestApiClient.cs`**
  - Em `DeleteAsync`, a chamada `Client.DeleteAsync(...)` resolve para o método
    de **instância** `HttpClient.DeleteAsync` (métodos de instância têm
    precedência sobre extensões), e não para a extensão
    `HttpClientExtensions.DeleteAsync` que valida o status. Consequência:
    **erros HTTP no DELETE são silenciosamente ignorados** (nenhuma exceção é
    lançada para 4xx/5xx). Coberto em
    `RestApiClientTests.DeleteAsync_ComRespostaDeErro_NaoLancaExcecao`.

- **`Codout.Framework.Api.Shared/ApiException.cs`** — `ApiException` é um POCO
  (não herda de `System.Exception`), portanto não pode ser lançada diretamente;
  apenas serializada/embrulhada (observação, caracterizada em
  `ApiExceptionTests.ApiException_NaoEhUmaException`).

- **`Codout.Framework.Api.Client/ApiClientBase.cs`** — o `HttpClient` é criado
  internamente, sem ponto de injeção de `HttpMessageHandler`; os testes de
  `RestApiClient` substituem o handler via reflection (campo privado
  `HttpMessageInvoker._handler`) para evitar rede (observação de testabilidade).

## Codout.Framework.Api

- **`Codout.Framework.Api/Middleware/ApiExceptionMiddleware.cs`**
  - Ao capturar uma exceção, o middleware **não altera `Response.StatusCode`**:
    serializa o status vigente (200) no corpo e responde `200 OK` para qualquer
    exceção não tratada, em vez de 500. Coberto em
    `ApiExceptionMiddlewareTests.QuandoHaExcecao_StatusCodePermanece200`.

- **`Codout.Framework.Api/RestApiEntityBase.cs`**
  - `Get` retorna `200 OK` com corpo nulo quando a entidade não existe, embora o
    contrato anunciado via `ProducesResponseType` inclua `404 NotFound`. Coberto
    em `RestApiEntityBaseTests.Get_QuandoNaoEncontrado_RetornaOkComCorpoNulo`.

## Codout.Multitenancy

- **`Codout.Multitenancy/MemoryCacheTenantResolver.cs`**
  - O cache é **consultado** com a chave de `GetContextIdentifier(httpContext)`,
    mas **gravado** com a chave de `GetTenantIdentifier(tenantContext)`. Se as
    duas convenções diferirem (ex.: busca por host, gravação por chave do
    tenant), a consulta nunca encontra a entrada gravada e o tenant é
    re-resolvido a cada request (cache inócuo + entradas órfãs). Só funciona
    quando as implementações retornam o mesmo valor. Coberto em
    `MemoryCacheTenantResolverTests.ResolveAsync_QuandoChaveDeBuscaDifereDaChaveDeGravacao_NuncaUsaOCache`.

- **`Codout.Multitenancy/Internal/TenantPipelineMiddleware.cs`**
  - Quando não há `TenantContext` no request, o middleware **não chama `_next`**:
    o restante do pipeline é curto-circuitado silenciosamente (resposta vazia
    200). Coberto em
    `TenantPipelineMiddlewareTests.UsePerTenant_SemTenant_CurtoCircuitaSemExecutarORoot`.

- Observação: a pasta `Codout.Multitenancy` contém dois csproj que compilam os
  mesmos fontes (`Codout.Multitenancy.csproj` net10.0 e
  `Softprime.Multitenancy.csproj` netstandard2.0); a suíte referencia o
  `Codout.Multitenancy.csproj`. A chave usada em `HttpContext.Items` ainda é
  `"softprime.TenantContext"` (resquício do nome antigo).
