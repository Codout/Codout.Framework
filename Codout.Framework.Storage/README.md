# Codout.Framework.Storage

Abstração cloud-agnostic para operações de armazenamento de arquivos (Azure, AWS S3, File System).

## ?? Recursos

- ? **Interface unificada** para múltiplos provedores
- ? **Async/await** com CancellationToken
- ? **Metadata support** para arquivos
- ? **SAS tokens** para acesso temporário
- ? **List operations** para descoberta de arquivos
- ? **Batch operations** para deletar múltiplos arquivos
- ? **Progress reporting** para uploads
- ? **Exceções customizadas** para tratamento específico
- ? **CDN support** para URLs otimizadas
- ? **Thread-safe** implementations

## ?? Instalação

```bash
# Abstrações
dotnet add package Codout.Framework.Storage

# Azure Blob Storage
dotnet add package Codout.Framework.Storage.Azure

# AWS S3 (em desenvolvimento)
dotnet add package Codout.Framework.Storage.AWS

# File System (em desenvolvimento)
dotnet add package Codout.Framework.Storage.FileSystem
```

## ?? Configuração

### Azure Blob Storage

```csharp
// appsettings.json
{
  "ConnectionStrings": {
    "AzureStorage": "DefaultEndpointsProtocol=https;AccountName=...;AccountKey=...;EndpointSuffix=core.windows.net"
  }
}

// Program.cs - Básico
services.AddAzureStorage(configuration);

// Program.cs - Avançado
services.AddAzureStorage(options =>
{
    options.ConnectionString = configuration.GetConnectionString("AzureStorage");
    options.DefaultContainer = "uploads";
    options.AutoCreateContainer = true;
    options.EnableCdn = true;
    options.CdnEndpoint = "https://mycdn.azureedge.net";
    options.MaxRetryAttempts = 3;
    options.PublicAccessType = "Blob";
});
```

## ?? Uso Básico

### Upload

```csharp
public class FileService
{
    private readonly IStorage _storage;

    public FileService(IStorage storage)
    {
        _storage = storage;
    }

    public async Task<Uri> UploadFileAsync(Stream file, string fileName, CancellationToken ct = default)
    {
        return await _storage.UploadAsync(file, "documents", fileName, ct);
    }
    
    // Com metadata
    public async Task<Uri> UploadWithMetadataAsync(Stream file, string fileName, CancellationToken ct = default)
    {
        var metadata = new Dictionary<string, string>
        {
            ["userId"] = "123",
            ["uploadDate"] = DateTime.UtcNow.ToString("O")
        };
        
        return await _storage.UploadAsync(file, "documents", fileName, metadata, ct);
    }
    
    // Com progress
    public async Task<Uri> UploadWithProgressAsync(Stream file, string fileName, IProgress<long> progress, CancellationToken ct = default)
    {
        return await _storage.UploadAsync(file, "documents", fileName, progress, ct);
    }
}
```

### Download

```csharp
public async Task<Stream> DownloadFileAsync(string fileName, CancellationToken ct = default)
{
    return await _storage.DownloadAsync("documents", fileName, ct);
}

// Stream somente leitura (mais eficiente)
public async Task<Stream> GetStreamAsync(string fileName, CancellationToken ct = default)
{
    return await _storage.GetStreamAsync("documents", fileName, ct);
}
```

### Delete

```csharp
// Deletar um arquivo
await _storage.DeleteAsync("documents", "file.pdf", ct);

// Deletar múltiplos arquivos
var files = new[] { "file1.pdf", "file2.pdf", "file3.pdf" };
await _storage.DeleteManyAsync("documents", files, ct);
```

### Copy e Move

```csharp
// Copiar
var newUri = await _storage.CopyToAsync("source-container", "dest-container", "file.pdf", ct);

// Mover
var movedUri = await _storage.MoveToAsync("source-container", "dest-container", "file.pdf", ct);
```

### List Files

```csharp
// Listar todos
var files = await _storage.ListAsync("documents", ct);

foreach (var file in files)
{
    Console.WriteLine($"{file.Name} - {file.Size} bytes - {file.LastModified}");
}

// Listar com prefixo
var pdfs = await _storage.ListAsync("documents", "invoices/", ct);
```

### Metadata

