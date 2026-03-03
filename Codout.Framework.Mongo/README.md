# Codout.Framework.Mongo

Implementação do padrão Repository e Unit of Work para MongoDB.

## ?? Recursos

- ? **Repository Pattern** com MongoDB
- ? **Unit of Work Pattern** com suporte a transações (replica set)
- ? **Async/await completo** com CancellationToken
- ? **Métodos auxiliares** (FirstOrDefault, Any, Count, ToList)
- ? **IAsyncDisposable** suportado
- ? **Nullable Reference Types** habilitado
- ? **Documentação XML** completa

## ?? Instalação

```bash
dotnet add package Codout.Framework.Mongo
```

## ?? Requisitos

### Transações MongoDB
Para usar transações (Unit of Work), você precisa:
- ? MongoDB 4.0+ com **replica set** configurado
- ? Ou MongoDB 4.2+ com **sharded cluster**

**Nota**: Transações NÃO funcionam em MongoDB standalone.

## ?? Configuração

### appsettings.json

```json
{
  "MongoDB": {
    "ConnectionString": "mongodb://localhost:27017",
    "DatabaseName": "MyDatabase"
  }
}
```

### Program.cs

```csharp
using Codout.Framework.Mongo;
using Codout.Framework.Data;
using MongoDB.Driver;

// Configurar MongoDB Client
services.AddSingleton<IMongoClient>(sp =>
{
    var connectionString = configuration["MongoDB:ConnectionString"];
    return new MongoClient(connectionString);
});

// Configurar Database
services.AddScoped<IMongoDatabase>(sp =>
{
    var client = sp.GetRequiredService<IMongoClient>();
    var databaseName = configuration["MongoDB:DatabaseName"];
    return client.GetDatabase(databaseName);
});

// Configurar Context
services.AddScoped<MongoDbContext>();

// Configurar Unit of Work
services.AddScoped<IUnitOfWork, MongoUnitOfWork>();
```

## ?? Uso Básico

### Entidades

```csharp
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Codout.Framework.Data.Entity;
using System.Reflection;

public class Product : IEntity
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;
    
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public bool IsActive { get; set; }
    
    public bool IsTransient() => string.IsNullOrEmpty(Id);
    
    public IEnumerable<PropertyInfo> GetSignatureProperties()
    {
        return new[] { typeof(Product).GetProperty(nameof(Id))! };
    }
}
```

### Repository

```csharp
public class ProductRepository : MongoRepository<Product>
{
    public ProductRepository(MongoDbContext context) : base(context)
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
    private readonly MongoRepository<Product> _repository;
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

// Com filtro
var active = repository.Where(p => p.IsActive);

// Read-only (mesma performance no MongoDB)
var readOnly = repository.AllReadOnly();

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

// Load/Get por ID
var byId = await repository.GetAsync(objectId, ct);
var loaded = await repository.LoadAsync(objectId, ct); // Alias de GetAsync
```

### Command Methods

```csharp
// Salvar (Insert)
await repository.SaveAsync(product, ct);

// Atualizar (Replace)
await repository.UpdateAsync(product, ct);

// SaveOrUpdate (insert se novo, replace se existente)
await repository.SaveOrUpdateAsync(product, ct);

// Deletar
await repository.DeleteAsync(product, ct);

// Deletar com filtro
await repository.DeleteAsync(p => p.IsActive == false, ct);

// Merge (reattach - MongoDB usa ReplaceOne)
var merged = await repository.MergeAsync(product, ct);

// Refresh (re-carregar do banco)
var refreshed = await repository.RefreshAsync(product, ct);
```

## ?? Limitações do MongoDB

### 1. Includes (Relacionamentos)

MongoDB não suporta `Include` nativo como EF Core:

