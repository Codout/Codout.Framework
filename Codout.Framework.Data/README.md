# Codout.Framework.Data

**Abstrações de persistência ORM-agnostic para .NET 10**

Este projeto define os contratos (interfaces) para implementação de padrões Repository e Unit of Work, independente de tecnologia de persistência.

## ?? Objetivo

Fornecer abstrações que podem ser implementadas por qualquer ORM:
- ? Entity Framework Core ([`Codout.Framework.EF`](../Codout.Framework.EF/README.md))
- ? NHibernate ([`Codout.Framework.NH`](../Codout.Framework.NH/README.md))
- ? MongoDB ([`Codout.Framework.Mongo`](../Codout.Framework.Mongo/README.md))

## ?? Instalação

```bash
dotnet add package Codout.Framework.Data
```

## ??? Arquitetura

### Contratos Principais

#### IEntity / IEntity\<TId\>
Define a base para todas as entidades do domínio.

```csharp
public interface IEntity
{
    IEnumerable<PropertyInfo> GetSignatureProperties();
    bool IsTransient();
}

public interface IEntity<out TId> : IEntity
{
    TId Id { get; }
}
```

#### IRepository\<T\>
Define operações CRUD genéricas para entidades.

```csharp
public interface IRepository<T> : IDisposable where T : class, IEntity
{
    // Query Methods
    IQueryable<T> All();
    IQueryable<T> AllReadOnly();
    IQueryable<T> Where(Expression<Func<T, bool>> predicate);
    T Get(Expression<Func<T, bool>> predicate);
    Task<T> GetAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default);
    
    // Auxiliary Methods (v10.0+)
    Task<T> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default);
    Task<bool> AnyAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default);
    Task<int> CountAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default);
    Task<List<T>> ToListAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default);
    
    // Command Methods
    void Delete(T entity);
    T Save(T entity);
    void Update(T entity);
    Task<T> SaveAsync(T entity, CancellationToken ct = default);
    Task UpdateAsync(T entity, CancellationToken ct = default);
    
    // Paging
    IQueryable<T> WherePaged(Expression<Func<T, bool>> predicate, out int total, int index = 0, int size = 50);
    
    // Includes
    IQueryable<T> IncludeMany(params Expression<Func<T, object>>[] includes);
}
```

#### IUnitOfWork
Define o padrão Unit of Work para gerenciamento de transações.

```csharp
public interface IUnitOfWork : IDisposable, IAsyncDisposable
{
    // Transaction Management
    void BeginTransaction();
    void BeginTransaction(IsolationLevel isolationLevel);
    Task BeginTransactionAsync(CancellationToken ct = default);
    Task BeginTransactionAsync(IsolationLevel isolationLevel, CancellationToken ct = default);
    
    void Commit();
    void Commit(IsolationLevel isolationLevel);
    Task CommitAsync(CancellationToken ct = default);
    
    void Rollback();
    Task RollbackAsync(CancellationToken ct = default);
    
    // Transaction Helpers
    T InTransaction<T>(Func<T> work) where T : class, IEntity;
    Task<T> InTransactionAsync<T>(Func<Task<T>> work, CancellationToken ct = default) where T : class, IEntity;
}
```

### Specification Pattern (v10.0+)

#### ISpecification\<T\>
Define especificações reutilizáveis para consultas complexas.

```csharp
public interface ISpecification<T> where T : class, IEntity
{
    Expression<Func<T, bool>>? Criteria { get; }
    Func<IQueryable<T>, IOrderedQueryable<T>>? OrderBy { get; }
    List<Expression<Func<T, object>>> Includes { get; }
    List<string> IncludeStrings { get; }
    int Take { get; }
    int Skip { get; }
    bool IsPagingEnabled { get; }
    bool AsNoTracking { get; }
}
```

**Exemplo de Uso:**
```csharp
public class ActiveProductsSpec : Specification<Product>
{
    public ActiveProductsSpec()
    {
        AddCriteria(p => p.IsActive && !p.IsDeleted);
        ApplyOrderBy(q => q.OrderBy(p => p.Name));
        AddInclude(p => p.Category);
        ApplyNoTracking();
    }
}
```

### Auditing Interfaces (v10.0+)

#### IAuditable
Para entidades que precisam de auditoria automática.

```csharp
public interface IAuditable
{
    DateTime CreatedAt { get; set; }
    string? CreatedBy { get; set; }
    DateTime? UpdatedAt { get; set; }
    string? UpdatedBy { get; set; }
}
```

#### ISoftDeletable
Para entidades com soft delete (exclusão lógica).

```csharp
public interface ISoftDeletable
{
    bool IsDeleted { get; set; }
    DateTime? DeletedAt { get; set; }
    string? DeletedBy { get; set; }
}
```

#### ICurrentUserProvider
Provider para obter o usuário atual.

```csharp
public interface ICurrentUserProvider
{
    string? GetCurrentUserId();
}
```

**Exemplo de Implementação:**
```csharp
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
```

## ?? Implementando as Abstrações

### Exemplo: Entity Framework Core

```csharp
public class EFRepository<T> : IRepository<T> where T : class, IEntity
{
    protected readonly DbContext Context;
    protected DbSet<T> DbSet => Context.Set<T>();

    public EFRepository(DbContext context)
    {
        Context = context;
    }

    public IQueryable<T> All() => DbSet;
    
    public async Task<T> GetAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default) =>
        await DbSet.SingleOrDefaultAsync(predicate, ct);
    
    public async Task<bool> AnyAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default) =>
        await DbSet.AnyAsync(predicate, ct);
    
    // ... implementar outros métodos
}
```

