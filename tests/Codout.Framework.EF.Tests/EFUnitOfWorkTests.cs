using System.Data;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Codout.Framework.EF.Tests;

/// <summary>
/// EFUnitOfWork: commit/rollback (sync e async), transações explícitas,
/// InTransaction/InTransactionAsync e o ciclo de vida do DbContext.
/// </summary>
public class EFUnitOfWorkTests : SqliteTestBase
{
    private sealed class TestUnitOfWork(TestDbContext context) : EFUnitOfWork<TestDbContext>(context);

    private TestUnitOfWork CreateUnitOfWork() => new(CreateContext());

    private int CountCustomers()
    {
        using var context = CreateContext();
        return context.Customers.Count();
    }

    [Fact]
    public void Commit_without_transaction_just_saves_changes()
    {
        using (var uow = CreateUnitOfWork())
        {
            uow.DbContext.Add(new Customer { Name = "Ana" });
            uow.Commit();
        }

        CountCustomers().Should().Be(1);
    }

    [Fact]
    public void Commit_inside_transaction_persists()
    {
        using (var uow = CreateUnitOfWork())
        {
            uow.BeginTransaction();
            uow.DbContext.Add(new Customer { Name = "Ana" });
            uow.Commit();
        }

        CountCustomers().Should().Be(1);
    }

    [Fact]
    public void Rollback_discards_pending_transaction_work()
    {
        using (var uow = CreateUnitOfWork())
        {
            uow.BeginTransaction();
            uow.DbContext.Add(new Customer { Name = "Ana" });
            uow.DbContext.SaveChanges();
            uow.Rollback();
        }

        CountCustomers().Should().Be(0);
    }

    [Fact]
    public void Rollback_without_transaction_is_a_noop()
    {
        using var uow = CreateUnitOfWork();
        var act = uow.Rollback;
        act.Should().NotThrow();
    }

    [Fact]
    public void BeginTransaction_twice_throws()
    {
        using var uow = CreateUnitOfWork();
        uow.BeginTransaction();

        var act = () => uow.BeginTransaction();

        act.Should().Throw<InvalidOperationException>("transações não são aninháveis");
        uow.Rollback();
    }

    [Fact]
    public void Transaction_can_be_restarted_after_commit()
    {
        using var uow = CreateUnitOfWork();

        uow.BeginTransaction();
        uow.DbContext.Add(new Customer { Name = "Ana" });
        uow.Commit();

        var act = () => uow.BeginTransaction();
        act.Should().NotThrow("o commit limpa a transação corrente");
        uow.Rollback();
    }

    [Fact]
    public async Task CommitAsync_persists_within_async_transaction()
    {
        await using (var uow = CreateUnitOfWork())
        {
            await uow.BeginTransactionAsync();
            uow.DbContext.Add(new Customer { Name = "Ana" });
            await uow.CommitAsync();
        }

        CountCustomers().Should().Be(1);
    }

    [Fact]
    public async Task CommitAsync_without_transaction_throws_unlike_sync_Commit()
    {
        // BUG?: assimetria de contrato — Commit() sem transação faz SaveChanges
        // (auto-commit), mas CommitAsync() sem transação lança InvalidOperationException.
        // Characterization test do comportamento atual — ver tests/FINDINGS-B.md.
        await using var uow = CreateUnitOfWork();
        uow.DbContext.Add(new Customer { Name = "Ana" });

        var act = () => uow.CommitAsync();

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task RollbackAsync_discards_pending_transaction_work()
    {
        await using (var uow = CreateUnitOfWork())
        {
            await uow.BeginTransactionAsync();
            uow.DbContext.Add(new Customer { Name = "Ana" });
            await uow.DbContext.SaveChangesAsync();
            await uow.RollbackAsync();
        }

        CountCustomers().Should().Be(0);
    }

    [Fact]
    public async Task RollbackAsync_without_transaction_is_a_noop()
    {
        await using var uow = CreateUnitOfWork();
        var act = () => uow.RollbackAsync();
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public void InTransaction_commits_and_returns_the_result()
    {
        Customer result;
        using (var uow = CreateUnitOfWork())
        {
            result = uow.InTransaction(() =>
            {
                var customer = new Customer { Name = "Ana" };
                uow.DbContext.Add(customer);
                return customer;
            });
        }

        result.Name.Should().Be("Ana");
        CountCustomers().Should().Be(1);
    }

    [Fact]
    public void InTransaction_rolls_back_when_work_throws()
    {
        using (var uow = CreateUnitOfWork())
        {
            var act = () => uow.InTransaction<Customer>(() =>
            {
                uow.DbContext.Add(new Customer { Name = "Ana" });
                uow.DbContext.SaveChanges();
                throw new InvalidOperationException("boom");
            });

            act.Should().Throw<InvalidOperationException>().WithMessage("boom");
        }

        CountCustomers().Should().Be(0, "a exceção dispara rollback");
    }

    [Fact]
    public void InTransaction_throws_on_null_work()
    {
        using var uow = CreateUnitOfWork();
        var act = () => uow.InTransaction<Customer>(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public async Task InTransactionAsync_commits_and_returns_the_result()
    {
        await using (var uow = CreateUnitOfWork())
        {
            var result = await uow.InTransactionAsync(() =>
            {
                var customer = new Customer { Name = "Ana" };
                uow.DbContext.Add(customer);
                return Task.FromResult(customer);
            });

            result.Name.Should().Be("Ana");
        }

        CountCustomers().Should().Be(1);
    }

    [Fact]
    public async Task InTransactionAsync_rolls_back_when_work_throws()
    {
        await using (var uow = CreateUnitOfWork())
        {
            var act = () => uow.InTransactionAsync<Customer>(async () =>
            {
                uow.DbContext.Add(new Customer { Name = "Ana" });
                await uow.DbContext.SaveChangesAsync();
                throw new InvalidOperationException("boom");
            });

            await act.Should().ThrowAsync<InvalidOperationException>();
        }

        CountCustomers().Should().Be(0);
    }

    [Fact]
    public async Task InTransactionAsync_reuses_an_externally_managed_transaction()
    {
        await using var uow = CreateUnitOfWork();
        await uow.BeginTransactionAsync();

        await uow.InTransactionAsync(() =>
        {
            uow.DbContext.Add(new Customer { Name = "Ana" });
            return Task.FromResult(new Customer());
        });

        CountCustomers().Should().Be(0, "a transação externa ainda não foi commitada");

        await uow.CommitAsync();
        CountCustomers().Should().Be(1);
    }

    [Fact]
    public void Dispose_disposes_the_underlying_context()
    {
        var uow = CreateUnitOfWork();
        var context = uow.DbContext;

        uow.Dispose();

        var act = () => context.Set<Customer>().Count();
        act.Should().Throw<ObjectDisposedException>("o ciclo de vida do DbContext pertence ao UnitOfWork");
    }

    [Fact]
    public void Constructor_throws_on_null_context()
    {
        var act = () => new TestUnitOfWork(null!);
        act.Should().Throw<ArgumentNullException>();
    }
}
