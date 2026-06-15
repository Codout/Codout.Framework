using System.Data;
using FluentAssertions;
using Xunit;

namespace Codout.Framework.NH.Tests;

[Collection("NH")]
public class NHUnitOfWorkTests(NHSqliteFixture fixture)
{
    private readonly NHSqliteFixture _fixture = Reset(fixture);

    private static NHSqliteFixture Reset(NHSqliteFixture fixture)
    {
        fixture.ResetDatabase();
        return fixture;
    }

    private NHUnitOfWork CreateUnitOfWork() => new(_fixture.OpenSession());

    private int CountWidgets()
    {
        using var session = _fixture.OpenSession();
        return session.Query<Widget>().Count();
    }

    [Fact]
    public void Commit_DevePersistirAlteracoesDaTransacao()
    {
        using (var uow = new NHUnitOfWork(_fixture.OpenSession()))
        {
            var repository = new NHRepository<Widget>(uow.Session);
            uow.BeginTransaction();
            repository.Save(new Widget { Name = "Comitado", Stock = 1 });
            uow.Commit();
        }

        CountWidgets().Should().Be(1);
    }

    [Fact]
    public void Rollback_DeveDescartarAlteracoesDaTransacao()
    {
        using (var uow = CreateUnitOfWork())
        {
            var repository = new NHRepository<Widget>(uow.Session);
            uow.BeginTransaction();
            repository.Save(new Widget { Name = "Descartado", Stock = 1 });
            uow.Rollback();
        }

        CountWidgets().Should().Be(0);
    }

    [Fact]
    public void Rollback_SemTransacaoAtiva_DeveSerNoOp()
    {
        using var uow = CreateUnitOfWork();

        var act = () => uow.Rollback();
        act.Should().NotThrow();
    }

    [Fact]
    public void Commit_SemTransacaoAtiva_DeveLancarInvalidOperationException()
    {
        using var uow = CreateUnitOfWork();

        var act = () => uow.Commit();
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void BeginTransaction_DuasVezes_DeveLancarInvalidOperationException()
    {
        using var uow = CreateUnitOfWork();
        uow.BeginTransaction();

        var act = () => uow.BeginTransaction();
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void BeginTransaction_AposCommit_DevePermitirNovaTransacao()
    {
        using var uow = CreateUnitOfWork();
        var repository = new NHRepository<Widget>(uow.Session);

        uow.BeginTransaction();
        repository.Save(new Widget { Name = "Primeira", Stock = 1 });
        uow.Commit();

        uow.BeginTransaction();
        repository.Save(new Widget { Name = "Segunda", Stock = 2 });
        uow.Commit();

        CountWidgets().Should().Be(2);
    }

    [Fact]
    public void Commit_ComIsolationLevel_DeveDelegarParaCommit()
    {
        using var uow = CreateUnitOfWork();
        var repository = new NHRepository<Widget>(uow.Session);

        uow.BeginTransaction(IsolationLevel.Serializable);
        repository.Save(new Widget { Name = "Isolado", Stock = 1 });
        uow.Commit(IsolationLevel.Serializable);

        CountWidgets().Should().Be(1);
    }

    [Fact]
    public void InTransaction_DeveComitarERetornarResultado()
    {
        using var uow = CreateUnitOfWork();
        var repository = new NHRepository<Widget>(uow.Session);

        var result = uow.InTransaction(() => repository.Save(new Widget { Name = "InTx", Stock = 1 }));

        result.Id.Should().BePositive();
        CountWidgets().Should().Be(1);
    }

    [Fact]
    public void InTransaction_ComExcecao_DeveReverterERelancar()
    {
        using var uow = CreateUnitOfWork();
        var repository = new NHRepository<Widget>(uow.Session);

        var act = () => uow.InTransaction<Widget>(() =>
        {
            repository.Save(new Widget { Name = "Falha", Stock = 1 });
            throw new InvalidDataException("boom");
        });

        act.Should().Throw<InvalidDataException>();
        CountWidgets().Should().Be(0);
    }

    [Fact]
    public void InTransaction_ComWorkNulo_DeveLancarArgumentNullException()
    {
        using var uow = CreateUnitOfWork();

        var act = () => uow.InTransaction<Widget>(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Dispose_ComTransacaoPendente_DeveReverterEFecharASessao()
    {
        var uow = CreateUnitOfWork();
        var session = uow.Session;
        var repository = new NHRepository<Widget>(session);

        uow.BeginTransaction();
        repository.Save(new Widget { Name = "Pendente", Stock = 1 });
        uow.Dispose();

        session.IsOpen.Should().BeFalse("o UnitOfWork é dono da sessão");
        CountWidgets().Should().Be(0, "alterações não comitadas são revertidas no Dispose");
    }

    [Fact]
    public async Task CommitAsync_DevePersistirAlteracoesDaTransacao()
    {
        await using (var uow = CreateUnitOfWork())
        {
            var repository = new NHRepository<Widget>(uow.Session);
            await uow.BeginTransactionAsync();
            await repository.SaveAsync(new Widget { Name = "ComitadoAsync", Stock = 1 });
            await uow.CommitAsync();
        }

        CountWidgets().Should().Be(1);
    }

    [Fact]
    public async Task RollbackAsync_DeveDescartarAlteracoesDaTransacao()
    {
        await using (var uow = CreateUnitOfWork())
        {
            var repository = new NHRepository<Widget>(uow.Session);
            await uow.BeginTransactionAsync();
            await repository.SaveAsync(new Widget { Name = "DescartadoAsync", Stock = 1 });
            await uow.RollbackAsync();
        }

        CountWidgets().Should().Be(0);
    }

    [Fact]
    public async Task CommitAsync_SemTransacaoAtiva_DeveLancarInvalidOperationException()
    {
        await using var uow = CreateUnitOfWork();

        var act = () => uow.CommitAsync();
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task RollbackAsync_SemTransacaoAtiva_DeveSerNoOp()
    {
        await using var uow = CreateUnitOfWork();

        var act = () => uow.RollbackAsync();
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task BeginTransactionAsync_DuasVezes_DeveLancarInvalidOperationException()
    {
        await using var uow = CreateUnitOfWork();
        await uow.BeginTransactionAsync();

        var act = () => uow.BeginTransactionAsync();
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task InTransactionAsync_DeveComitarERetornarResultado()
    {
        await using var uow = CreateUnitOfWork();
        var repository = new NHRepository<Widget>(uow.Session);

        var result = await uow.InTransactionAsync(() =>
            repository.SaveAsync(new Widget { Name = "InTxAsync", Stock = 1 }));

        result.Id.Should().BePositive();
        CountWidgets().Should().Be(1);
    }

    [Fact]
    public async Task InTransactionAsync_ComExcecao_DeveReverterERelancar()
    {
        await using var uow = CreateUnitOfWork();
        var repository = new NHRepository<Widget>(uow.Session);

        var act = () => uow.InTransactionAsync<Widget>(async () =>
        {
            await repository.SaveAsync(new Widget { Name = "FalhaAsync", Stock = 1 });
            throw new InvalidDataException("boom");
        });

        await act.Should().ThrowAsync<InvalidDataException>();
        CountWidgets().Should().Be(0);
    }

    [Fact]
    public async Task DisposeAsync_ComTransacaoPendente_DeveReverter()
    {
        var uow = CreateUnitOfWork();
        var repository = new NHRepository<Widget>(uow.Session);

        await uow.BeginTransactionAsync();
        await repository.SaveAsync(new Widget { Name = "PendenteAsync", Stock = 1 });
        await uow.DisposeAsync();

        CountWidgets().Should().Be(0);
    }
}
