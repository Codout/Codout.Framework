using FluentAssertions;
using MongoDB.Driver;
using Xunit;

namespace Codout.Framework.Mongo.Tests;

/// <summary>
/// Testes de unidade do MongoUnitOfWork que não precisam de servidor
/// (construção, estados sem transação e dispose).
/// </summary>
public class MongoUnitOfWorkUnitTests
{
    private static MongoUnitOfWork CreateOfflineUnitOfWork() =>
        new(new MongoClient("mongodb://localhost:27017/?serverSelectionTimeoutMS=100"));

    [Fact]
    public void Construtor_ComClientNulo_DeveLancarArgumentNullException()
    {
        var act = () => new MongoUnitOfWork(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void CurrentSession_SemTransacao_DeveSerNull()
    {
        using var uow = CreateOfflineUnitOfWork();

        uow.CurrentSession.Should().BeNull();
    }

    [Fact]
    public void Commit_SemTransacaoAtiva_DeveLancarInvalidOperationException()
    {
        using var uow = CreateOfflineUnitOfWork();

        var act = () => uow.Commit();
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public async Task CommitAsync_SemTransacaoAtiva_DeveLancarInvalidOperationException()
    {
        await using var uow = CreateOfflineUnitOfWork();

        var act = () => uow.CommitAsync();
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public void Rollback_SemTransacaoAtiva_DeveSerNoOp()
    {
        using var uow = CreateOfflineUnitOfWork();

        var act = () => uow.Rollback();
        act.Should().NotThrow();
    }

    [Fact]
    public async Task RollbackAsync_SemTransacaoAtiva_DeveSerNoOp()
    {
        await using var uow = CreateOfflineUnitOfWork();

        var act = () => uow.RollbackAsync();
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public void InTransaction_ComWorkNulo_DeveLancarArgumentNullException()
    {
        using var uow = CreateOfflineUnitOfWork();

        var act = () => uow.InTransaction<Gadget>(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public async Task InTransactionAsync_ComWorkNulo_DeveLancarArgumentNullException()
    {
        await using var uow = CreateOfflineUnitOfWork();

        var act = () => uow.InTransactionAsync<Gadget>(null!);
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public void Dispose_DeveSerIdempotente()
    {
        var uow = CreateOfflineUnitOfWork();

        var act = () =>
        {
            uow.Dispose();
            uow.Dispose();
        };

        act.Should().NotThrow();
    }

    [Fact]
    public async Task DisposeAsync_DeveSerSeguroSemTransacao()
    {
        var uow = CreateOfflineUnitOfWork();

        var act = () => uow.DisposeAsync().AsTask();
        await act.Should().NotThrowAsync();
    }
}
