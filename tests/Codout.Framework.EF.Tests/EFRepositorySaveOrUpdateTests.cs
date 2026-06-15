using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Codout.Framework.EF.Tests;

/// <summary>
/// SaveOrUpdate / SaveOrUpdateAsync — a lógica delicada do fix 6.3.0:
/// insert vs. update decidido por Entry().State + inspeção da PK + Find no banco,
/// e NÃO por IEntity.IsTransient() (que falha para Id pré-atribuído no ctor).
/// </summary>
public class EFRepositorySaveOrUpdateTests : SqliteTestBase
{
    private Guid Seed(string name = "Ana", int age = 30)
    {
        using var context = CreateContext();
        var customer = new Customer { Name = name, Age = age };
        context.Customers.Add(customer);
        context.SaveChanges();
        return customer.Id!.Value;
    }

    // ---- Insert: entidade nova com Id pré-atribuído (o caso que o IsTransient errava) ----

    [Fact]
    public void Detached_new_entity_with_preassigned_id_is_inserted()
    {
        var customer = new Customer { Name = "Ana", Age = 30 }; // Id setado no ctor (ClientGeneratedEntity)
        customer.IsTransient().Should().BeFalse("o cenário-chave: Id pré-atribuído engana o IsTransient()");

        using (var context = CreateContext())
        {
            var repository = new EFRepository<Customer>(context);
            var result = repository.SaveOrUpdate(customer);

            result.Should().BeSameAs(customer);
            context.Entry(customer).State.Should().Be(EntityState.Added, "linha não existe ⇒ INSERT");
            context.SaveChanges();
        }

        using var verify = CreateContext();
        verify.Customers.Single().Name.Should().Be("Ana");
    }

    [Fact]
    public void Entity_with_default_key_is_added_without_database_roundtrip()
    {
        using var context = CreateContext();
        var repository = new EFRepository<Invoice>(context);

        var invoice = new Invoice { Number = "INV-1" }; // Id int = 0 (store-generated)
        repository.SaveOrUpdate(invoice);

        context.Entry(invoice).State.Should().Be(EntityState.Added);
        context.SaveChanges();
        invoice.Id.Should().BeGreaterThan(0, "autoincrement preenche a chave");
    }

    // ---- Update: entidade detached cuja linha já existe ----

    [Fact]
    public void Detached_entity_with_existing_row_updates_the_row()
    {
        var id = Seed("Ana", 30);

        var detached = new Customer { Name = "Ana Maria", Age = 31 };
        detached.SetId(id);

        using (var context = CreateContext())
        {
            var repository = new EFRepository<Customer>(context);
            var result = repository.SaveOrUpdate(detached);

            result.Should().BeSameAs(detached, "o contrato devolve a instância recebida");
            context.SaveChanges();
        }

        using var verify = CreateContext();
        var reloaded = verify.Customers.Single(c => c.Id == id);
        reloaded.Name.Should().Be("Ana Maria");
        reloaded.Age.Should().Be(31);
        verify.Customers.Count().Should().Be(1, "UPDATE, não um segundo INSERT");
    }

    [Fact]
    public void Detached_update_copies_values_into_the_tracked_instance_not_the_argument()
    {
        var id = Seed("Ana");

        using var context = CreateContext();
        var repository = new EFRepository<Customer>(context);

        var detached = new Customer { Name = "Nova" };
        detached.SetId(id);

        repository.SaveOrUpdate(detached);

        // Quem fica tracked é a instância carregada pelo Find (existing); a recebida
        // permanece Detached. Comportamento documentado do fix 6.3.0 (preserva
        // OriginalValues/tokens de concorrência do banco).
        context.Entry(detached).State.Should().Be(EntityState.Detached);

        var trackedEntry = context.ChangeTracker.Entries<Customer>().Single();
        trackedEntry.State.Should().Be(EntityState.Modified);
        trackedEntry.Entity.Name.Should().Be("Nova");
    }

    // ---- Entidade já rastreada ----

    [Fact]
    public void Tracked_unchanged_entity_is_not_forced_to_modified()
    {
        var id = Seed("Ana");

        using var context = CreateContext();
        var repository = new EFRepository<Customer>(context);

        var tracked = repository.Get(id);
        repository.SaveOrUpdate(tracked);

        context.Entry(tracked).State.Should().Be(EntityState.Unchanged,
            "mudança de comportamento 6.3.0: confia no change tracker, sem force full update");
    }

    [Fact]
    public void Tracked_entity_with_mutation_persists_via_change_tracker()
    {
        var id = Seed("Ana");

        using (var context = CreateContext())
        {
            var repository = new EFRepository<Customer>(context);
            var tracked = repository.Get(id);
            tracked.Name = "Mudada";

            repository.SaveOrUpdate(tracked);
            context.SaveChanges();
        }

        using var verify = CreateContext();
        verify.Customers.Single(c => c.Id == id).Name.Should().Be("Mudada");
    }

    [Fact]
    public void SaveOrUpdate_throws_on_null_entity()
    {
        using var context = CreateContext();
        var repository = new EFRepository<Customer>(context);

        FluentActions.Invoking(() => repository.SaveOrUpdate(null!))
            .Should().Throw<ArgumentNullException>();
    }

    // ---- Variantes assíncronas ----

    [Fact]
    public async Task SaveOrUpdateAsync_inserts_detached_new_entity_with_preassigned_id()
    {
        var customer = new Customer { Name = "Async", Age = 1 };

        using (var context = CreateContext())
        {
            var repository = new EFRepository<Customer>(context);
            await repository.SaveOrUpdateAsync(customer);

            context.Entry(customer).State.Should().Be(EntityState.Added);
            await context.SaveChangesAsync();
        }

        using var verify = CreateContext();
        (await verify.Customers.CountAsync()).Should().Be(1);
    }

    [Fact]
    public async Task SaveOrUpdateAsync_updates_existing_row_from_detached_entity()
    {
        var id = Seed("Ana", 30);

        var detached = new Customer { Name = "Async Update", Age = 99 };
        detached.SetId(id);

        using (var context = CreateContext())
        {
            var repository = new EFRepository<Customer>(context);
            await repository.SaveOrUpdateAsync(detached);
            await context.SaveChangesAsync();
        }

        using var verify = CreateContext();
        var reloaded = await verify.Customers.SingleAsync(c => c.Id == id);
        reloaded.Name.Should().Be("Async Update");
        reloaded.Age.Should().Be(99);
    }

    [Fact]
    public async Task SaveOrUpdateAsync_honors_a_cancelled_token_when_a_lookup_is_needed()
    {
        var id = Seed("Ana");

        var detached = new Customer { Name = "X" };
        detached.SetId(id);

        using var context = CreateContext();
        var repository = new EFRepository<Customer>(context);
        var cancelled = new CancellationToken(canceled: true);

        var act = () => repository.SaveOrUpdateAsync(detached, cancelled);

        await act.Should().ThrowAsync<OperationCanceledException>(
            "fix 6.3.0: o FindAsync respeita o CancellationToken recebido");
    }

    [Fact]
    public async Task SaveOrUpdateAsync_throws_on_null_entity()
    {
        using var context = CreateContext();
        var repository = new EFRepository<Customer>(context);

        await FluentActions.Awaiting(() => repository.SaveOrUpdateAsync(null!))
            .Should().ThrowAsync<ArgumentNullException>();
    }
}
