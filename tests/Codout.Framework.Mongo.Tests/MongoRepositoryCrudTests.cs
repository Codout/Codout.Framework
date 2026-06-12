using FluentAssertions;
using MongoDB.Bson;
using MongoDB.Driver;
using Xunit;

namespace Codout.Framework.Mongo.Tests;

[Collection("Mongo")]
public class MongoRepositoryCrudTests
{
    private readonly MongoFixture _fixture;
    private readonly MongoRepository<Gadget> _repository;

    public MongoRepositoryCrudTests(MongoFixture fixture)
    {
        _fixture = fixture;
        if (_fixture.IsAvailable)
            _fixture.ResetCollection();
        _repository = _fixture.IsAvailable ? _fixture.CreateRepository() : null!;
    }

    [SkippableFact]
    public void Save_DevePersistirEGerarObjectId()
    {
        _fixture.EnsureAvailable();

        var gadget = _repository.Save(new Gadget { Name = "Sensor", Price = 10 });

        gadget.Id.Should().NotBe(ObjectId.Empty, "o driver atribui o _id no InsertOne");
        gadget.IsTransient().Should().BeFalse();

        _fixture.GadgetCollection.CountDocuments(g => g.Id == gadget.Id).Should().Be(1);
    }

    [SkippableFact]
    public void Save_DeveUsarColecaoComNomeDoTipoEmMinusculas()
    {
        _fixture.EnsureAvailable();

        _repository.Save(new Gadget { Name = "Nome" });

        // MongoRepository<T> resolve a coleção como typeof(T).Name.ToLowerInvariant().
        var names = _fixture.Database!.ListCollectionNames().ToList();
        names.Should().Contain("gadget");
    }

    [SkippableFact]
    public void Get_PorChaveString_DeveRetornarEntidade()
    {
        _fixture.EnsureAvailable();
        var seeded = _fixture.Seed("Chip", 5);

        var found = _repository.Get(seeded.Id.ToString());

        found.Should().NotBeNull();
        found.Name.Should().Be("Chip");
    }

    [SkippableFact]
    public void Get_PorChaveObjectId_DeveRetornarEntidade()
    {
        _fixture.EnsureAvailable();
        var seeded = _fixture.Seed("Placa", 7);

        var found = _repository.Get(seeded.Id);

        found.Should().NotBeNull();
        found.Price.Should().Be(7);
    }

    [SkippableFact]
    public void Get_PorChaveInexistente_DeveRetornarNull()
    {
        _fixture.EnsureAvailable();

        _repository.Get(ObjectId.GenerateNewId()).Should().BeNull();
    }

    [SkippableFact]
    public void Get_PorPredicado_DeveRetornarEntidadeUnica()
    {
        _fixture.EnsureAvailable();
        _fixture.Seed("Cabo", 1);
        _fixture.Seed("Fonte", 2);

        var found = _repository.Get(g => g.Name == "Fonte");

        found.Should().NotBeNull();
        found.Price.Should().Be(2);
    }

    [SkippableFact]
    public void Get_PorPredicadoComMultiplosResultados_DeveLancarExcecao()
    {
        _fixture.EnsureAvailable();
        _fixture.Seed("Duplicado", 1);
        _fixture.Seed("Duplicado", 2);

        var act = () => _repository.Get(g => g.Name == "Duplicado");

        // Usa SingleOrDefault — mais de um resultado é erro.
        act.Should().Throw<InvalidOperationException>();
    }

    [SkippableFact]
    public void Load_DeveDelegarParaGet()
    {
        _fixture.EnsureAvailable();
        var seeded = _fixture.Seed("Conector", 3);

        _repository.Load(seeded.Id.ToString())!.Name.Should().Be("Conector");
        _repository.Load(ObjectId.GenerateNewId()).Should().BeNull();
    }

    [SkippableFact]
    public void Update_DeveSubstituirODocumento()
    {
        _fixture.EnsureAvailable();
        var seeded = _fixture.Seed("Antigo", 1);
        seeded.Name = "Novo";
        seeded.Price = 99;

        _repository.Update(seeded);

        var persisted = _repository.Get(seeded.Id);
        persisted.Name.Should().Be("Novo");
        persisted.Price.Should().Be(99);
    }

    [SkippableFact]
    public void SaveOrUpdate_ComEntidadeTransiente_DeveInserir()
    {
        _fixture.EnsureAvailable();

        var gadget = _repository.SaveOrUpdate(new Gadget { Name = "Inserido", Price = 1 });

        gadget.Id.Should().NotBe(ObjectId.Empty);
        _fixture.GadgetCollection.CountDocuments(g => g.Name == "Inserido").Should().Be(1);
    }

    [SkippableFact]
    public void SaveOrUpdate_ComEntidadePersistida_DeveSubstituir()
    {
        _fixture.EnsureAvailable();
        var seeded = _fixture.Seed("Persistido", 1);
        seeded.Price = 50;

        _repository.SaveOrUpdate(seeded);

        _fixture.GadgetCollection.CountDocuments(FilterDefinition<Gadget>.Empty).Should().Be(1);
        _repository.Get(seeded.Id).Price.Should().Be(50);
    }

    [SkippableFact]
    public void Merge_DeveSubstituirERetornarEntidade()
    {
        _fixture.EnsureAvailable();
        var seeded = _fixture.Seed("Mesclado", 1);
        seeded.Price = 33;

        var merged = _repository.Merge(seeded);

        merged.Should().BeSameAs(seeded);
        _repository.Get(seeded.Id).Price.Should().Be(33);
    }

    [SkippableFact]
    public void Refresh_DeveRecarregarDoBanco()
    {
        _fixture.EnsureAvailable();
        var seeded = _fixture.Seed("Original", 10);

        // Altera o documento por fora do repositório.
        _fixture.GadgetCollection.UpdateOne(
            g => g.Id == seeded.Id,
            Builders<Gadget>.Update.Set(g => g.Price, 123));

        var refreshed = _repository.Refresh(seeded);

        // Refresh devolve uma NOVA instância lida do banco (não atualiza a atual).
        refreshed.Should().NotBeSameAs(seeded);
        refreshed.Price.Should().Be(123);
        seeded.Price.Should().Be(10);
    }

    [SkippableFact]
    public void Delete_DeveRemoverODocumento()
    {
        _fixture.EnsureAvailable();
        var seeded = _fixture.Seed("Remover", 1);

        _repository.Delete(seeded);

        _fixture.GadgetCollection.CountDocuments(g => g.Id == seeded.Id).Should().Be(0);
    }

    [SkippableFact]
    public void Delete_PorPredicado_DeveRemoverApenasCorrespondentes()
    {
        _fixture.EnsureAvailable();
        _fixture.Seed("Apagar", 1);
        _fixture.Seed("Apagar", 2);
        _fixture.Seed("Manter", 3);

        _repository.Delete(g => g.Name == "Apagar");

        var remaining = _repository.All().ToList();
        remaining.Should().ContainSingle(g => g.Name == "Manter");
    }
}
