using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Codout.Framework.EF.Tests;

/// <summary>
/// Superfície assíncrona do EFRepository: queries, comandos e overloads de
/// CancellationToken (incluindo tokens já cancelados).
/// </summary>
public class EFRepositoryAsyncTests : SqliteTestBase
{
    private static readonly CancellationToken Cancelled = new(canceled: true);

    private Guid Seed(params Customer[] customers)
    {
        using var context = CreateContext();
        context.Customers.AddRange(customers);
        context.SaveChanges();
        return customers.First().Id!.Value;
    }

    [Fact]
    public async Task GetAsync_by_predicate_and_by_key_round_trip()
    {
        var id = Seed(new Customer { Name = "Ana", Age = 30 });

        using var context = CreateContext();
        var repository = new EFRepository<Customer>(context);

        (await repository.GetAsync(c => c.Name == "Ana")).Id.Should().Be(id);
        (await repository.GetAsync((object)id)).Name.Should().Be("Ana");
        (await repository.LoadAsync(id)).Name.Should().Be("Ana");
        (await repository.GetAsync(c => c.Name == "Zoe")).Should().BeNull();
    }

    [Fact]
    public async Task FirstOrDefault_Any_Count_ToList_work()
    {
        Seed(
            new Customer { Name = "Ana", Age = 18 },
            new Customer { Name = "Bia", Age = 40 },
            new Customer { Name = "Caio", Age = 65 });

        using var context = CreateContext();
        var repository = new EFRepository<Customer>(context);

        (await repository.FirstOrDefaultAsync(c => c.Age > 100)).Should().BeNull();
        (await repository.AnyAsync(c => c.Age >= 40)).Should().BeTrue();
        (await repository.AnyAsync(c => c.Age > 100)).Should().BeFalse();
        (await repository.CountAsync(c => c.Age >= 40)).Should().Be(2);
        (await repository.ToListAsync(c => c.Age >= 40)).Should().HaveCount(2);
    }

    [Fact]
    public async Task SaveAsync_and_DeleteAsync_round_trip()
    {
        using (var context = CreateContext())
        {
            var repository = new EFRepository<Customer>(context);
            await repository.SaveAsync(new Customer { Name = "Ana" });
            await repository.SaveAsync(new Customer { Name = "Bia" }, CancellationToken.None);
            await context.SaveChangesAsync();
        }

        using (var context = CreateContext())
        {
            var repository = new EFRepository<Customer>(context);
            var ana = await repository.GetAsync(c => c.Name == "Ana");
            await repository.DeleteAsync(ana);
            await context.SaveChangesAsync();
        }

        using (var context = CreateContext())
        {
            var repository = new EFRepository<Customer>(context);
            await repository.DeleteAsync(c => c.Name == "Bia");
            await context.SaveChangesAsync();
        }

        using var verify = CreateContext();
        (await verify.Customers.CountAsync()).Should().Be(0);
    }

    [Fact]
    public async Task UpdateAsync_persists_changes_of_detached_entity()
    {
        var id = Seed(new Customer { Name = "Ana", Age = 30 });

        using (var context = CreateContext())
        {
            var repository = new EFRepository<Customer>(context);
            var detached = new Customer { Name = "Atualizada", Age = 31 };
            detached.SetId(id);

            await repository.UpdateAsync(detached);
            await context.SaveChangesAsync();
        }

        using var verify = CreateContext();
        (await verify.Customers.SingleAsync(c => c.Id == id)).Name.Should().Be("Atualizada");
    }

    [Fact]
    public async Task MergeAsync_attaches_detached_entity()
    {
        var id = Seed(new Customer { Name = "Ana", Age = 30 });

        using var context = CreateContext();
        var repository = new EFRepository<Customer>(context);

        var detached = new Customer { Name = "Ana", Age = 30 };
        detached.SetId(id);

        var merged = await repository.MergeAsync(detached);

        merged.Should().BeSameAs(detached);
        context.Entry(detached).State.Should().Be(EntityState.Unchanged);
    }

    [Fact]
    public async Task RefreshAsync_restores_database_values()
    {
        var id = Seed(new Customer { Name = "Ana", Age = 30 });

        using var context = CreateContext();
        var repository = new EFRepository<Customer>(context);

        var entity = await repository.GetAsync((object)id);
        entity.Name = "Local";

        var refreshed = await repository.RefreshAsync(entity);

        refreshed.Name.Should().Be("Ana");
    }

    [Fact]
    public async Task Cancelled_token_aborts_async_queries()
    {
        Seed(new Customer { Name = "Ana" });

        using var context = CreateContext();
        var repository = new EFRepository<Customer>(context);

        await FluentActions.Awaiting(() => repository.GetAsync(c => c.Name == "Ana", Cancelled))
            .Should().ThrowAsync<OperationCanceledException>();
        await FluentActions.Awaiting(() => repository.CountAsync(c => true, Cancelled))
            .Should().ThrowAsync<OperationCanceledException>();
        await FluentActions.Awaiting(() => repository.ToListAsync(c => true, Cancelled))
            .Should().ThrowAsync<OperationCanceledException>();
        await FluentActions.Awaiting(() => repository.DeleteAsync(c => true, Cancelled))
            .Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task Cancelled_token_aborts_GetAsync_by_key_for_untracked_entity()
    {
        var id = Seed(new Customer { Name = "Ana" });

        using var context = CreateContext();
        var repository = new EFRepository<Customer>(context);

        await FluentActions.Awaiting(() => repository.GetAsync((object)id, Cancelled))
            .Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task RefreshAsync_with_token_currently_ignores_cancellation()
    {
        // BUG?: RefreshAsync(entity, cancellationToken) delega para o Refresh SÍNCRONO
        // (Task.FromResult(Refresh(entity))): bloqueia a thread e IGNORA o token.
        // Characterization test do comportamento atual — ver tests/FINDINGS-B.md.
        var id = Seed(new Customer { Name = "Ana", Age = 30 });

        using var context = CreateContext();
        var repository = new EFRepository<Customer>(context);

        var entity = await repository.GetAsync((object)id);
        entity.Name = "Local";

        var refreshed = await repository.RefreshAsync(entity, Cancelled);

        refreshed.Name.Should().Be("Ana", "o reload acontece mesmo com token cancelado");
    }
}
