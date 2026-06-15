using FluentAssertions;
using MongoDB.Driver;
using Xunit;

namespace Codout.Framework.Mongo.Tests;

/// <summary>
/// Testes de integração do MongoUnitOfWork — exigem replica set (a fixture
/// sobe o mongod efêmero com UseSingleNodeReplicaSet).
/// </summary>
[Collection("Mongo")]
public class MongoUnitOfWorkTests
{
    private readonly MongoFixture _fixture;

    public MongoUnitOfWorkTests(MongoFixture fixture)
    {
        _fixture = fixture;
        if (_fixture.IsAvailable)
            _fixture.ResetCollection();
    }

    private MongoUnitOfWork CreateUnitOfWork() => new(_fixture.Client!);

    [SkippableFact]
    public void BeginTransaction_DeveAbrirSessaoComTransacaoAtiva()
    {
        _fixture.EnsureAvailable();
        using var uow = CreateUnitOfWork();

        uow.CurrentSession.Should().BeNull();

        uow.BeginTransaction();

        uow.CurrentSession.Should().NotBeNull();
        uow.CurrentSession!.IsInTransaction.Should().BeTrue();
    }

    [SkippableFact]
    public void BeginTransaction_DuasVezes_DeveLancarInvalidOperationException()
    {
        _fixture.EnsureAvailable();
        using var uow = CreateUnitOfWork();
        uow.BeginTransaction();

        var act = () => uow.BeginTransaction();
        act.Should().Throw<InvalidOperationException>();
    }

    [SkippableFact]
    public void Commit_DeveEncerrarASessao()
    {
        _fixture.EnsureAvailable();
        using var uow = CreateUnitOfWork();
        uow.BeginTransaction();

        // Operação dentro da sessão da transação.
        _fixture.GadgetCollection.InsertOne(uow.CurrentSession, new Gadget { Name = "NaSessao", Price = 1 });

        uow.Commit();

        uow.CurrentSession.Should().BeNull();
        _fixture.GadgetCollection.CountDocuments(g => g.Name == "NaSessao").Should().Be(1);
    }

    [SkippableFact]
    public void Rollback_DeveDescartarOperacoesFeitasNaSessao()
    {
        _fixture.EnsureAvailable();
        using var uow = CreateUnitOfWork();
        uow.BeginTransaction();

        _fixture.GadgetCollection.InsertOne(uow.CurrentSession, new Gadget { Name = "Descartado", Price = 1 });

        uow.Rollback();

        uow.CurrentSession.Should().BeNull();
        _fixture.GadgetCollection.CountDocuments(g => g.Name == "Descartado").Should().Be(0);
    }

    // BUG?: o MongoRepository NUNCA usa a sessão do MongoUnitOfWork
    // (CurrentSession) — todas as operações do repositório executam FORA da
    // transação. Rollback no UnitOfWork NÃO desfaz um Save feito pelo
    // repositório. Registrado em tests/FINDINGS-D.md.
    [SkippableFact]
    public void Rollback_Caracterizacao_NaoDesfazOperacoesDoRepositorio()
    {
        _fixture.EnsureAvailable();
        using var uow = CreateUnitOfWork();
        var repository = _fixture.CreateRepository();

        uow.BeginTransaction();
        repository.Save(new Gadget { Name = "ForaDaTransacao", Price = 1 });
        uow.Rollback();

        // Comportamento atual: o documento sobrevive ao rollback, pois o
        // repositório não participa da sessão transacional.
        _fixture.GadgetCollection.CountDocuments(g => g.Name == "ForaDaTransacao")
            .Should().Be(1, "o repositório não usa a sessão do UnitOfWork");
    }

    [SkippableFact]
    public void Commit_SemTransacaoAtiva_DeveLancarInvalidOperationException()
    {
        _fixture.EnsureAvailable();
        using var uow = CreateUnitOfWork();

        var act = () => uow.Commit();
        act.Should().Throw<InvalidOperationException>();
    }

    [SkippableFact]
    public void Rollback_SemTransacaoAtiva_DeveSerNoOp()
    {
        _fixture.EnsureAvailable();
        using var uow = CreateUnitOfWork();

        var act = () => uow.Rollback();
        act.Should().NotThrow();
    }

    [SkippableFact]
    public void InTransaction_DeveComitarERetornarResultado()
    {
        _fixture.EnsureAvailable();
        using var uow = CreateUnitOfWork();

        var result = uow.InTransaction(() =>
        {
            var gadget = new Gadget { Name = "InTx", Price = 1 };
            _fixture.GadgetCollection.InsertOne(uow.CurrentSession, gadget);
            return gadget;
        });

        result.Should().NotBeNull();
        uow.CurrentSession.Should().BeNull();
        _fixture.GadgetCollection.CountDocuments(g => g.Name == "InTx").Should().Be(1);
    }

