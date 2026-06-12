# Codout.Framework.Storage.Azure

Implementação da interface `IStorage` do Codout.Framework.Storage para Azure Blob Storage, com upload/download, cópia/movimentação entre containers, metadados, listagem e geração de SAS URI.

## Instalação

```bash
dotnet add package Codout.Framework.Storage.Azure
```

## Uso

Registre via DI com um dos overloads de `AddAzureStorage` (por `IConfiguration` — lê a connection string `AzureStorage` —, por connection string, por `AzureStorageOptions` ou por `Action<AzureStorageOptions>`). O serviço é registrado como singleton de `IStorage`:

```csharp
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// usa ConnectionStrings:AzureStorage do appsettings.json
builder.Services.AddAzureStorage(builder.Configuration);

// ou explicitamente:
builder.Services.AddAzureStorage("UseDevelopmentStorage=true");
```

Consumindo `IStorage`:

```csharp
using Codout.Framework.Storage;

public class DocumentoService(IStorage storage)
{
    public async Task<Uri> SalvarAsync(Stream arquivo, CancellationToken ct)
    {
        // upload (o content-type é inferido pela extensão do arquivo)
        return await storage.UploadAsync(arquivo, "documentos", "contrato.pdf", ct);
    }

    public async Task<Stream> BaixarAsync(CancellationToken ct)
    {
        return await storage.DownloadAsync("documentos", "contrato.pdf", ct);
    }
}
```

Também é possível instanciar diretamente: `new AzureStorage(connectionString)` ou `new AzureStorage(new AzureStorageOptions { ConnectionString = "..." })`. Erros do Azure são encapsulados em `StorageException` (de Codout.Framework.Storage).

## Pacotes relacionados

- [Codout.Framework.Storage](https://www.nuget.org/packages/Codout.Framework.Storage) — abstração `IStorage`, options e exceções (dependência deste pacote).
- [Codout.Framework.Common](https://www.nuget.org/packages/Codout.Framework.Common) — utilitários comuns do framework.
- [Codout.Framework.Application](https://www.nuget.org/packages/Codout.Framework.Application) — camada de aplicação/serviços.

---
Parte do [Codout.Framework](https://github.com/Codout/Codout.Framework) — licença MIT.
