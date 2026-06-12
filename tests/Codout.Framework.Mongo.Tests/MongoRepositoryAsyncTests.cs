using FluentAssertions;
using MongoDB.Bson;
using MongoDB.Driver;
using Xunit;

namespace Codout.Framework.Mongo.Tests;

[Collection("Mongo")]
public class MongoRepositoryAsyncTests
{
    private readonly MongoFixture _fixture;
    private readonly MongoRepository<Gadget> _repository;

    public MongoRepositoryAsyncTests(MongoFixture fixture)
    {
        _fixture = fixture;
        if (_fixture.IsAvailable)
            _fixture.ResetCollection();
        _repository = _fixture.IsAvailable ? _fixture.CreateRepository() : null!;
    }

    [SkippableFact]
    public async Task SaveAsync_DevePersistirEGerarObjectId()
    {
        _fixture.EnsureAvailable();

        var gadget = await _repository.SaveAsync(new Gadget { Name = "Async", Price = 1 });

        gadget.Id.Should().NotBe(ObjectId.Empty);
        (await _fixture.GadgetCollection.CountDocumentsAsync(g => g.Id == gadget.Id)).Should().Be(1);
    }

    [SkippableFact]
    public async Task GetAsync_PorChave_DeveRetornarEntidade()
    {
        _fixture.EnsureAvailable();
        var seeded = _fixture.Seed("BuscaAsync", 9);

        var found = await _repository.GetAsync(seeded.Id);

        found.Should().NotBeNull();
        found.Price.Should().Be(9);
    }

    [SkippableFact]
    public async Task GetAsync_PorChaveInexistente_DeveRetornarNull()
    {
        _fixture.EnsureAvailable();

        (await _repository.GetAsync(ObjectId.GenerateNewId())).Should().BeNull();
    }

    [SkippableFact]
    public async Task GetAsync_PorPredicado_DeveRetornarPrimeiro()
    {
        _fixture.EnsureAvailable();
        _fixture.Seed("PredAsync", 3);

        var found = await _repository.GetAsync(g => g.Name == "PredAsync");

        found.Should().NotBeNull();
        found.Price.Should().Be(3);
    }

    // BUG?: a versão síncrona Get(predicate) usa SingleOrDefault (lança com
    // múltiplos resultados), mas GetAsync(predicate) usa FirstOrDefaultAsync —
    // comportamentos divergentes entre sync e async para o mesmo contrato.
    // Registrado em tests/FINDINGS-D.md.
    [SkippableFact]
    public async Task GetAsync_PorPredicadoComMultiplosResultados_NaoLanca_DiferenteDoSync()
    {
        _fixture.EnsureAvailable();
        _fixture.Seed("Duplo", 1);
        _fixture.Seed("Duplo", 2);

        var found = await _repository.GetAsync(g => g.Name == "Duplo");

        found.Should().NotBeNull("comportamento atual: FirstOrDefault, sem validar unicidade");
    }

    [SkippableFact]
    public async Task LoadAsync_DeveDelegarParaGetAsync()
    {
        _fixture.EnsureAvailable();
        var seeded = _fixture.Seed("LoadAsync", 2);

        (await _repository.LoadAsync(seeded.Id))!.Name.Should().Be("LoadAsync");
        (await _repository.LoadAsync(ObjectId.GenerateNewId())).Should().BeNull();
    }

    [SkippableFact]
    public async Task FirstOrDefaultAsync_DeveRetornarPrimeiroOuNull()
    {
        _fixture.EnsureAvailable();
        _fixture.Seed("Primeiro", 1);

        (await _repository.FirstOrDefaultAsync(g => g.Name == "Primeiro")).Should().NotBeNull();
        (await _repository.FirstOrDefaultAsync(g => g.Name == "Inexistente")).Should().BeNull();
    }

