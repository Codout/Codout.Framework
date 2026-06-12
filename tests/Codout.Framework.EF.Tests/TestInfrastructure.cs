using Codout.Framework.Domain.Entities;
using Codout.Framework.EF.Conventions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Codout.Framework.EF.Tests;

// ---------------------------------------------------------------------------
// Modelo compartilhado pelos testes de EFRepository / EFUnitOfWork /
// Interceptors / Specifications, sobre SQLite in-memory.
// ---------------------------------------------------------------------------

/// <summary>Agregado simples com Guid client-generated.</summary>
public class Customer : ClientGeneratedEntity
{
    public string Name { get; set; } = "";
    public int Age { get; set; }
}

/// <summary>Entidade com PK int store-generated (autoincrement) — chave default = 0.</summary>
public class Invoice : Entity<int>
{
    public string Number { get; set; } = "";
}

/// <summary>Agregado com coleção, para Include / specifications.</summary>
public class Blog : ClientGeneratedEntity
{
    public string Title { get; set; } = "";
    public List<Post> Posts { get; set; } = [];
}

public class Post : ClientGeneratedEntity
{
    public Guid BlogId { get; set; }
    public string Content { get; set; } = "";
}

/// <summary>Implementa a IAuditable LOCAL do pacote EF (Codout.Framework.EF.Interceptors).</summary>
public class AuditedDocument : ClientGeneratedEntity, Interceptors.IAuditable
{
    public string Title { get; set; } = "";
    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
}

/// <summary>
/// Implementa SOMENTE a abstração pública Codout.Framework.Data.Auditing.IAuditable —
/// usada para caracterizar que o AuditableInterceptor NÃO a reconhece (ver FINDINGS-B.md).
/// </summary>
public class DataAuditedDocument : ClientGeneratedEntity, Data.Auditing.IAuditable
{
    public string Title { get; set; } = "";
    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
}

/// <summary>Entidade soft-deletable (abstração pública Data.Auditing.ISoftDeletable).</summary>
public class SoftItem : ClientGeneratedEntity, Data.Auditing.ISoftDeletable
{
    public string Name { get; set; } = "";
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }
}

public class TestDbContext(DbContextOptions<TestDbContext> options) : DbContext(options)
{
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Invoice> Invoices => Set<Invoice>();
    public DbSet<Blog> Blogs => Set<Blog>();
    public DbSet<Post> Posts => Set<Post>();
    public DbSet<AuditedDocument> AuditedDocuments => Set<AuditedDocument>();
    public DbSet<DataAuditedDocument> DataAuditedDocuments => Set<DataAuditedDocument>();
    public DbSet<SoftItem> SoftItems => Set<SoftItem>();

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        base.ConfigureConventions(configurationBuilder);
        configurationBuilder.AddCodoutClientGeneratedIdConvention();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Blog>()
            .HasMany(b => b.Posts)
            .WithOne()
            .HasForeignKey(p => p.BlogId);
    }
}

/// <summary>
/// Base dos testes: abre uma conexão SQLite in-memory por classe de teste (o banco
/// vive enquanto a conexão estiver aberta) e fabrica DbContexts independentes que
/// compartilham o mesmo banco.
/// </summary>
public abstract class SqliteTestBase : IDisposable
{
    private readonly SqliteConnection _connection;

    protected SqliteTestBase()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        using var context = CreateContext();
        context.Database.EnsureCreated();
    }

    protected TestDbContext CreateContext(params IInterceptor[] interceptors)
    {
        var builder = new DbContextOptionsBuilder<TestDbContext>().UseSqlite(_connection);

        if (interceptors.Length > 0)
            builder.AddInterceptors(interceptors);

        return new TestDbContext(builder.Options);
    }

    public void Dispose() => _connection.Dispose();
}
