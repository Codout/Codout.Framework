# Codout.Framework.Storage

Abstração para armazenamento de arquivos em nuvem: define o contrato `IStorage` (upload, download, listagem, metadados e URIs SAS) e os modelos `StorageItem`, `StorageMetadata` e `StorageOptions`, independentes do provedor.

## Instalação

```bash
dotnet add package Codout.Framework.Storage
```

## Uso

Dependa apenas de `IStorage` e deixe o provedor concreto para um pacote de implementação:

```csharp
using Codout.Framework.Storage;

public class ArquivoService(IStorage storage)
{
    public async Task<Uri> EnviarAsync(Stream arquivo, CancellationToken ct)
    {
        return await storage.UploadAsync(arquivo, "documentos", "contrato.pdf", ct);
    }

    public Task<Stream> BaixarAsync(CancellationToken ct) =>
        storage.DownloadAsync("documentos", "contrato.pdf", ct);
}
```

Outras operações do contrato:

```csharp
bool existe = await storage.ExistsAsync("documentos", "contrato.pdf", ct);
IEnumerable<StorageItem> itens = await storage.ListAsync("documentos", ct);
Uri sas = await storage.GetSasUriAsync("documentos", "contrato.pdf", TimeSpan.FromHours(1), ct);
await storage.DeleteAsync("documentos", "contrato.pdf", ct);
```

## Pacotes relacionados

- `Codout.Framework.Storage.Azure` — implementação de `IStorage` para Azure Blob Storage.

Parte do [Codout.Framework](https://github.com/Codout/Codout.Framework) — licença MIT.