```csharp
// Obter metadata
var metadata = await _storage.GetMetadataAsync("documents", "file.pdf", ct);
Console.WriteLine($"Type: {metadata.ContentType}, Size: {metadata.Size}");

// Setar metadata customizada
var customData = new Dictionary<string, string>
{
    ["category"] = "invoice",
    ["year"] = "2024"
};
await _storage.SetMetadataAsync("documents", "file.pdf", customData, ct);
```

### SAS Tokens (Acesso Temporário)

```csharp
// Gerar URL com acesso temporário de 1 hora
var sasUri = await _storage.GetSasUriAsync("documents", "file.pdf", TimeSpan.FromHours(1), ct);

// Enviar ao cliente
return Ok(new { downloadUrl = sasUri.ToString() });
```

### Verificar Existência

```csharp
if (await _storage.ExistsAsync("documents", "file.pdf", ct))
{
    Console.WriteLine("File exists!");
}
```

## ?? Operações Avançadas

### Upload com Progress Bar

```csharp
public async Task<Uri> UploadWithProgressBarAsync(Stream file, string fileName)
{
    var progress = new Progress<long>(bytesUploaded =>
    {
        var percentage = (bytesUploaded * 100) / file.Length;
        Console.WriteLine($"Uploaded: {percentage}%");
    });

    return await _storage.UploadAsync(file, "documents", fileName, progress);
}
```

### Tratamento de Erros Específicos

```csharp
try
{
    await _storage.DownloadAsync("documents", "file.pdf", ct);
}
catch (StorageNotFoundException ex)
{
    // Arquivo não encontrado
    return NotFound($"File not found: {ex.FileName}");
}
catch (StorageContainerException ex)
{
    // Erro no container
    return BadRequest($"Container error: {ex.Message}");
}
catch (StorageException ex)
{
    // Erro genérico de storage
    return StatusCode(500, $"Storage error: {ex.Message}");
}
```

### Múltiplos Provedores

```csharp
// Registrar múltiplos storage providers
services.AddAzureStorage(azureOptions);
services.AddKeyedSingleton<IStorage>("aws", new AwsStorage(awsOptions));
services.AddKeyedSingleton<IStorage>("local", new FileSystemStorage(fsOptions));

// Usar específico
public class FileService
{
    private readonly IStorage _primaryStorage;
    private readonly IStorage _backupStorage;

    public FileService(
        IStorage primaryStorage,
        [FromKeyedServices("aws")] IStorage backupStorage)
    {
        _primaryStorage = primaryStorage;
        _backupStorage = backupStorage;
    }
}
```

## ?? Modelos

### StorageItem

```csharp
public class StorageItem
{
    public string Name { get; set; }
    public Uri Uri { get; set; }
    public string ContentType { get; set; }
    public long Size { get; set; }
    public DateTimeOffset LastModified { get; set; }
    public string? ETag { get; set; }
    public bool IsDirectory { get; set; }
}
```

### StorageMetadata

```csharp
public class StorageMetadata
{
    public string ContentType { get; set; }
    public long Size { get; set; }
    public DateTimeOffset LastModified { get; set; }
    public string? ETag { get; set; }
    public IDictionary<string, string> CustomMetadata { get; set; }
    public string? ContentEncoding { get; set; }
    public string? CacheControl { get; set; }
}
```

## ?? Melhores Práticas

1. **Use CancellationToken** em todas operações async
2. **Use GetStreamAsync** em vez de DownloadAsync para arquivos grandes (streaming)
3. **Implemente retry logic** para operações críticas
4. **Use SAS tokens** para acesso temporário em vez de URLs públicas
5. **Configure CDN** para melhor performance de leitura
6. **Use metadata** para armazenar informações contextuais
7. **Valide nomes de arquivos** antes do upload
8. **Implemente progress reporting** para uploads grandes
9. **Use batch operations** para deletar múltiplos arquivos
10. **Trate exceções específicas** (`StorageNotFoundException`, etc.)

## ?? Novidades v10.0

- ? **Nova interface IStorage** com naming convention async
- ? **Metadata support** completo
- ? **SAS tokens** para acesso temporário
- ? **List operations** com prefixo
- ? **Batch delete** para múltiplos arquivos
- ? **Progress reporting** em uploads
- ? **Exceções customizadas** tipadas
- ? **CDN support** integrado
- ? **Thread-safe** lazy loading
- ? **Extensões DI** fluentes
- ? **Nullable reference types**

## ?? Licença

Propriedade da Codout

---

**Versão:** 10.0.0  
**Status:** Estável para produção  
**Target:** .NET 10
