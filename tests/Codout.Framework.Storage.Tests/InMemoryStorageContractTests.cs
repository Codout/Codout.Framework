using Codout.Framework.Storage.Exceptions;
using FluentAssertions;
using Xunit;

namespace Codout.Framework.Storage.Tests;

/// <summary>
/// O pacote Codout.Framework.Storage só define a abstração (não há implementação
/// local/file system no repositório). Estes testes usam um fake em memória para
/// verificar que o contrato IStorage é implementável de ponta a ponta e para
/// documentar a semântica esperada das operações (upload/download/exists/list/
/// delete/copy/move/metadata/URIs).
/// </summary>
public class InMemoryStorageContractTests
{
    private readonly InMemoryStorage _storage = new();

    private static MemoryStream StreamOf(string conteudo) =>
        new(System.Text.Encoding.UTF8.GetBytes(conteudo));

    [Fact]
    public async Task UploadAsync_DeveRetornarUriDoArquivo()
    {
        var uri = await _storage.UploadAsync(StreamOf("abc"), "docs", "a.txt");

        uri.Should().Be(new Uri("memory://docs/a.txt"));
    }

    [Fact]
    public async Task DownloadAsync_DeveRetornarConteudoEnviado()
    {
        await _storage.UploadAsync(StreamOf("conteúdo"), "docs", "a.txt");

        using var stream = await _storage.DownloadAsync("docs", "a.txt");
        using var reader = new StreamReader(stream);

        (await reader.ReadToEndAsync()).Should().Be("conteúdo");
    }

    [Fact]
    public async Task DownloadAsync_ArquivoInexistente_LancaStorageNotFound()
    {
        var acao = () => _storage.DownloadAsync("docs", "nao-existe.txt");

        await acao.Should().ThrowAsync<StorageNotFoundException>();
    }

    [Fact]
    public async Task ExistsAsync_DeveRefletirEstadoDoContainer()
    {
        (await _storage.ExistsAsync("docs", "a.txt")).Should().BeFalse();

        await _storage.UploadAsync(StreamOf("x"), "docs", "a.txt");

        (await _storage.ExistsAsync("docs", "a.txt")).Should().BeTrue();
    }

    [Fact]
    public async Task DeleteAsync_DeveRemoverArquivo()
    {
        await _storage.UploadAsync(StreamOf("x"), "docs", "a.txt");

        await _storage.DeleteAsync("docs", "a.txt");

        (await _storage.ExistsAsync("docs", "a.txt")).Should().BeFalse();
    }

    [Fact]
    public async Task DeleteManyAsync_DeveRemoverVariosArquivos()
    {
        await _storage.UploadAsync(StreamOf("1"), "docs", "a.txt");
        await _storage.UploadAsync(StreamOf("2"), "docs", "b.txt");

        await _storage.DeleteManyAsync("docs", ["a.txt", "b.txt"]);

        (await _storage.ListAsync("docs")).Should().BeEmpty();
    }

    [Fact]
    public async Task ListAsync_ComPrefixo_DeveFiltrar()
    {
        await _storage.UploadAsync(StreamOf("1"), "docs", "rel/a.txt");
        await _storage.UploadAsync(StreamOf("2"), "docs", "img/b.png");

        var itens = await _storage.ListAsync("docs", "rel/");

        itens.Should().ContainSingle().Which.Name.Should().Be("rel/a.txt");
    }

    [Fact]
    public async Task CopyToAsync_DeveManterOriginal()
    {
        await _storage.UploadAsync(StreamOf("x"), "origem", "a.txt");

        await _storage.CopyToAsync("origem", "destino", "a.txt");

        (await _storage.ExistsAsync("origem", "a.txt")).Should().BeTrue();
        (await _storage.ExistsAsync("destino", "a.txt")).Should().BeTrue();
    }

    [Fact]
    public async Task MoveToAsync_DeveRemoverOriginal()
    {
        await _storage.UploadAsync(StreamOf("x"), "origem", "a.txt");

        await _storage.MoveToAsync("origem", "destino", "a.txt");

        (await _storage.ExistsAsync("origem", "a.txt")).Should().BeFalse();
        (await _storage.ExistsAsync("destino", "a.txt")).Should().BeTrue();
    }

    [Fact]
    public async Task Metadata_DevePersistirCustomMetadata()
    {
        await _storage.UploadAsync(StreamOf("x"), "docs", "a.txt",
            new Dictionary<string, string> { ["autor"] = "codout" });

        var metadata = await _storage.GetMetadataAsync("docs", "a.txt");
        metadata.CustomMetadata.Should().ContainKey("autor").WhoseValue.Should().Be("codout");
        metadata.Size.Should().Be(1);

        await _storage.SetMetadataAsync("docs", "a.txt", new Dictionary<string, string> { ["v"] = "2" });

        (await _storage.GetMetadataAsync("docs", "a.txt"))
            .CustomMetadata.Should().ContainKey("v");
    }

    [Fact]
    public async Task UploadAsync_ComProgress_DeveReportarTamanho()
    {
        long? reportado = null;
        var progress = new Progress<long>(v => reportado = v);

        await _storage.UploadAsync(StreamOf("12345"), "docs", "a.txt", progress);

        // Progress<T> agenda callbacks; o fake reporta sincronamente via IProgress
        await Task.Delay(50);
        reportado.Should().Be(5);
    }

    [Fact]
    public void GetBlobUri_DeveMontarUriDeterministica()
    {
        _storage.GetBlobUri("docs", "a.txt").Should().Be(new Uri("memory://docs/a.txt"));
    }

