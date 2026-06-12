using Codout.Framework.Data.Repository;
using Codout.Framework.Mongo.Configuration;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using Xunit;

namespace Codout.Framework.Mongo.Tests;

/// <summary>
/// Testes do AddMongoDb (validação de opções + registros de DI). Nenhum deles
/// precisa de servidor: o MongoClient não abre conexão na construção.
/// </summary>
public class ConfigureServicesTests
{
    private const string ConnectionString = "mongodb://localhost:27017/?serverSelectionTimeoutMS=100";

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void AddMongoDb_ComConnectionStringInvalida_DeveLancarArgumentNullException(string? connectionString)
    {
        var services = new ServiceCollection();

        var act = () => services.AddMongoDb(connectionString!, "db");

        act.Should().Throw<ArgumentNullException>().WithParameterName("connectionString");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void AddMongoDb_ComDatabaseNameInvalido_DeveLancarArgumentNullException(string? databaseName)
    {
        var services = new ServiceCollection();

        var act = () => services.AddMongoDb(ConnectionString, databaseName!);

        act.Should().Throw<ArgumentNullException>().WithParameterName("databaseName");
    }

    [Fact]
    public void AddMongoDb_DeveRetornarAMesmaColecaoDeServicos()
    {
        var services = new ServiceCollection();

        services.AddMongoDb(ConnectionString, "db").Should().BeSameAs(services);
    }

    [Fact]
    public void AddMongoDb_DeveRegistrarClientEDatabaseComoSingleton()
    {
        var services = new ServiceCollection();
        services.AddMongoDb(ConnectionString, "minha_base");

        using var provider = services.BuildServiceProvider();

        var client = provider.GetRequiredService<IMongoClient>();
        provider.GetRequiredService<IMongoClient>().Should().BeSameAs(client);

        var database = provider.GetRequiredService<IMongoDatabase>();
        database.DatabaseNamespace.DatabaseName.Should().Be("minha_base");
        provider.GetRequiredService<IMongoDatabase>().Should().BeSameAs(database);
    }

    [Fact]
    public void AddMongoDb_DeveRegistrarIRepositoryGenericoComoMongoRepository()
    {
        var services = new ServiceCollection();
        services.AddMongoDb(ConnectionString, "db");

        using var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();

        var repository = scope.ServiceProvider.GetRequiredService<IRepository<Gadget>>();

        repository.Should().BeOfType<MongoRepository<Gadget>>();
    }

    [Fact]
    public void AddMongoDb_RepositorioDeveSerScoped()
    {
        var services = new ServiceCollection();
        services.AddMongoDb(ConnectionString, "db");

        using var provider = services.BuildServiceProvider();

        using var scope1 = provider.CreateScope();
        var a = scope1.ServiceProvider.GetRequiredService<IRepository<Gadget>>();
        var b = scope1.ServiceProvider.GetRequiredService<IRepository<Gadget>>();
        a.Should().BeSameAs(b, "dentro do mesmo scope a instância é reutilizada");

        using var scope2 = provider.CreateScope();
        var c = scope2.ServiceProvider.GetRequiredService<IRepository<Gadget>>();
        c.Should().NotBeSameAs(a, "scopes diferentes têm instâncias diferentes");
    }
}
