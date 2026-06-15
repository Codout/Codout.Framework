using System;
using System.Linq;
using System.Threading.Tasks;
using Codout.Framework.Domain.Entities;
using Codout.Framework.EF.Conventions;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Xunit;

namespace Codout.Framework.EF.Tests;

/// <summary>
/// Prova comportamental da ClientGeneratedIdConvention: uma entidade que herda de
/// ClientGeneratedEntity (Id setado no ctor, marker IClientGeneratedId) e adicionada
/// a um agregado JÁ TRACKED deve ser inferida como Added (INSERT), não Modified.
///
/// É a regressão do bug EF Core 10 (DbUpdateConcurrencyException ao receber título):
/// sem a convenção, a PK herda ValueGeneratedOnAdd e o EF aplica "PK preenchida ⇒
/// existe ⇒ Modified" → UPDATE de linha inexistente. A convenção declara
/// ValueGeneratedNever e o EF passa a inferir Added.
/// </summary>
public class ClientGeneratedIdConventionTests : IDisposable
{
    private readonly SqliteConnection _connection;

    public ClientGeneratedIdConventionTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();
    }

    public void Dispose() => _connection.Dispose();

    // ---- Modelo de teste: Parent (root) 1-N Child, ambos client-generated ----

    public class Parent : ClientGeneratedEntity
    {
        private readonly List<Child> _children = [];
        public string Name { get; private set; } = "";
        public IReadOnlyCollection<Child> Children => _children.AsReadOnly();
        public Parent() { }
        public Parent(string name) { Name = name; }
        public Child AddChild(string label)
        {
            var c = new Child(Id!.Value, label);
            _children.Add(c);
            return c;
        }
    }

    public class Child : ClientGeneratedEntity
    {
        public Guid ParentId { get; private set; }
        public string Label { get; private set; } = "";
        public Child() { }
        public Child(Guid parentId, string label) { ParentId = parentId; Label = label; }
    }

    // Dois tipos DISTINTOS de contexto: o EF cacheia o model por tipo de DbContext,
    // então usar uma flag bool no mesmo tipo contaminaria o cache entre as configs.
    // Tipos separados garantem caches de model independentes.
    private abstract class TestContextBase(DbContextOptions options) : DbContext(options)
    {
        public DbSet<Parent> Parents => Set<Parent>();
        public DbSet<Child> Children => Set<Child>();

        protected override void OnModelCreating(ModelBuilder b)
        {
            b.Entity<Parent>(e =>
            {
                e.ToTable("Parents");
                e.HasKey(x => x.Id);
                e.Property(x => x.Name);
                e.HasMany(x => x.Children).WithOne().HasForeignKey(c => c.ParentId);
            });
            b.Entity<Child>(e =>
            {
                e.ToTable("Children");
                e.HasKey(x => x.Id);
                e.Property(x => x.ParentId);
                e.Property(x => x.Label);
            });
        }
    }

    private sealed class ConventionContext(DbContextOptions options) : TestContextBase(options)
    {
        protected override void ConfigureConventions(ModelConfigurationBuilder cb)
        {
            base.ConfigureConventions(cb);
            cb.AddCodoutClientGeneratedIdConvention(); // <-- a convenção sob teste
        }
    }

    private sealed class PlainContext(DbContextOptions options) : TestContextBase(options)
    {
        // Sem a convenção: baseline que reproduz o bug (PK Guid → OnAdd → Modified).
    }

    private DbContextOptions Options() =>
        new DbContextOptionsBuilder().UseSqlite(_connection).Options;

    [Fact]
    public void Convention_sets_PK_to_ValueGeneratedNever_for_marked_entities()
    {
        using var ctx = new ConventionContext(Options());
        foreach (var clr in new[] { typeof(Parent), typeof(Child) })
        {
            var pk = ctx.Model.FindEntityType(clr)!.FindPrimaryKey()!;
            pk.Properties.Single().ValueGenerated
                .Should().Be(ValueGenerated.Never, $"{clr.Name} é IClientGeneratedId");
        }
    }

    [Fact]
    public void Without_convention_PK_stays_ValueGeneratedOnAdd_reproducing_the_bug()
    {
        using var ctx = new PlainContext(Options());
        ctx.Model.FindEntityType(typeof(Child))!.FindPrimaryKey()!.Properties.Single()
            .ValueGenerated.Should().Be(ValueGenerated.OnAdd, "default do EF sem a convenção");
    }

    [Fact]
    public async Task New_child_added_to_tracked_parent_is_Added_and_persists_with_convention()
    {
        Guid parentId;
        using (var seed = new ConventionContext(Options()))
        {
            seed.Database.EnsureCreated();
            var p = new Parent("agregado");
            seed.Parents.Add(p);
            await seed.SaveChangesAsync();
            parentId = p.Id!.Value;
        }

        using var ctx = new ConventionContext(Options());
        var parent = await ctx.Parents.Include(x => x.Children).FirstAsync(x => x.Id == parentId);

        var child = parent.AddChild("novo");          // Id setado no ctor (ClientGeneratedEntity)
        ctx.ChangeTracker.DetectChanges();

        ctx.Entry(child).State.Should().Be(EntityState.Added, "convenção ⇒ ValueGeneratedNever ⇒ Added");

        await ctx.SaveChangesAsync();                 // sem convenção isto seria UPDATE-0-rows → exception
        using var verify = new ConventionContext(Options());
        (await verify.Children.CountAsync()).Should().Be(1);
    }

    [Fact]
    public async Task Without_convention_new_child_is_misinferred_as_Modified()
    {
        Guid parentId;
        using (var seed = new PlainContext(Options()))
        {
            seed.Database.EnsureCreated();
            var p = new Parent("agregado");
            seed.Parents.Add(p);
            await seed.SaveChangesAsync();
            parentId = p.Id!.Value;
        }

        using var ctx = new PlainContext(Options());
        var parent = await ctx.Parents.Include(x => x.Children).FirstAsync(x => x.Id == parentId);

        var child = parent.AddChild("novo");
        ctx.ChangeTracker.DetectChanges();

        // Reproduz o bug: sem a convenção, o registro novo (Id no ctor) é Modified.
        ctx.Entry(child).State.Should().Be(EntityState.Modified, "bug que a convenção corrige");
    }
}