    [SkippableFact]
    public async Task AnyAsync_DeveIndicarExistencia()
    {
        _fixture.EnsureAvailable();
        _fixture.Seed("Existe", 1);

        (await _repository.AnyAsync(g => g.Name == "Existe")).Should().BeTrue();
        (await _repository.AnyAsync(g => g.Name == "NaoExiste")).Should().BeFalse();
    }

    [SkippableFact]
    public async Task CountAsync_DeveContarPeloPredicado()
    {
        _fixture.EnsureAvailable();
        _fixture.Seed("A", 1);
        _fixture.Seed("B", 5);
        _fixture.Seed("C", 8);

        (await _repository.CountAsync(g => g.Price >= 5)).Should().Be(2);
    }

    [SkippableFact]
    public async Task ToListAsync_DeveMaterializarPeloPredicado()
    {
        _fixture.EnsureAvailable();
        _fixture.Seed("L1", 1);
        _fixture.Seed("L2", 2);
        _fixture.Seed("L3", 3);

        var list = await _repository.ToListAsync(g => g.Price > 1);

        list.Should().HaveCount(2);
        list.Should().OnlyContain(g => g.Price > 1);
    }

    [SkippableFact]
    public async Task UpdateAsync_DeveSubstituirODocumento()
    {
        _fixture.EnsureAvailable();
        var seeded = _fixture.Seed("UpAsync", 1);
        seeded.Price = 55;

        await _repository.UpdateAsync(seeded);

        (await _repository.GetAsync(seeded.Id)).Price.Should().Be(55);
    }

    [SkippableFact]
    public async Task SaveOrUpdateAsync_TransienteInsere_PersistidaSubstitui()
    {
        _fixture.EnsureAvailable();

        var inserted = await _repository.SaveOrUpdateAsync(new Gadget { Name = "SoU", Price = 1 });
        inserted.Id.Should().NotBe(ObjectId.Empty);

        inserted.Price = 22;
        await _repository.SaveOrUpdateAsync(inserted);

        (await _fixture.GadgetCollection.CountDocumentsAsync(g => g.Name == "SoU")).Should().Be(1);
        (await _repository.GetAsync(inserted.Id)).Price.Should().Be(22);
    }

    [SkippableFact]
    public async Task MergeAsync_DeveSubstituirERetornarEntidade()
    {
        _fixture.EnsureAvailable();
        var seeded = _fixture.Seed("MergeAsync", 1);
        seeded.Price = 33;

        var merged = await _repository.MergeAsync(seeded);

        merged.Should().BeSameAs(seeded);
        (await _repository.GetAsync(seeded.Id)).Price.Should().Be(33);
    }

    [SkippableFact]
    public async Task RefreshAsync_DeveRecarregarDoBanco()
    {
        _fixture.EnsureAvailable();
        var seeded = _fixture.Seed("RefAsync", 10);

        await _fixture.GadgetCollection.UpdateOneAsync(
            g => g.Id == seeded.Id,
            Builders<Gadget>.Update.Set(g => g.Price, 321));

        var refreshed = await _repository.RefreshAsync(seeded);

        refreshed.Should().NotBeSameAs(seeded);
        refreshed.Price.Should().Be(321);
    }

    [SkippableFact]
    public async Task DeleteAsync_DeveRemoverODocumento()
    {
        _fixture.EnsureAvailable();
        var seeded = _fixture.Seed("DelAsync", 1);

        await _repository.DeleteAsync(seeded);

        (await _fixture.GadgetCollection.CountDocumentsAsync(g => g.Id == seeded.Id)).Should().Be(0);
    }

    [SkippableFact]
    public async Task DeleteAsync_PorPredicado_DeveRemoverApenasCorrespondentes()
    {
        _fixture.EnsureAvailable();
        _fixture.Seed("DelPred", 1);
        _fixture.Seed("DelPred", 2);
        _fixture.Seed("Fica", 3);

        await _repository.DeleteAsync(g => g.Name == "DelPred");

        var remaining = _repository.All().ToList();
        remaining.Should().ContainSingle(g => g.Name == "Fica");
    }
}