```csharp
// ? Não funciona como esperado
var products = repository.IncludeMany(p => p.Category);

// ? Use agregações ou lookups do MongoDB
var collection = context.GetCollection<Product>();
var productsWithCategory = await collection.Aggregate()
    .Lookup("categories", "categoryId", "_id", "category")
    .ToListAsync(ct);
```

### 2. Transações

Transações requerem replica set:

```bash
# Configurar replica set local (desenvolvimento)
mongod --replSet rs0 --port 27017 --dbpath /data/db1

# Inicializar replica set
mongosh
> rs.initiate()
```

### 3. IQueryable Limitado

O driver MongoDB suporta LINQ, mas não todos os operadores:

```csharp
// ? Funciona
var products = repository.Where(p => p.Price > 100 && p.IsActive);

// ?? Pode não funcionar
var products = repository.Where(p => p.Name.StartsWith("A") || p.Name.EndsWith("Z"));

// ? Alternativa: usar filtros do MongoDB
var filter = Builders<Product>.Filter.Or(
    Builders<Product>.Filter.Regex("name", new BsonRegularExpression("^A")),
    Builders<Product>.Filter.Regex("name", new BsonRegularExpression("Z$"))
);
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

## ??? Arquitetura Recomendada

```
?? MyProject.Data.Mongo
??? ?? Context
?   ??? MyMongoContext.cs (se precisar estender)
??? ?? Repositories
?   ??? IProductRepository.cs
?   ??? ProductRepository.cs
??? ?? Entities
    ??? Product.cs
```

## ?? Melhores Práticas

1. **Use ObjectId para IDs**: MongoDB funciona melhor com ObjectId
2. **Configure índices**: Use atributos `[BsonIndex]` ou configure via código
3. **Use CancellationToken**: Sempre em operações async
4. **Transações apenas quando necessário**: Têm overhead de performance
5. **Evite queries muito complexas**: Use agregações do MongoDB diretamente
6. **Configure Write Concern**: Para garantir persistência em replica set
7. **Use await using**: Para dispose automático do UnitOfWork
8. **Valide ObjectId**: Antes de fazer queries por ID

## ?? Índices

```csharp
// Via código na configuração
var collection = database.GetCollection<Product>("products");

var indexKeysDefinition = Builders<Product>.IndexKeys
    .Ascending(p => p.Name)
    .Descending(p => p.Price);

await collection.Indexes.CreateOneAsync(
    new CreateIndexModel<Product>(indexKeysDefinition),
    cancellationToken: ct
);
```

## ?? Novidades v10.0

### Novos Recursos
- ? **MongoUnitOfWork** criado do zero com transações
- ? **Métodos auxiliares**: `FirstOrDefaultAsync`, `AnyAsync`, `CountAsync`, `ToListAsync`
- ? **IAsyncDisposable** implementado
- ? **Sobrecargas com CancellationToken** em todos métodos async
- ? **Validações** com `ArgumentNullException.ThrowIfNull`

### Melhorias
- ? **Performance** otimizada para operações batch
- ?? **Documentação XML** completa
- ?? **Nullable reference types** habilitado
- ?? **Thread-safe** implementation

### Correções
- ?? **GetIdValue** agora suporta múltiplos tipos de ID
- ?? **Exists** implementado corretamente
- ?? **Load/Refresh** agora funcionam adequadamente

## ?? Links Relacionados

- [Codout.Framework.Data](../Codout.Framework.Data/README.md) - Abstrações base
- [Codout.Framework.EF](../Codout.Framework.EF/README.md) - Implementação Entity Framework
- [Codout.Framework.NH](../Codout.Framework.NH/README.md) - Implementação NHibernate
- [MongoDB Driver .NET](https://www.mongodb.com/docs/drivers/csharp/current/)
- [MongoDB Transactions](https://www.mongodb.com/docs/manual/core/transactions/)

## ?? Licença

Propriedade da Codout

---

**Versão:** 10.0.0  
**Status:** Estável para produção  
**Target:** .NET 10  
**MongoDB Driver:** 3.5.1