### Exemplo: Unit of Work

```csharp
public class EFUnitOfWork : IUnitOfWork
{
    private readonly DbContext _context;
    private IDbContextTransaction? _transaction;

    public EFUnitOfWork(DbContext context)
    {
        _context = context;
    }

    public async Task BeginTransactionAsync(CancellationToken ct = default)
    {
        _transaction = await _context.Database.BeginTransactionAsync(ct);
    }

    public async Task CommitAsync(CancellationToken ct = default)
    {
        await _context.SaveChangesAsync(ct);
        await _transaction?.CommitAsync(ct);
    }
    
    // ... implementar outros métodos
}
```

## ?? Exemplo Completo

```csharp
// Entidade
public class Product : IEntity<int>, IAuditable, ISoftDeletable
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    
    // IAuditable
    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
    
    // ISoftDeletable
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }
    
    public IEnumerable<PropertyInfo> GetSignatureProperties() => 
        new[] { typeof(Product).GetProperty(nameof(Id))! };
    
    public bool IsTransient() => Id == 0;
}

// Specification
public class ActiveProductsSpec : ISpecification<Product>
{
    public Expression<Func<Product, bool>>? Criteria => p => p.IsActive && !p.IsDeleted;
    public Func<IQueryable<Product>, IOrderedQueryable<Product>>? OrderBy => q => q.OrderBy(p => p.Name);
    public List<Expression<Func<Product, object>>> Includes { get; } = new();
    public List<string> IncludeStrings { get; } = new();
    public int Take { get; }
    public int Skip { get; }
    public bool IsPagingEnabled => false;
    public bool AsNoTracking => true;
}

// Uso
public class ProductService
{
    private readonly IRepository<Product> _repository;
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
    
    public async Task<List<Product>> GetActiveProductsAsync(CancellationToken ct = default)
    {
        var spec = new ActiveProductsSpec();
        return await _repository.ToListAsync(spec.Criteria!, ct);
    }
}
```

## ?? Novidades v10.0

### Breaking Changes
**Nenhum!** Todas as mudanças são aditivas e retrocompatíveis.

### Novas Interfaces
- ? `ISpecification<T>` - Specification Pattern
- ? `IAuditable` - Auditoria automática
- ? `ISoftDeletable` - Soft delete
- ? `ICurrentUserProvider` - Provider de usuário

### Métodos Adicionados

#### IUnitOfWork
- ? `Task BeginTransactionAsync(CancellationToken)`
- ? `Task BeginTransactionAsync(IsolationLevel, CancellationToken)`
- ? `Task CommitAsync(CancellationToken)`
- ? `Task RollbackAsync(CancellationToken)`
- ? `Task<T> InTransactionAsync<T>(Func<Task<T>>, CancellationToken)`
- ? `IAsyncDisposable` support

#### IRepository\<T\>
- ? Sobrecargas com `CancellationToken` em todos métodos async
- ? `Task<T> FirstOrDefaultAsync(Expression, CancellationToken)`
- ? `Task<bool> AnyAsync(Expression, CancellationToken)`
- ? `Task<int> CountAsync(Expression, CancellationToken)`
- ? `Task<List<T>> ToListAsync(Expression, CancellationToken)`

### Deprecations
- ?? `IUnitOfWorkProvider<T>` - Use DI direto em vez de factory pattern

## ?? Compatibilidade

| Implementação | Status | Versão | Link |
|---------------|--------|--------|------|
| **Entity Framework Core** | ? Completo | 10.0.0 | [README](../Codout.Framework.EF/README.md) |
| **NHibernate** | ? Completo | 10.0.0 | [README](../Codout.Framework.NH/README.md) |
| **MongoDB** | ? Completo | 10.0.0 | [README](../Codout.Framework.Mongo/README.md) |

## ?? Próximos Passos

1. Escolha uma implementação:
   - [Codout.Framework.EF](../Codout.Framework.EF/README.md) - Entity Framework Core
   - [Codout.Framework.NH](../Codout.Framework.NH/README.md) - NHibernate
   - [Codout.Framework.Mongo](../Codout.Framework.Mongo/README.md) - MongoDB

2. Implemente suas entidades usando `IEntity` ou `IEntity<TId>`

3. Adicione auditoria com `IAuditable` e soft delete com `ISoftDeletable`

4. Use `ISpecification<T>` para queries complexas reutilizáveis

5. Gerencie transações com `IUnitOfWork`

## ?? Qualidade

- ? **100% retrocompatível** com versões anteriores
- ? **Nullable Reference Types** habilitado
- ? **Documentação XML** completa
- ? **CancellationToken** em todas operações async
- ? **IAsyncDisposable** suportado
- ? **Specification Pattern** para queries complexas
- ? **Auditing** e **Soft Delete** built-in

## ?? Comparação de Implementações

| Recurso | EF Core | NHibernate | MongoDB |
|---------|---------|------------|---------|
| Transações | ? | ? | ? (replica set) |
| Lazy Loading | ? | ? (virtual) | ? |
| Change Tracking | ? | ? | ? |
| LINQ Support | ??? | ?? | ? |
| Migrations | ? | ? | ? |
| NoSQL | ? | ? | ? |
| Interceptors | ? | ?? | ? |
| Specifications | ? | ? | ? |

## ?? Licença

Propriedade da Codout

---

**Versão:** 10.0.0  
**Status:** Estável para produção  
**Target:** .NET 10