    [Fact]
    public async Task GetSasUriAsync_DeveEmbutirExpiracao()
    {
        await _storage.UploadAsync(StreamOf("x"), "docs", "a.txt");

        var uri = await _storage.GetSasUriAsync("docs", "a.txt", TimeSpan.FromHours(1));

        uri.Query.Should().Contain("expires=");
    }

    #region Fake em memória

    private sealed class InMemoryStorage : IStorage
    {
        private sealed record Entry(byte[] Content, Dictionary<string, string> Metadata, DateTimeOffset LastModified);

        private readonly Dictionary<string, Dictionary<string, Entry>> _containers = new();

        private Dictionary<string, Entry> Container(string container)
        {
            if (!_containers.TryGetValue(container, out var c))
                _containers[container] = c = new Dictionary<string, Entry>();
            return c;
        }

        private Entry GetEntryOrThrow(string container, string fileName)
        {
            if (!Container(container).TryGetValue(fileName, out var entry))
                throw new StorageNotFoundException(container, fileName);
            return entry;
        }

        private static Uri UriOf(string container, string fileName) => new($"memory://{container}/{fileName}");

        public Task<Uri> UploadAsync(Stream file, string container, string fileName, CancellationToken cancellationToken = default)
            => UploadAsync(file, container, fileName, (IDictionary<string, string>?)null, cancellationToken);

        public Task<Uri> UploadAsync(Stream file, string container, string fileName, IDictionary<string, string>? metadata, CancellationToken cancellationToken = default)
        {
            using var ms = new MemoryStream();
            file.CopyTo(ms);
            Container(container)[fileName] = new Entry(ms.ToArray(),
                new Dictionary<string, string>(metadata ?? new Dictionary<string, string>()), DateTimeOffset.UtcNow);
            return Task.FromResult(UriOf(container, fileName));
        }

        public async Task<Uri> UploadAsync(Stream file, string container, string fileName, IProgress<long>? progress, CancellationToken cancellationToken = default)
        {
            var uri = await UploadAsync(file, container, fileName, (IDictionary<string, string>?)null, cancellationToken);
            progress?.Report(Container(container)[fileName].Content.LongLength);
            return uri;
        }

        public Task<Stream> DownloadAsync(string container, string fileName, CancellationToken cancellationToken = default)
            => Task.FromResult<Stream>(new MemoryStream(GetEntryOrThrow(container, fileName).Content));

        public Task<Stream> GetStreamAsync(string container, string fileName, CancellationToken cancellationToken = default)
            => DownloadAsync(container, fileName, cancellationToken);

        public Task DeleteAsync(string container, string fileName, CancellationToken cancellationToken = default)
        {
            Container(container).Remove(fileName);
            return Task.CompletedTask;
        }

        public Task DeleteManyAsync(string container, IEnumerable<string> fileNames, CancellationToken cancellationToken = default)
        {
            foreach (var fileName in fileNames)
                Container(container).Remove(fileName);
            return Task.CompletedTask;
        }

        public async Task<Uri> MoveToAsync(string fromContainer, string toContainer, string fileName, CancellationToken cancellationToken = default)
        {
            var uri = await CopyToAsync(fromContainer, toContainer, fileName, cancellationToken);
            await DeleteAsync(fromContainer, fileName, cancellationToken);
            return uri;
        }

        public Task<Uri> CopyToAsync(string fromContainer, string toContainer, string fileName, CancellationToken cancellationToken = default)
        {
            var entry = GetEntryOrThrow(fromContainer, fileName);
            Container(toContainer)[fileName] = entry with { LastModified = DateTimeOffset.UtcNow };
            return Task.FromResult(UriOf(toContainer, fileName));
        }

        public Task<bool> ExistsAsync(string container, string fileName, CancellationToken cancellationToken = default)
            => Task.FromResult(Container(container).ContainsKey(fileName));

        public Task<IEnumerable<StorageItem>> ListAsync(string container, CancellationToken cancellationToken = default)
            => ListAsync(container, null, cancellationToken);

        public Task<IEnumerable<StorageItem>> ListAsync(string container, string? prefix, CancellationToken cancellationToken = default)
        {
            var itens = Container(container)
                .Where(kvp => prefix == null || kvp.Key.StartsWith(prefix, StringComparison.Ordinal))
                .Select(kvp => new StorageItem
                {
                    Name = kvp.Key,
                    Uri = UriOf(container, kvp.Key),
                    Size = kvp.Value.Content.LongLength,
                    LastModified = kvp.Value.LastModified
                })
                .ToList();

            return Task.FromResult<IEnumerable<StorageItem>>(itens);
        }

        public Task<StorageMetadata> GetMetadataAsync(string container, string fileName, CancellationToken cancellationToken = default)
        {
            var entry = GetEntryOrThrow(container, fileName);
            return Task.FromResult(new StorageMetadata
            {
                Size = entry.Content.LongLength,
                LastModified = entry.LastModified,
                CustomMetadata = new Dictionary<string, string>(entry.Metadata)
            });
        }

        public Task SetMetadataAsync(string container, string fileName, IDictionary<string, string> metadata, CancellationToken cancellationToken = default)
        {
            var entry = GetEntryOrThrow(container, fileName);
            Container(container)[fileName] = entry with { Metadata = new Dictionary<string, string>(metadata) };
            return Task.CompletedTask;
        }

        public Uri GetBlobUri(string container, string fileName) => UriOf(container, fileName);

        public Task<Uri> GetSasUriAsync(string container, string fileName, TimeSpan expiresIn, CancellationToken cancellationToken = default)
        {
            GetEntryOrThrow(container, fileName);
            var expires = DateTimeOffset.UtcNow.Add(expiresIn).ToUnixTimeSeconds();
            return Task.FromResult(new Uri($"memory://{container}/{fileName}?expires={expires}"));
        }
    }

    #endregion
}
