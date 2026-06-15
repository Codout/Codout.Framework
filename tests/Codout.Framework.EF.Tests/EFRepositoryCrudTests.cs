using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Codout.Framework.EF.Tests;

/// <summary>
/// CRUD básico do EFRepository sobre SQLite in-memory: queries (All/AllReadOnly/Where/
/// WhereReadOnly/WherePaged/Get/Load), comandos (Save/Delete/Update/Merge/Refresh)
/// e Includes.
/// </summary>
public class EFRepositoryCrudTests : SqliteTestBase
{
    private static Customer NewCustomer(string name, int age = 20) => new() { Name = name, Age = age };

    private Guid SeedCustomers(params Customer[] customers)
    {
        using var context = CreateContext();
        context.Customers.AddRange(customers);
        context.SaveChanges();
        return customers.First().Id!.Value;
    }

    [Fact]
    public void Save_then_Get_by_key_round_trips()
    {
        Guid id;
        using (var context = new EFRepositoryScope(CreateContext()))
        {
            var saved = context.Repository.Save(NewCustomer("Ana", 30));
            context.Context.SaveChanges();
            id = saved.Id!.Value;
        }

        using var verify = new EFRepositoryScope(CreateContext());
        var loaded = verify.Repository.Get(id);

        loaded.Should().NotBeNull();
        loaded.Name.Should().Be("Ana");
        loaded.Age.Should().Be(30);
    }

