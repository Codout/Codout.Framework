using Codout.Framework.Storage;
using Codout.Framework.Storage.Azure;
using Codout.Framework.Storage.Configuration;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Codout.Framework.Storage.Azure.Tests;

public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddAzureStorage_ComConfiguration_SemConnectionString_LancaInvalidOperationException()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>())
            .Build();

        var acao = () => new ServiceCollection().AddAzureStorage(configuration);

        acao.Should().Throw<InvalidOperationException>()
            .WithMessage("AzureStorage connection string not found in configuration.");
    }

    [Fact]
    public void AddAzureStorage_ComConfiguration_DeveRegistrarIStorageSingleton()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:AzureStorage"] = TestConnectionStrings.Development
            })
            .Build();

        var provider = new ServiceCollection()
            .AddAzureStorage(configuration)
            .BuildServiceProvider();

        var storage = provider.GetRequiredService<IStorage>();

        storage.Should().BeOfType<AzureStorage>();
        provider.GetRequiredService<IStorage>().Should().BeSameAs(storage, "deve ser singleton");
    }

    [Fact]
    public void AddAzureStorage_ComConnectionString_DeveRegistrarIStorage()
    {
        var provider = new ServiceCollection()
            .AddAzureStorage(TestConnectionStrings.Development)
            .BuildServiceProvider();

        provider.GetRequiredService<IStorage>().Should().BeOfType<AzureStorage>();
    }

    [Fact]
    public void AddAzureStorage_ComOptionsNulas_LancaArgumentNullException()
    {
        var acao = () => new ServiceCollection().AddAzureStorage((AzureStorageOptions)null!);

        acao.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void AddAzureStorage_ComOptionsSemConnectionString_FalhaNoRegistro()
    {
        // Caracterização: a instância de AzureStorage é criada eagerly no momento
        // do registro (AddSingleton com instância), então options inválidas falham
        // já no AddAzureStorage, não no primeiro resolve.
        var acao = () => new ServiceCollection().AddAzureStorage(new AzureStorageOptions());

        acao.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void AddAzureStorage_ComDelegateNulo_LancaArgumentNullException()
    {
        var acao = () => new ServiceCollection().AddAzureStorage((Action<AzureStorageOptions>)null!);

        acao.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void AddAzureStorage_ComDelegate_DeveAplicarConfiguracao()
    {
        var provider = new ServiceCollection()
            .AddAzureStorage(options =>
            {
                options.ConnectionString = TestConnectionStrings.FakeAccount;
                options.EnableCdn = true;
                options.CdnEndpoint = "https://cdn.exemplo.com";
            })
            .BuildServiceProvider();

        var storage = (AzureStorage)provider.GetRequiredService<IStorage>();

        storage.GetBlobUri("docs", "a.txt")
            .Should().Be(new Uri("https://cdn.exemplo.com/docs/a.txt"));
    }
}
