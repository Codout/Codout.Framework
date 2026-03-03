# Codout.Framework.NH

Implementação do padrão Repository e Unit of Work para NHibernate.

## ?? Recursos

- ? **Repository Pattern** com NHibernate
- ? **Unit of Work Pattern** com transações
- ? **Async/await completo** com CancellationToken
- ? **Métodos auxiliares** (FirstOrDefault, Any, Count, ToList)
- ? **FluentNHibernate** suportado
- ? **IAsyncDisposable** suportado
- ? **Nullable Reference Types** habilitado
- ? **Documentação XML** completa

## ?? Instalação

```bash
dotnet add package Codout.Framework.NH
```

## ?? Configuração

### appsettings.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=MyDb;User Id=sa;Password=***;"
  }
}
```

### Program.cs com FluentNHibernate

```csharp
using Codout.Framework.NH;
using Codout.Framework.Data;
using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using NHibernate;
using NHibernate.Tool.hbm2ddl;

// Configurar NHibernate Session Factory
services.AddSingleton<ISessionFactory>(sp =>
{
    var connectionString = configuration.GetConnectionString("DefaultConnection");
    
    return Fluently.Configure()
        .Database(MsSqlConfiguration.MsSql2012
            .ConnectionString(connectionString)
            .ShowSql())
        .Mappings(m => m.FluentMappings
            .AddFromAssemblyOf<Product>())
        .ExposeConfiguration(cfg => new SchemaUpdate(cfg).Execute(false, true))
        .BuildSessionFactory();
});

// Configurar Session (scoped)
services.AddScoped<ISession>(sp =>
{
    var factory = sp.GetRequiredService<ISessionFactory>();
    return factory.OpenSession();
});

// Configurar Unit of Work
services.AddScoped<IUnitOfWork, NHUnitOfWork>();
```

## ?? Uso Básico

### Entidades

```csharp
using Codout.Framework.Data.Entity;
using System.Reflection;

public class Product : IEntity
{
    public virtual int Id { get; set; }
    public virtual string Name { get; set; } = string.Empty;
    public virtual decimal Price { get; set; }
    public virtual bool IsActive { get; set; }
    public virtual Category? Category { get; set; }
    
    public bool IsTransient() => Id == 0;
    
    public IEnumerable<PropertyInfo> GetSignatureProperties()
    {
        return new[] { typeof(Product).GetProperty(nameof(Id))! };
    }
}
```

**Importante**: Propriedades **devem ser virtual** para permitir lazy loading e proxies do NHibernate.

### Mapeamento FluentNHibernate

```csharp
using FluentNHibernate.Mapping;

public class ProductMap : ClassMap<Product>
{
    public ProductMap()
    {
        Table("Products");
        
        Id(x => x.Id).GeneratedBy.Identity();
        
        Map(x => x.Name).Not.Nullable().Length(200);
        Map(x => x.Price).Not.Nullable();
        Map(x => x.IsActive).Not.Nullable();
        
        References(x => x.Category)
            .Column("CategoryId")
            .LazyLoad();
    }
}
```

### Repository

```csharp
public class ProductRepository : NHRepository<Product>
{
    public ProductRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
    {
    }
    
    public async Task<List<Product>> GetActiveProductsAsync(CancellationToken ct = default)
    {
        return await ToListAsync(p => p.IsActive, ct);
    }
    
    public async Task<bool> HasActiveProductsAsync(CancellationToken ct = default)
    {
        return await AnyAsync(p => p.IsActive, ct);
    }
    
    public async Task<int> CountActiveProductsAsync(CancellationToken ct = default)
    {
        return await CountAsync(p => p.IsActive, ct);
    }
}
```

### Unit of Work (com Transações)

```csharp
public class ProductService
{
    private readonly NHRepository<Product> _repository;
    private readonly IUnitOfWork _unitOfWork;

    public async Task<Product> CreateProductAsync(Product product, CancellationToken ct = default)
    {
        await _unitOfWork.BeginTransactionAsync(ct);
        
        try
        {
            await _repository.SaveAsync(product, ct);
            await _unitOfWork.CommitAsync(ct);
            return product;
        }
        catch
        {
            await _unitOfWork.RollbackAsync(ct);
            throw;
        }
    }
}
```

### InTransaction Helper

```csharp
var product = await _unitOfWork.InTransactionAsync(async () =>
{
    var newProduct = new Product { Name = "Test", Price = 100 };
    await _repository.SaveAsync(newProduct, ct);
    return newProduct;
}, ct);
```

## ?? Operações Disponíveis

### Query Methods

```csharp
// Todos os registros
var all = repository.All();

