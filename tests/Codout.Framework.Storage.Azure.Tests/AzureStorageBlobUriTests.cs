using Codout.Framework.Storage.Azure;
using Codout.Framework.Storage.Configuration;
using FluentAssertions;
using Xunit;

namespace Codout.Framework.Storage.Azure.Tests;

/// <summary>
/// GetBlobUri é construído localmente pelo SDK (sem requisição), o que permite
/// testar a montagem de URIs (incluindo CDN) totalmente offline.
/// </summary>
public class AzureStorageBlobUriTests
{
    [Fact]
    public void GetBlobUri_ComContaFicticia_DeveMontarUriDoBlobEndpoint()
    {
        var storage = new AzureStorage(TestConnectionStrings.FakeAccount);

        var uri = storage.GetBlobUri("docs", "relatorio.pdf");

        uri.Should().Be(new Uri("https://contateste.blob.core.windows.net/docs/relatorio.pdf"));
    }

    [Fact]
    public void GetBlobUri_DeveNormalizarContainerParaMinusculas()
    {
        var storage = new AzureStorage(TestConnectionStrings.FakeAccount);

        var uri = storage.GetBlobUri("MeuContainer", "a.txt");

        uri.AbsolutePath.Should().StartWith("/meucontainer/");
    }

    [Fact]
    public void GetBlobUri_ComEmuladorDeDesenvolvimento_DeveUsarEndpointLocal()
    {
        var storage = new AzureStorage(TestConnectionStrings.Development);

        var uri = storage.GetBlobUri("docs", "a.txt");

        uri.Should().Be(new Uri("http://127.0.0.1:10000/devstoreaccount1/docs/a.txt"));
    }

    [Fact]
    public void GetBlobUri_ComCdnHabilitado_DeveUsarEndpointDoCdn()
    {
        var storage = new AzureStorage(new AzureStorageOptions
        {
            ConnectionString = TestConnectionStrings.FakeAccount,
            EnableCdn = true,
            CdnEndpoint = "https://cdn.exemplo.com/"
        });

        var uri = storage.GetBlobUri("Docs", "relatorio.pdf");

        // Trailing slash do endpoint é aparado e o container é normalizado.
        uri.Should().Be(new Uri("https://cdn.exemplo.com/docs/relatorio.pdf"));
    }

    [Fact]
    public void GetBlobUri_ComCdnHabilitadoMasSemEndpoint_DeveCairNoBlobEndpoint()
    {
        var storage = new AzureStorage(new AzureStorageOptions
        {
            ConnectionString = TestConnectionStrings.FakeAccount,
            EnableCdn = true,
            CdnEndpoint = "  "
        });

        var uri = storage.GetBlobUri("docs", "a.txt");

        uri.Host.Should().Be("contateste.blob.core.windows.net");
    }

    [Fact]
    public void GetBlobUri_ComCdn_NaoEscapaNomeDoArquivo()
    {
        // BUG?: o caminho do CDN é interpolado sem URL-encoding do fileName,
        // enquanto o blob endpoint (SDK) escapa caracteres especiais — as duas
        // formas geram URIs divergentes para o mesmo blob (ex.: espaço).
        // Teste de caracterização do comportamento atual.
        var comCdn = new AzureStorage(new AzureStorageOptions
        {
            ConnectionString = TestConnectionStrings.FakeAccount,
            EnableCdn = true,
            CdnEndpoint = "https://cdn.exemplo.com"
        });
        var semCdn = new AzureStorage(TestConnectionStrings.FakeAccount);

        var uriCdn = comCdn.GetBlobUri("docs", "meu arquivo.txt");
        var uriBlob = semCdn.GetBlobUri("docs", "meu arquivo.txt");

        uriCdn.OriginalString.Should().Be("https://cdn.exemplo.com/docs/meu arquivo.txt");
        uriBlob.AbsoluteUri.Should().Contain("meu%20arquivo.txt");
    }
}
