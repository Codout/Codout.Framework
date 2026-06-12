using System.Reflection;
using FluentAssertions;
using MongoDB.Driver;
using Xunit;

namespace Codout.Framework.Mongo.Tests;

/// <summary>
/// Testes de unidade do MongoRepository que NÃO precisam de servidor: o driver
/// só abre conexão quando uma operação de I/O é executada, então construção,
/// validação de argumentos e resolução de nome de coleção são verificáveis
/// com um client apontando para um endereço inválido.
/// </summary>
public class MongoRepositoryUnitTests
{
    private static MongoRepository<Gadget> CreateOfflineRepository(out IMongoDatabase database)
    {
        // Endereço inerte + timeout curto: nenhuma operação aqui faz I/O.
        var client = new MongoClient("mongodb://localhost:27017/?serverSelectionTimeoutMS=100");
        database = client.GetDatabase("offline_db");
        return new MongoRepository<Gadget>(database);
    }

    [Fact]
    public void Construtor_DeveResolverColecaoComNomeDoTipoEmMinusculas()
    {
        var repository = CreateOfflineRepository(out _);

        // O nome da coleção é detalhe interno — inspecionado via reflection.
        var field = typeof(MongoRepository<Gadget>)
            .GetField("_collection", BindingFlags.Instance | BindingFlags.NonPublic);
        field.Should().NotBeNull();

        var collection = (IMongoCollection<Gadget>)field!.GetValue(repository)!;
        collection.CollectionNamespace.CollectionName.Should().Be("gadget");
    }

    [Fact]
    public void Save_ComEntidadeNula_DeveLancarArgumentNullException()
    {
        var repository = CreateOfflineRepository(out _);

        var act = () => repository.Save(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void SaveOrUpdate_ComEntidadeNula_DeveLancarArgumentNullException()
    {
        var repository = CreateOfflineRepository(out _);

        var act = () => repository.SaveOrUpdate(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Update_ComEntidadeNula_DeveLancarArgumentNullException()
    {
        var repository = CreateOfflineRepository(out _);

        var act = () => repository.Update(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Delete_ComEntidadeNula_DeveLancarArgumentNullException()
    {
        var repository = CreateOfflineRepository(out _);

        var act = () => repository.Delete((Gadget)null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Merge_ComEntidadeNula_DeveLancarArgumentNullException()
    {
        var repository = CreateOfflineRepository(out _);

        var act = () => repository.Merge(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Refresh_ComEntidadeNula_DeveLancarArgumentNullException()
    {
        var repository = CreateOfflineRepository(out _);

        var act = () => repository.Refresh(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public async Task SaveAsync_ComEntidadeNula_DeveLancarArgumentNullException()
    {
        var repository = CreateOfflineRepository(out _);

        var act = () => repository.SaveAsync(null!);
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task SaveOrUpdateAsync_ComEntidadeNula_DeveLancarArgumentNullException()
    {
        var repository = CreateOfflineRepository(out _);

        var act = () => repository.SaveOrUpdateAsync(null!);
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task UpdateAsync_ComEntidadeNula_DeveLancarArgumentNullException()
    {
        var repository = CreateOfflineRepository(out _);

        var act = () => repository.UpdateAsync(null!);
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task DeleteAsync_ComEntidadeNula_DeveLancarArgumentNullException()
    {
        var repository = CreateOfflineRepository(out _);

        var act = () => repository.DeleteAsync((Gadget)null!);
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task MergeAsync_ComEntidadeNula_DeveLancarArgumentNullException()
    {
        var repository = CreateOfflineRepository(out _);

        var act = () => repository.MergeAsync(null!);
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task RefreshAsync_ComEntidadeNula_DeveLancarArgumentNullException()
    {
        var repository = CreateOfflineRepository(out _);

        var act = () => repository.RefreshAsync(null!);
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    // BUG?: Get(key)/GetAsync(key) só funcionam com chaves que parseiam como
    // ObjectId. Qualquer outra chave (int, Guid, string arbitrária — formatos
    // legítimos de _id no MongoDB) retorna null SILENCIOSAMENTE, sem tocar o
    // servidor e sem lançar erro. Registrado em tests/FINDINGS-D.md.
    [Fact]
    public void Get_ComChaveNaoObjectId_RetornaNullSemConsultarOServidor()
    {
        var repository = CreateOfflineRepository(out _);

        // Não há servidor — se a chamada tentasse consultar, lançaria timeout.
        repository.Get("nao-e-objectid").Should().BeNull();
        repository.Get(12345).Should().BeNull();
        repository.Get(Guid.NewGuid()).Should().BeNull();
        repository.Get((object)null!).Should().BeNull();
    }

    [Fact]
    public async Task GetAsync_ComChaveNaoObjectId_RetornaNullSemConsultarOServidor()
    {
        var repository = CreateOfflineRepository(out _);

        (await repository.GetAsync("nao-e-objectid")).Should().BeNull();
        (await repository.GetAsync((object)null!)).Should().BeNull();
    }

    [Fact]
    public async Task LoadAsync_ComChaveNaoObjectId_RetornaNullSemConsultarOServidor()
    {
        var repository = CreateOfflineRepository(out _);

        (await repository.LoadAsync("nao-e-objectid")).Should().BeNull();
    }

    [Fact]
    public void Dispose_DeveSerIdempotente()
    {
        var repository = CreateOfflineRepository(out _);

        var act = () =>
        {
            repository.Dispose();
            repository.Dispose();
        };

        act.Should().NotThrow();
    }
}
