using Codout.Framework.Storage.Azure;
using FluentAssertions;
using Xunit;

namespace Codout.Framework.Storage.Azure.Tests;

/// <summary>
/// As validações de argumentos do AzureStorage ocorrem antes de qualquer I/O,
/// então são testáveis sem Azurite/conta Azure. Operações que exigem requisição
/// real (Exists/Download/List/SAS de blob existente etc.) não são cobertas aqui
/// — veja tests/FINDINGS-C.md.
/// </summary>
public class AzureStorageArgumentValidationTests
{
    private readonly AzureStorage _storage = new(TestConnectionStrings.Development);

    private static MemoryStream AnyStream() => new([1, 2, 3]);

    [Fact]
    public async Task UploadAsync_ComStreamNulo_LancaArgumentNullException()
    {
        var acao = () => _storage.UploadAsync(null!, "docs", "a.txt");

        await acao.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task UploadAsync_ComMetadata_ComStreamNulo_LancaArgumentNullException()
    {
        var acao = () => _storage.UploadAsync(null!, "docs", "a.txt",
            new Dictionary<string, string>());

        await acao.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task UploadAsync_ComProgress_ComStreamNulo_LancaArgumentNullException()
    {
        var acao = () => _storage.UploadAsync(null!, "docs", "a.txt", new Progress<long>());

        await acao.Should().ThrowAsync<ArgumentNullException>();
    }

    [Theory]
    [InlineData(null, "a.txt", "container")]
    [InlineData("", "a.txt", "container")]
    [InlineData("  ", "a.txt", "container")]
    [InlineData("docs", null, "fileName")]
    [InlineData("docs", "", "fileName")]
    [InlineData("docs", "  ", "fileName")]
    public async Task UploadAsync_ComContainerOuArquivoInvalido_LancaArgumentException(
        string? container, string? fileName, string paramName)
    {
        using var stream = AnyStream();

        var acao = () => _storage.UploadAsync(stream, container!, fileName!);

        (await acao.Should().ThrowAsync<ArgumentException>())
            .And.ParamName.Should().Be(paramName);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task DownloadAsync_ComContainerInvalido_LancaArgumentException(string? container)
    {
        var acao = () => _storage.DownloadAsync(container!, "a.txt");

        await acao.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task GetStreamAsync_ComArquivoInvalido_LancaArgumentException()
    {
        var acao = () => _storage.GetStreamAsync("docs", " ");

        await acao.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task DeleteAsync_ComArquivoInvalido_LancaArgumentException()
    {
        var acao = () => _storage.DeleteAsync("docs", "");

        await acao.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task DeleteManyAsync_ComListaNula_LancaArgumentNullException()
    {
        var acao = () => _storage.DeleteManyAsync("docs", null!);

        await acao.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task DeleteManyAsync_ComContainerInvalido_LancaArgumentException()
    {
        var acao = () => _storage.DeleteManyAsync(" ", ["a.txt"]);

        (await acao.Should().ThrowAsync<ArgumentException>())
            .And.ParamName.Should().Be("container");
    }

    [Fact]
    public async Task MoveToAsync_ComDestinoInvalido_LancaArgumentException()
    {
        var acao = () => _storage.MoveToAsync("origem", "", "a.txt");

        (await acao.Should().ThrowAsync<ArgumentException>())
            .And.ParamName.Should().Be("toContainer");
    }

    [Fact]
    public async Task CopyToAsync_ComDestinoInvalido_LancaArgumentException()
    {
        var acao = () => _storage.CopyToAsync("origem", "  ", "a.txt");

        (await acao.Should().ThrowAsync<ArgumentException>())
            .And.ParamName.Should().Be("toContainer");
    }

    [Fact]
    public async Task CopyToAsync_ComOrigemInvalida_LancaArgumentException()
    {
        var acao = () => _storage.CopyToAsync("", "destino", "a.txt");

        (await acao.Should().ThrowAsync<ArgumentException>())
            .And.ParamName.Should().Be("container");
    }

    [Fact]
    public async Task ListAsync_ComContainerInvalido_LancaArgumentException()
    {
        var acao = () => _storage.ListAsync("");

        (await acao.Should().ThrowAsync<ArgumentException>())
            .And.ParamName.Should().Be("container");
    }

    [Fact]
    public async Task GetMetadataAsync_ComArquivoInvalido_LancaArgumentException()
    {
        var acao = () => _storage.GetMetadataAsync("docs", "");

        await acao.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task SetMetadataAsync_ComMetadataNula_LancaArgumentNullException()
    {
        var acao = () => _storage.SetMetadataAsync("docs", "a.txt", null!);

        await acao.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task GetSasUriAsync_ComContainerInvalido_LancaArgumentException()
    {
        var acao = () => _storage.GetSasUriAsync("", "a.txt", TimeSpan.FromHours(1));

        await acao.Should().ThrowAsync<ArgumentException>();
    }

    [Theory]
    [InlineData("", "a.txt")]
    [InlineData("docs", "")]
    public void GetBlobUri_ComArgumentosInvalidos_LancaArgumentException(string container, string fileName)
    {
        var acao = () => _storage.GetBlobUri(container, fileName);

        acao.Should().Throw<ArgumentException>();
    }
}