    [SkippableFact]
    public void InTransaction_ComExcecao_DeveReverterERelancar()
    {
        _fixture.EnsureAvailable();
        using var uow = CreateUnitOfWork();

        var act = () => uow.InTransaction<Gadget>(() =>
        {
            _fixture.GadgetCollection.InsertOne(uow.CurrentSession, new Gadget { Name = "Falha", Price = 1 });
            throw new InvalidDataException("boom");
        });

        act.Should().Throw<InvalidDataException>();
        _fixture.GadgetCollection.CountDocuments(g => g.Name == "Falha").Should().Be(0);
    }

    [SkippableFact]
    public async Task CommitAsync_DeveEncerrarASessao()
    {
        _fixture.EnsureAvailable();
        await using var uow = CreateUnitOfWork();
        await uow.BeginTransactionAsync();

        await _fixture.GadgetCollection.InsertOneAsync(
            uow.CurrentSession, new Gadget { Name = "AsyncCommit", Price = 1 });

        await uow.CommitAsync();

        uow.CurrentSession.Should().BeNull();
        (await _fixture.GadgetCollection.CountDocumentsAsync(g => g.Name == "AsyncCommit")).Should().Be(1);
    }

    [SkippableFact]
    public async Task RollbackAsync_DeveDescartarOperacoesFeitasNaSessao()
    {
        _fixture.EnsureAvailable();
        await using var uow = CreateUnitOfWork();
        await uow.BeginTransactionAsync();

        await _fixture.GadgetCollection.InsertOneAsync(
            uow.CurrentSession, new Gadget { Name = "AsyncRollback", Price = 1 });

        await uow.RollbackAsync();

        (await _fixture.GadgetCollection.CountDocumentsAsync(g => g.Name == "AsyncRollback")).Should().Be(0);
    }

    [SkippableFact]
    public async Task CommitAsync_SemTransacaoAtiva_DeveLancarInvalidOperationException()
    {
        _fixture.EnsureAvailable();
        await using var uow = CreateUnitOfWork();

        var act = () => uow.CommitAsync();
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [SkippableFact]
    public async Task BeginTransactionAsync_DuasVezes_DeveLancarInvalidOperationException()
    {
        _fixture.EnsureAvailable();
        await using var uow = CreateUnitOfWork();
        await uow.BeginTransactionAsync();

        var act = () => uow.BeginTransactionAsync();
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [SkippableFact]
    public async Task InTransactionAsync_DeveComitarERetornarResultado()
    {
        _fixture.EnsureAvailable();
        await using var uow = CreateUnitOfWork();

        var result = await uow.InTransactionAsync(async () =>
        {
            var gadget = new Gadget { Name = "InTxAsync", Price = 1 };
            await _fixture.GadgetCollection.InsertOneAsync(uow.CurrentSession, gadget);
            return gadget;
        });

        result.Should().NotBeNull();
        (await _fixture.GadgetCollection.CountDocumentsAsync(g => g.Name == "InTxAsync")).Should().Be(1);
    }

    [SkippableFact]
    public async Task InTransactionAsync_ComExcecao_DeveReverterERelancar()
    {
        _fixture.EnsureAvailable();
        await using var uow = CreateUnitOfWork();

        var act = () => uow.InTransactionAsync<Gadget>(async () =>
        {
            await _fixture.GadgetCollection.InsertOneAsync(
                uow.CurrentSession, new Gadget { Name = "FalhaAsync", Price = 1 });
            throw new InvalidDataException("boom");
        });

        await act.Should().ThrowAsync<InvalidDataException>();
        (await _fixture.GadgetCollection.CountDocumentsAsync(g => g.Name == "FalhaAsync")).Should().Be(0);
    }

    [SkippableFact]
    public void Dispose_ComTransacaoPendente_DeveDescartarASessao()
    {
        _fixture.EnsureAvailable();
        var uow = CreateUnitOfWork();
        uow.BeginTransaction();

        _fixture.GadgetCollection.InsertOne(uow.CurrentSession, new Gadget { Name = "Pendente", Price = 1 });

        uow.Dispose();

        // A sessão é descartada sem commit — a transação morre com ela.
        _fixture.GadgetCollection.CountDocuments(g => g.Name == "Pendente").Should().Be(0);
    }
}
