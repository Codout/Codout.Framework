using Codout.Framework.Storage.Azure;
using Codout.Framework.Storage.Configuration;
using FluentAssertions;
using Xunit;

namespace Codout.Framework.Storage.Azure.Tests;

/// <summary>
/// Testes unitários puros — nenhum teste aqui toca rede ou conta Azure real.
/// O AzureStorage cria o BlobServiceClient de forma lazy, então construção e
/// validação de argumentos são totalmente testáveis offline.
/// </summary>
public class AzureStorageConstructionTests
{
    [Fact]
    public void Ctor_ComOptionsNulas_LancaArgumentNullException()
    {
        var acao = () => new AzureStorage((AzureStorageOptions)null!);

        acao.Should().Throw<ArgumentNullException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Ctor_SemConnectionString_LancaArgumentException(string? connectionString)
    {
        var acao = () => new AzureStorage(new AzureStorageOptions { ConnectionString = connectionString });

        acao.Should().Throw<ArgumentException>()
            .WithMessage("ConnectionString is required.*")
            .And.ParamName.Should().Be("options");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Ctor_StringOverload_SemConnectionString_LancaArgumentException(string connectionString)
    {
        var acao = () => new AzureStorage(connectionString);

        acao.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Ctor_ComConnectionStringDeDesenvolvimento_NaoLanca()
    {
        var acao = () => new AzureStorage(TestConnectionStrings.Development);

        acao.Should().NotThrow();
    }

    [Fact]
    public void Ctor_ComConnectionStringInvalida_NaoLancaPorqueClientELazy()
    {
        // Caracterização: o BlobServiceClient só é criado no primeiro uso (Lazy),
        // então uma connection string sintaticamente inválida não falha na construção.
        var acao = () => new AzureStorage("isto-nao-e-uma-connection-string");

        acao.Should().NotThrow();
    }

    [Fact]
    public void GetBlobUri_ComConnectionStringInvalida_FalhaAoMaterializarClient()
    {
        // Caracterização: o erro de connection string inválida só aparece quando o
        // client é materializado (primeiro acesso), e escapa como FormatException
        // do SDK em vez de StorageException da abstração.
        var storage = new AzureStorage("isto-nao-e-uma-connection-string");

        var acao = () => storage.GetBlobUri("docs", "a.txt");

        acao.Should().Throw<FormatException>();
    }
}

internal static class TestConnectionStrings
{
    /// <summary>
    /// Connection string padrão do emulador (Azurite). Nenhum teste conecta de fato:
    /// é usada apenas para o SDK construir URIs localmente.
    /// </summary>
    public const string Development = "UseDevelopmentStorage=true";

    /// <summary>
    /// Connection string sintaticamente válida de conta fictícia (chave é base64 dummy).
    /// </summary>
    public static readonly string FakeAccount =
        "DefaultEndpointsProtocol=https;AccountName=contateste;AccountKey=" +
        Convert.ToBase64String("chave-de-teste-nao-e-um-segredo-real-0001"u8.ToArray()) +
        ";EndpointSuffix=core.windows.net";
}