    [Fact]
    public void Save_throws_on_null_entity()
    {
        using var scope = new EFRepositoryScope(CreateContext());

        var act = () => scope.Repository.Save(null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Get_by_predicate_returns_single_match()
    {
        SeedCustomers(NewCustomer("Ana"), NewCustomer("Bia"));

        using var scope = new EFRepositoryScope(CreateContext());
        scope.Repository.Get(c => c.Name == "Bia").Name.Should().Be("Bia");
        scope.Repository.Get(c => c.Name == "Zoe").Should().BeNull();
    }

    [Fact]
    public void Get_by_predicate_throws_when_multiple_match()
    {
        SeedCustomers(NewCustomer("Ana", 30), NewCustomer("Bia", 30));

        using var scope = new EFRepositoryScope(CreateContext());
        var act = () => scope.Repository.Get(c => c.Age == 30);

        act.Should().Throw<InvalidOperationException>("Get usa SingleOrDefault");
    }

    [Fact]
    public void Load_delegates_to_Get_by_key()
    {
        var id = SeedCustomers(NewCustomer("Ana"));

        using var scope = new EFRepositoryScope(CreateContext());
        scope.Repository.Load(id).Name.Should().Be("Ana");
    }

    [Fact]
    public void All_returns_every_entity()
    {
        SeedCustomers(NewCustomer("Ana"), NewCustomer("Bia"), NewCustomer("Caio"));

        using var scope = new EFRepositoryScope(CreateContext());
        scope.Repository.All().Count().Should().Be(3);
    }

    [Fact]
    public void All_tracks_entities_but_AllReadOnly_does_not()
    {
        SeedCustomers(NewCustomer("Ana"));

        using (var tracked = new EFRepositoryScope(CreateContext()))
        {
            _ = tracked.Repository.All().ToList();
            tracked.Context.ChangeTracker.Entries().Should().NotBeEmpty();
        }

        using var untracked = new EFRepositoryScope(CreateContext());
        _ = untracked.Repository.AllReadOnly().ToList();
        untracked.Context.ChangeTracker.Entries().Should().BeEmpty("AsNoTracking não rastreia");
    }

    [Fact]
    public void Where_filters_and_WhereReadOnly_does_not_track()
    {
        SeedCustomers(NewCustomer("Ana", 18), NewCustomer("Bia", 40), NewCustomer("Caio", 65));

        using var scope = new EFRepositoryScope(CreateContext());
        var tracked = scope.Repository.Where(c => c.Age >= 40).ToList();
        tracked.Should().HaveCount(2);

        _ = scope.Repository.WhereReadOnly(c => c.Age >= 40).ToList();
        scope.Context.ChangeTracker.Entries().Count().Should().Be(2, "apenas o Where tracked rastreou");
    }

    [Fact]
    public void WherePaged_returns_page_and_total()
    {
        SeedCustomers(
            NewCustomer("A", 30), NewCustomer("B", 30), NewCustomer("C", 30),
            NewCustomer("D", 30), NewCustomer("E", 10));

        using var scope = new EFRepositoryScope(CreateContext());

        var page0 = scope.Repository.WherePaged(c => c.Age == 30, out var total, index: 0, size: 3).ToList();
        total.Should().Be(4, "total conta TODOS os que casam com o predicado");
        page0.Should().HaveCount(3);

        var page1 = scope.Repository.WherePaged(c => c.Age == 30, out total, index: 1, size: 3).ToList();
        page1.Should().HaveCount(1, "última página parcial");
        total.Should().Be(4);
    }

    [Fact]
    public void Delete_entity_removes_the_row()
    {
        var id = SeedCustomers(NewCustomer("Ana"));

        using (var scope = new EFRepositoryScope(CreateContext()))
        {
            var entity = scope.Repository.Get(id);
            scope.Repository.Delete(entity);
            scope.Context.SaveChanges();
        }

        using var verify = new EFRepositoryScope(CreateContext());
        verify.Repository.All().Count().Should().Be(0);
    }

    [Fact]
    public void Delete_by_predicate_removes_only_matches()
    {
        SeedCustomers(NewCustomer("Ana", 18), NewCustomer("Bia", 40), NewCustomer("Caio", 70));

        using (var scope = new EFRepositoryScope(CreateContext()))
        {
            scope.Repository.Delete(c => c.Age >= 40);
            scope.Context.SaveChanges();
        }

        using var verify = new EFRepositoryScope(CreateContext());
        verify.Repository.All().Single().Name.Should().Be("Ana");
    }

    [Fact]
    public void Delete_throws_on_null_entity()
    {
        using var scope = new EFRepositoryScope(CreateContext());
        var act = () => scope.Repository.Delete((Customer)null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Update_marks_detached_entity_as_modified_and_persists()
    {
        var id = SeedCustomers(NewCustomer("Ana", 30));

        using (var scope = new EFRepositoryScope(CreateContext()))
        {
            var detached = new Customer { Name = "Ana Maria", Age = 31 };
            detached.SetId(id);

            scope.Repository.Update(detached);
            scope.Context.Entry(detached).State.Should().Be(EntityState.Modified);
            scope.Context.SaveChanges();
        }

        using var verify = new EFRepositoryScope(CreateContext());
        var reloaded = verify.Repository.Get(id);
        reloaded.Name.Should().Be("Ana Maria");
        reloaded.Age.Should().Be(31);
    }

    [Fact]
    public void Merge_attaches_detached_entity_as_unchanged()
    {
        var id = SeedCustomers(NewCustomer("Ana", 30));

        using var scope = new EFRepositoryScope(CreateContext());
        var detached = new Customer { Name = "Ana", Age = 30 };
        detached.SetId(id);

        var merged = scope.Repository.Merge(detached);

        merged.Should().BeSameAs(detached);
        scope.Context.Entry(detached).State.Should().Be(EntityState.Unchanged,
            "Attach com PK preenchida rastreia sem marcar como Modified");
    }

    [Fact]
    public void Refresh_restores_database_values_over_local_changes()
    {
        var id = SeedCustomers(NewCustomer("Ana", 30));

        using var scope = new EFRepositoryScope(CreateContext());
        var entity = scope.Repository.Get(id);
        entity.Name = "Renomeada";

        var refreshed = scope.Repository.Refresh(entity);

        refreshed.Should().BeSameAs(entity);
        entity.Name.Should().Be("Ana", "Reload descarta a mutação local");
        scope.Context.Entry(entity).State.Should().Be(EntityState.Unchanged);
    }

    [Fact]
    public void IncludeMany_loads_related_collection()
    {
        Guid blogId;
        using (var seed = CreateContext())
        {
            var blog = new Blog { Title = "blog" };
            blog.Posts.Add(new Post { BlogId = blog.Id!.Value, Content = "p1" });
            blog.Posts.Add(new Post { BlogId = blog.Id!.Value, Content = "p2" });
            seed.Blogs.Add(blog);
            seed.SaveChanges();
            blogId = blog.Id!.Value;
        }

        using var context = CreateContext();
        var repository = new EFRepository<Blog>(context);

        var loaded = repository.IncludeMany(b => b.Posts).Single(b => b.Id == blogId);

        loaded.Posts.Should().HaveCount(2);
    }

    [Fact]
    public void Dispose_does_not_dispose_the_context()
    {
        using var context = CreateContext();
        var repository = new EFRepository<Customer>(context);

        repository.Dispose();

        // O contexto continua usável: o ciclo de vida é do DI/UnitOfWork.
        var act = () => context.Customers.Count();
        act.Should().NotThrow();
    }

    [Fact]
    public void Constructor_throws_on_null_context()
    {
        var act = () => new EFRepository<Customer>(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    /// <summary>Par contexto+repositório com descarte do contexto ao final do escopo.</summary>
    private sealed class EFRepositoryScope(TestDbContext context) : IDisposable
    {
        public TestDbContext Context { get; } = context;
        public EFRepository<Customer> Repository { get; } = new(context);
        public void Dispose() => Context.Dispose();
    }
}
