# Codout.Framework.EF

Implementação enterprise do padrão Repository e Unit of Work para Entity Framework Core 10.

## 🚀 Recursos

- ✅ **Repository Pattern** com suporte async/await completo
- ✅ **Unit of Work Pattern** com transações robustas
- ✅ **Specification Pattern** para queries complexas reutilizáveis
- ✅ **Interceptors** para auditoria e soft delete automáticos
- ✅ **Builder Fluente** para configuração avançada
- ✅ **CancellationToken** em todas operações async
- ✅ **Retry Policies** integradas
- ✅ **IAsyncDisposable** suportado
- ✅ **Nullable Reference Types** habilitado

## 📦 Instalação

```bash
dotnet add package Codout.Framework.EF
```

## 🔧 Configuração Básica (Legado)

```csharp
// Program.cs ou Startup.cs
#pragma warning disable CS0618
services.AddEFCore<MyDbContext>(configuration);
#pragma warning restore CS0618
```

## 🔧 Configuração Avançada (Recomendado)

```csharp
services.AddEFCore<MyDbContext>(configuration)
    .WithConnectionStringFromConfiguration("DefaultConnection")
    .UseSqlServer()
    .EnableAuditing()
    .EnableSoftDelete()
    .EnableRetryOnFailure(maxRetryCount: 5)
    .EnableDetailedErrors()
    .Build();
```

### Configuração para Desenvolvimento

```csharp
#if DEBUG
services.AddEFCore<MyDbContext>(configuration)
    .WithConnectionStringFromConfiguration()
    .UseSqlServer()
    .EnableSensitiveDataLogging()
    .EnableDetailedErrors()
    .Build();
#endif
```

### Configuração com Connection String Manual

```csharp
services.AddEFCore<MyDbContext>(configuration)
    .WithConnectionString("Server=localhost;Database=MyDb;...")
    .UseSqlServer()
    .Build();
```

## 💡 Uso Básico

### Repository

```csharp
public class ProductRepository : EFRepository<Product>
{
    public ProductRepository(MyDbContext context) : base(context)
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

### Unit of Work

```csharp
public class MyUnitOfWork : EFUnitOfWork<MyDbContext>
{
    public MyUnitOfWork(MyDbContext context) : base(context)
    {
    }
}

// Uso com async/await
await using var uow = new MyUnitOfWork(context);
await uow.BeginTransactionAsync(ct);

try
{
    await repository.SaveAsync(product, ct);
    await uow.CommitAsync(ct);
}
catch
{
    await uow.RollbackAsync(ct);
    throw;
}
```

### InTransaction Helper

```csharp
var product = await uow.InTransactionAsync(async () =>
{
    var newProduct = new Product { Name = "Test" };
    await repository.SaveAsync(newProduct);
    return newProduct;
}, ct);
```

## 🎯 Specification Pattern

### Criando Specifications

```csharp
using Codout.Framework.EF.Specifications;
using Codout.Framework.Data.Specifications;

public class ActiveProductsSpecification : Specification<Product>
{
    public ActiveProductsSpecification()
    {
        AddCriteria(p => p.IsActive && !p.IsDeleted);
        ApplyOrderBy(q => q.OrderBy(p => p.Name));
        AddInclude(p => p.Category);
        ApplyNoTracking();
    }
}

public class ProductsByCategorySpecification : Specification<Product>
{
    public ProductsByCategorySpecification(int categoryId, int page, int pageSize)
    {
        AddCriteria(p => p.CategoryId == categoryId);
        ApplyOrderBy(q => q.OrderByDescending(p => p.CreatedAt));
        AddInclude(p => p.Category);
        AddInclude("Reviews"); // String-based include
        ApplyPaging((page - 1) * pageSize, pageSize);
    }
}
```

### Usando Specifications

```csharp
var spec = new ActiveProductsSpecification();
var products = await repository.ListAsync(spec, ct);

var categorySpec = new ProductsByCategorySpecification(categoryId: 5, page: 1, pageSize: 20);
var pagedProducts = await repository.ListAsync(categorySpec, ct);

var count = await repository.CountAsync(spec, ct);
var exists = await repository.AnyAsync(spec, ct);
var first = await repository.FirstOrDefaultAsync(spec, ct);
```

## 🔍 Auditoria Automática

### Implementar IAuditable

```csharp
using Codout.Framework.Data.Auditing;

public class Product : IEntity, IAuditable
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    
    // Auditoria automática - preenchido pelo interceptor
    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
    
    public bool IsTransient() => Id == 0;
    public IEnumerable<PropertyInfo> GetSignatureProperties() => 
        new[] { typeof(Product).GetProperty(nameof(Id))! };
}
```

### Configurar Provider de Usuário

```csharp
using Codout.Framework.Data.Auditing;

public class HttpContextUserProvider : ICurrentUserProvider
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    
    public HttpContextUserProvider(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }
    
    public string? GetCurrentUserId()
    {
        return _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    }
}

// Registrar
services.AddHttpContextAccessor();
services.AddSingleton<ICurrentUserProvider, HttpContextUserProvider>();
```

## 🗑️ Soft Delete Automático

```csharp
using Codout.Framework.Data.Auditing;

public class Product : IEntity, ISoftDeletable
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    
    // Soft delete automático - preenchido pelo interceptor
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }
    
    public bool IsTransient() => Id == 0;
    public IEnumerable<PropertyInfo> GetSignatureProperties() => 
        new[] { typeof(Product).GetProperty(nameof(Id))! };
}

// Configurar no DbContext
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    // Query filter global para excluir registros deletados
    modelBuilder.Entity<Product>()
        .HasQueryFilter(p => !p.IsDeleted);
}
```

## 🔄 Operações com CancellationToken

```csharp
var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));

