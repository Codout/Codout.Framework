# FINDINGS — suítes de teste Mailer / Storage (sessão C)

Achados registrados durante a criação das suítes `Codout.Mailer.Tests`,
`Codout.Mailer.Razor.Tests`, `Codout.Framework.Storage.Tests` e
`Codout.Framework.Storage.Azure.Tests`. Nenhum código de produção foi alterado;
os comportamentos abaixo estão cobertos por testes de caracterização
(marcados com `// BUG?:` quando aplicável).

## Codout.Mailer

- **`Codout.Mailer/Services/MailerHealthCheck.cs`**
  - A classe está no **namespace global** (o arquivo não declara `namespace`),
    embora esteja na pasta `Services`.
  - O campo `_dispatcher` é declarado mas **nunca atribuído nem usado** (não há
    construtor) — o health check não verifica conectividade nenhuma e sempre
    retorna `Healthy` (o próprio código tem o comentário "Implementar verificação
    de conectividade"). Gera warning CS0169 no build.
  - Coberto em `ConfigureServicesTests.MailerHealthCheck_DeveRetornarHealthy`.

- **`Codout.Mailer/Services/MailerServiceBase.cs`**
  - `model.To.Address` é acessado no log **antes** do bloco `try`; um model sem
    destinatário derruba o chamador com `NullReferenceException` em vez de
    retornar `MailerResponse { Sent = false }` como as demais falhas.
  - Caracterizado em `MailerServiceBaseTests.Send_ComModelSemDestinatario_LancaNullReference`.

- **`Codout.Mailer/Helpers/HtmlUtilities.cs`**
  - `Cut(text, length)` com `length < 4` lança `ArgumentOutOfRangeException`
    (slice `text[..(length - 4)]` com índice negativo).
  - `CountWords` usa `Split(' ', '\n')` sem `RemoveEmptyEntries`: espaços
    consecutivos inflam a contagem (`"uma  duas"` → 3).
  - Caracterizados em `HtmlUtilitiesTests`.

- **`Codout.Mailer/Helpers/Extensions.cs`**
  - `ReadFully` força `stream.Position = 0`, exigindo stream *seekable*; streams
    de rede/pipe lançam `NotSupportedException`.
  - Caracterizado em `ExtensionsTests.ReadFully_ComStreamNaoSeekable_LancaNotSupportedException`.

- **`Codout.Mailer/Models/MailerResponse.cs`**
  - `ErrorMessages` não é inicializada (fica `null` em respostas de sucesso);
    consumidores precisam checar `null` antes de iterar.

## Codout.Mailer.AWS

- **`Codout.Mailer.AWS/AWSDispatcher.cs`**
  - O `AmazonSimpleEmailServiceV2Client` é **instanciado dentro de `Send`**
    (não injetável). Não é possível testar a montagem da mensagem MIME nem o
    tratamento da resposta do SES com fakes/handlers — só com chamada de rede
    real, o que é proibido nos testes. A suíte cobre apenas as validações de
    configuração (executadas antes de qualquer I/O) e o registro de DI.
  - **Inconsistência**: a validação de settings (`AccessKey`/`RegionEndpoint`/
    `SecretKey`) **lança `InvalidOperationException`**, enquanto qualquer outra
    falha é convertida em `MailerResponse { Sent = false }` pelo `catch`.
  - **Observação**: no caminho de sucesso, o `MessageId` retornado pelo SES é
    colocado em `ErrorMessages` (semanticamente errado, é o campo de erros).
  - Sugestão (não aplicada): receber `IAmazonSimpleEmailServiceV2` via construtor
    para permitir fakes.

## Codout.Mailer.SendGrid

- **`Codout.Mailer.SendGrid/SendGridDispatcher.cs`**
  - O `SendGridClient` é **instanciado dentro de `Send`** (não injetável). O SDK
    tem construtor `SendGridClient(HttpClient, ...)` que permitiria interceptar
    com `HttpMessageHandler` fake, mas o dispatcher não o expõe — montagem do
    `SendGridMessage` e tratamento de resposta não são testáveis sem rede.
  - `SendGridSettings.StandBox` (provável typo de **Sandbox**) **não é usado**
    em lugar nenhum.
  - Não há validação da `ApiKey` (nem na construção nem antes do envio).
  - `Sent` só é `true` para `202 Accepted` (200 OK resultaria `false`) — correto
    para a API v3 do SendGrid, registrado como observação.

## Codout.Mailer.Razor

- A renderização Razor real (compilação em runtime + template embarcado) **foi
  possível** sem host ASP.NET Core completo: bastou `IWebHostEnvironment` fake,
  `DiagnosticListener("Microsoft.AspNetCore")` e `PreserveCompilationContext`
  habilitado no csproj de teste. Coberta em `RazorRenderingIntegrationTests`.
- `RazorMailerOptions.Validate()` é `internal`, então a validação só é testável
  através de `AddMailerRazor` (coberto em `ConfigureServicesTests`).
- `RazorMailerOptions.EnableCache` **não é usado** em lugar nenhum (nem no
  engine nem no `AddMailerRazor`).

## Codout.Framework.Storage

- O pacote define `FileSystemStorageOptions` e `AwsStorageOptions`, mas **não
  existe implementação `IStorage` de file system nem de AWS S3** no repositório
  (somente a do Azure). A suíte cobre exceções, options, models e valida o
  contrato `IStorage` com um fake em memória
  (`InMemoryStorageContractTests`).

## Codout.Framework.Storage.Azure

- **`Codout.Framework.Storage.Azure/AzureStorage.cs`**
  - `GetBlobUri` com CDN habilitado **não faz URL-encoding do `fileName`**
    (interpolação direta), enquanto o endpoint de blob (SDK) escapa caracteres
    especiais — o mesmo blob gera URIs divergentes (ex.: espaço vs `%20`).
    Caracterizado em `AzureStorageBlobUriTests.GetBlobUri_ComCdn_NaoEscapaNomeDoArquivo`.
  - Connection string sintaticamente inválida não falha na construção (client é
    `Lazy`); o erro só aparece no primeiro uso e **escapa como `FormatException`
    do SDK** em vez de `StorageException` da abstração. Caracterizado em
    `AzureStorageConstructionTests`.
  - Operações que dependem de requisição real (Exists/Download/List/SAS/Upload
    efetivo etc.) **não são testáveis offline**: o `BlobServiceClient` é criado
    internamente a partir da connection string, sem ponto de injeção para um
    client fake e sem Azurite no ambiente. A suíte cobre construção, validação
    de argumentos (que ocorre antes de qualquer I/O), montagem de URIs
    (local, sem rede) e o registro de DI.
- **`Extensions/ServiceCollectionExtensions.cs`**
  - `AddAzureStorage(options)` cria a instância de `AzureStorage` **eagerly no
    momento do registro** (`AddSingleton` com instância), então options
    inválidas falham no `AddAzureStorage`, não no primeiro resolve.
    Caracterizado em `ServiceCollectionExtensionsTests`.

## Observação de build (não é bug de produto)

- `Directory.Build.props` liga `GeneratePackageOnBuild` globalmente; compilar os
  projetos de teste dispara o pack dos projetos referenciados (e, fora da
  solution, `$(SolutionDir)Packages` resolve para `<projeto>/Packages/`). Os
  csproj de teste desligam isso para si mesmos; para não gerar `.nupkg` ao rodar
  as suítes, use `dotnet test ... -p:GeneratePackageOnBuild=false`.