// Read-only (DefaultReadOnly = true)
var readOnly = repository.AllReadOnly();

// Com filtro
var active = repository.Where(p => p.IsActive);

// Read-only com filtro
var activeReadOnly = repository.WhereReadOnly(p => p.IsActive);

// Paginação
var paged = repository.WherePaged(p => p.IsActive, out int total, index: 0, size: 20);

// Get único
var product = await repository.GetAsync(p => p.Id == id, ct);

// FirstOrDefault
var first = await repository.FirstOrDefaultAsync(p => p.IsActive, ct);

// Any
var exists = await repository.AnyAsync(p => p.Name == "Test", ct);

// Count
var count = await repository.CountAsync(p => p.IsActive, ct);

// ToList
var list = await repository.ToListAsync(p => p.Price > 100, ct);

// Load (retorna proxy lazy - não é async nativo)
var proxy = repository.Load(id);
```

### Command Methods

```csharp
// Salvar
await repository.SaveAsync(product, ct);

// Atualizar
await repository.UpdateAsync(product, ct);

// SaveOrUpdate (NHibernate detecta automaticamente)
await repository.SaveOrUpdateAsync(product, ct);

// Deletar
await repository.DeleteAsync(product, ct);

// Deletar com filtro
await repository.DeleteAsync(p => p.IsActive == false, ct);

// Merge (detached -> persistent)
var merged = await repository.MergeAsync(product, ct);

// Refresh (re-carregar do banco)
var refreshed = await repository.RefreshAsync(product, ct);
```

## ?? Eager Loading

### NHibernate não suporta Include como EF Core

```csharp
// ? Não funciona
var products = repository.IncludeMany(p => p.Category);

// ? Use Fetch no LINQ
var products = repository.All()
    .Fetch(p => p.Category)
    .ThenFetch(c => c.Supplier)
    .ToList();

// ? Ou configure no mapeamento
public class ProductMap : ClassMap<Product>
{
    public ProductMap()
    {
        References(x => x.Category)
            .Not.LazyLoad(); // Eager load sempre
    }
}

// ? Ou use FetchMode em queries
var session = ((NHRepository<Product>)repository).Session;
var products = session.CreateCriteria<Product>()
    .SetFetchMode("Category", FetchMode.Eager)
    .List<Product>();
```

## ??? Mapeamento Avançado

### Relacionamentos

```csharp
public class ProductMap : ClassMap<Product>
{
    public ProductMap()
    {
        Table("Products");
        
        Id(x => x.Id).GeneratedBy.Identity();
        
        // Many-to-One
        References(x => x.Category)
            .Column("CategoryId")
            .LazyLoad()
            .Cascade.None();
        
        // One-to-Many
        HasMany(x => x.Reviews)
            .KeyColumn("ProductId")
            .Inverse()
            .Cascade.AllDeleteOrphan();
    }
}
```

### Componentes

```csharp
public class ProductMap : ClassMap<Product>
{
    public ProductMap()
    {
        Component(x => x.Address, m =>
        {
            m.Map(x => x.Street).Column("Street");
            m.Map(x => x.City).Column("City");
            m.Map(x => x.ZipCode).Column("ZipCode");
        });
    }
}
```

## ?? Session Management

### Acesso direto à Session

```csharp
public class ProductRepository : NHRepository<Product>
{
    public async Task<List<Product>> GetProductsWithCustomQuery(CancellationToken ct = default)
    {
        return await Session
            .CreateQuery("FROM Product p WHERE p.Price > 100")
            .ListAsync<Product>(ct);
    }
    
    public async Task BulkUpdateAsync(CancellationToken ct = default)
    {
        await Session
            .CreateQuery("UPDATE Product SET IsActive = false WHERE Price < 10")
            .ExecuteUpdateAsync(ct);
    }
}
```

## ?? Considerações NHibernate

### 1. Propriedades Virtuais

```csharp
// ? Correto - permite lazy loading
public class Product
{
    public virtual int Id { get; set; }
    public virtual string Name { get; set; }
    public virtual Category Category { get; set; }
}