// Métodos auxiliares com CancellationToken
var product = await repository.GetAsync(p => p.Id == 1, cts.Token);
var first = await repository.FirstOrDefaultAsync(p => p.IsActive, cts.Token);
var count = await repository.CountAsync(p => p.IsActive, cts.Token);
var exists = await repository.AnyAsync(p => p.Name == "Test", cts.Token);
var list = await repository.ToListAsync(p => p.Price > 100, cts.Token);

// Transações com CancellationToken
await uow.BeginTransactionAsync(cts.Token);
await uow.CommitAsync(cts.Token);
await uow.RollbackAsync(cts.Token);
```

## 📊 Operações Avançadas

### Include com Navigation Properties

```csharp
var products = repository
    .IncludeMany(
        p => p.Category,
        p => p.Reviews
    )
    .Where(p => p.IsActive)
    .ToList();

// String-based includes
var productsWithStrings = repository
    .IncludeMany("Category", "Reviews.User")
    .Where(p => p.IsActive)
    .ToList();
```

### Paginação

```csharp
var products = repository.WherePaged(
    predicate: p => p.IsActive,
    out int total,
    index: 0,
    size: 20
);

Console.WriteLine($"Total: {total}, Current Page: {products.Count()}");
```

### Read-Only Queries

```csharp
// Melhor performance para queries read-only (AsNoTracking automático)
var products = repository
    .WhereReadOnly(p => p.CategoryId == 1)
    .ToList();

var allReadOnly = repository.AllReadOnly();
```

### Operações CRUD Completas

```csharp
// Save
var saved = await repository.SaveAsync(product, ct);

// Update
await repository.UpdateAsync(product, ct);

// SaveOrUpdate (detecta se é novo ou existente)
var savedOrUpdated = await repository.SaveOrUpdateAsync(product, ct);

// Delete
await repository.DeleteAsync(product, ct);

// Delete com predicate
await repository.DeleteAsync(p => p.IsActive == false, ct);

// Merge (reattach detached entity)
var merged = await repository.MergeAsync(product, ct);

// Refresh (reload from database)
var refreshed = await repository.RefreshAsync(product, ct);
```

## 🏗️ Arquitetura Recomendada

```
📁 MyProject.Data
├── 📁 Context
│   └── MyDbContext.cs
├── 📁 Repositories
│   ├── IProductRepository.cs
│   └── ProductRepository.cs
├── 📁 Specifications
│   ├── ActiveProductsSpecification.cs
│   └── ProductsByCategorySpecification.cs
├── 📁 UnitOfWork
│   ├── IMyUnitOfWork.cs
│   └── MyUnitOfWork.cs
└── 📁 Entities
    └── Product.cs
```

## 🎓 Melhores Práticas

1. **Use CancellationToken** em todas operações async
2. **Use Specifications** para queries complexas reutilizáveis
3. **Evite expor IQueryable** fora da camada de dados
4. **Use AsNoTracking** para queries read-only (via `WhereReadOnly`)
5. **Implemente IAuditable** para auditoria automática
6. **Use ISoftDeletable** em vez de delete físico
7. **Configure retry policies** para resiliência
8. **Use scoped lifetime** para DbContext (padrão)
9. **Configure query filters** no DbContext para soft delete
10. **Use `WithConnectionStringFromConfiguration`** para ambientes diferentes
11. **Use `await using`** para dispose automático do UnitOfWork
12. **Trate exceções** apropriadamente em transações

## 📝 Breaking Changes

### Migração do Método Legado

```csharp
// ❌ Legado (deprecated)
services.AddEFCore<MyDbContext>(configuration);

// ✅ Novo (recomendado)
services.AddEFCore<MyDbContext>(configuration)
    .WithConnectionStringFromConfiguration()
    .UseSqlServer()
    .Build();
```

### Interfaces Movidas

```csharp
// ❌ Antes
using Codout.Framework.EF.Interceptors;

// ✅ Agora (interfaces no projeto base)
using Codout.Framework.Data.Auditing;

public class Product : IAuditable, ISoftDeletable { }
```

## 🆕 Novidades v10.0

### Novos Recursos
- ✨ **Specification Pattern** completo
- ✨ **Métodos auxiliares**: `FirstOrDefaultAsync`, `AnyAsync`, `CountAsync`, `ToListAsync`
- ✨ **IAsyncDisposable** no UnitOfWork
- ✨ **Interfaces de auditoria** no `Codout.Framework.Data`
- ✨ **Builder fluente** (`EFCoreBuilder`)
- ✨ **Interceptors** para auditoria e soft delete

### Correções Críticas
- 🐛 **Transações corrigidas** - `Commit()` agora requer `BeginTransaction()` explícito
- 🐛 **Dispose do Repository** - Não faz mais dispose do DbContext
- 🐛 **Exception handling** - Não engole mais exceções em transações

### Melhorias
- ⚡ **Performance** com `AsNoTracking` e `WhereReadOnly`
- 📚 **Documentação XML** completa
- 🔒 **Nullable reference types** habilitado
- 🎯 **CancellationToken** em todas operações async

## 🔗 Links Relacionados

- [Codout.Framework.Data](../Codout.Framework.Data/README.md) - Abstrações base
- [Codout.Framework.Mongo](../Codout.Framework.Mongo/README.md) - Implementação MongoDB
- [Codout.Framework.NH](../Codout.Framework.NH/README.md) - Implementação NHibernate

## 📄 Licença

Propriedade da Codout

---

**Versão:** 10.0.0  
**Status:** Estável para produção  
**Target:** .NET 10  
**EF Core:** 10.0.0