// ? Errado - não permite lazy loading
public class Product
{
    public int Id { get; set; }
    public string Name { get; set; }
}
```

### 2. Flush vs Commit

```csharp
// Flush persiste no banco mas não comita a transação
await Session.FlushAsync(ct);

// Commit faz flush + commit da transação
await unitOfWork.CommitAsync(ct);
```

### 3. Detached Entities

```csharp
// Entity fora do contexto (detached)
var product = new Product { Id = 1, Name = "Updated" };

// ? Update falha se detached
await repository.UpdateAsync(product, ct);

// ? Use Merge para reattach
var merged = await repository.MergeAsync(product, ct);
await repository.UpdateAsync(merged, ct);
```

### 4. Load vs Get

```csharp
// Load - retorna proxy, lança exceção se não existir quando acessado
var proxy = repository.Load(id); // Não acessa o banco ainda
var name = proxy.Name; // Acessa o banco aqui

// Get - retorna null se não existir
var product = await repository.GetAsync(id, ct); // Acessa o banco imediatamente
if (product == null) { /* não existe */ }
```

## ?? Operações com CancellationToken

```csharp
var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));

try
{
    var product = await repository.GetAsync(p => p.Id == id, cts.Token);
    await repository.UpdateAsync(product, cts.Token);
    await unitOfWork.CommitAsync(cts.Token);
}
catch (OperationCanceledException)
{
    Console.WriteLine("Operação cancelada pelo timeout");
}
```

## ?? Melhores Práticas

1. **Use propriedades virtual**: Permite lazy loading e proxies
2. **Configure Cascade corretamente**: Evite orphan deletions acidentais
3. **Use CancellationToken**: Sempre em operações async
4. **Flush antes de queries**: Se precisar dos dados persistidos
5. **Evite N+1**: Use Fetch/FetchMany para eager loading
6. **Configure índices**: No banco ou via atributos
7. **Use Stateless Session**: Para operações batch de alto volume
8. **Use await using**: Para dispose automático do UnitOfWork
9. **Evite lazy loading em loops**: Carregue dados antecipadamente
10. **Configure cache de segundo nível**: Para performance

## ?? Índices

```csharp
public class ProductMap : ClassMap<Product>
{
    public ProductMap()
    {
        // Índice único
        Map(x => x.Name)
            .Not.Nullable()
            .Length(200)
            .UniqueKey("UK_Product_Name");
        
        // Índice composto
        Map(x => x.CategoryId).Index("IX_Product_Category_Active");
        Map(x => x.IsActive).Index("IX_Product_Category_Active");
    }
}
```

## ?? Novidades v10.0

### Novos Recursos
- ? **Métodos auxiliares**: `FirstOrDefaultAsync`, `AnyAsync`, `CountAsync`, `ToListAsync`
- ? **IAsyncDisposable** implementado no UnitOfWork
- ? **Sobrecargas com CancellationToken** em todos métodos async
- ? **Validações** com `ArgumentNullException.ThrowIfNull`
- ? **Flush explícito** antes de commit para consistência

### Melhorias
- ? **Performance** otimizada em batch operations
- ?? **Documentação XML** completa
- ?? **Nullable reference types** habilitado
- ?? **Thread-safe** implementation

### Correções
- ?? **Transações** agora fazem flush antes de commit
- ?? **Dispose** correto de transações em exceções
- ?? **LoadAsync** implementado corretamente

## ?? Links Relacionados

- [Codout.Framework.Data](../Codout.Framework.Data/README.md) - Abstrações base
- [Codout.Framework.EF](../Codout.Framework.EF/README.md) - Implementação Entity Framework
- [Codout.Framework.Mongo](../Codout.Framework.Mongo/README.md) - Implementação MongoDB
- [NHibernate Documentation](https://nhibernate.info/doc/)
- [FluentNHibernate](https://github.com/nhibernate/fluent-nhibernate)

## ?? Licença

Propriedade da Codout

---

**Versão:** 10.0.0  
**Status:** Estável para produção  
**Target:** .NET 10  
**NHibernate:** 5.6.0  
**FluentNHibernate:** 3.4.1
