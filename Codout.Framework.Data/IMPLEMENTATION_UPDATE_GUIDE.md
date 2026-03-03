# Guia de Atualização para Implementações

## ?? Objetivo

Este guia ajuda mantenedores de implementações (`Codout.Framework.NH`, `Codout.Framework.Mongo`) a atualizar seus projetos para as novas interfaces do `Codout.Framework.Data` v10.0.

## ?? Mudanças Necessárias

### IRepository\<T\> - Novas Sobrecargas

Todos os métodos async agora possuem sobrecargas com `CancellationToken`:

```csharp
// ? Adicionar estas sobrecarg

Task<T> GetAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken);
Task<T> GetAsync(object key, CancellationToken cancellationToken);
Task<T> LoadAsync(object key, CancellationToken cancellationToken);
Task DeleteAsync(T entity, CancellationToken cancellationToken);
Task DeleteAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken);
Task<T> SaveAsync(T entity, CancellationToken cancellationToken);
Task<T> SaveOrUpdateAsync(T entity, CancellationToken cancellationToken);
Task UpdateAsync(T entity, CancellationToken cancellationToken);
Task<T> MergeAsync(T entity, CancellationToken cancellationToken);
Task<T> RefreshAsync(T entity, CancellationToken cancellationToken);
```

### IRepository\<T\> - Novos Métodos Auxiliares

```csharp
// ? Implementar estes novos métodos
Task<T> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
Task<bool> AnyAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
Task<int> CountAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
Task<List<T>> ToListAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
```

### IUnitOfWork - Métodos Async

```csharp
// ? Implementar suporte async completo
Task BeginTransactionAsync(CancellationToken cancellationToken = default);
Task BeginTransactionAsync(IsolationLevel isolationLevel, CancellationToken cancellationToken = default);
Task CommitAsync(CancellationToken cancellationToken = default);
Task RollbackAsync(CancellationToken cancellationToken = default);
Task<T> InTransactionAsync<T>(Func<Task<T>> work, CancellationToken cancellationToken = default) where T : class, IEntity;
```

### IUnitOfWork - IAsyncDisposable

```csharp
// ? Implementar IAsyncDisposable
public async ValueTask DisposeAsync()
{
    // Dispose de recursos async
    await DisposeAsyncCore();
    Dispose(false);
    GC.SuppressFinalize(this);
}
```

## ?? Exemplo: NHibernate

### Antes (v9.x)

```csharp
public class NHRepository<T> : IRepository<T> where T : class, IEntity
{
    public async Task<T> GetAsync(Expression<Func<T, bool>> predicate)
    {
        return await Session.Query<T>().SingleOrDefaultAsync(predicate);
    }
}
```

### Depois (v10.0)

```csharp
public class NHRepository<T> : IRepository<T> where T : class, IEntity
{
    // Manter método original para retrocompatibilidade
    public async Task<T> GetAsync(Expression<Func<T, bool>> predicate)
    {
        return await GetAsync(predicate, CancellationToken.None);
    }
    
    // Nova sobrecarga com CancellationToken
    public async Task<T> GetAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken)
    {
        return await Session.Query<T>().SingleOrDefaultAsync(predicate, cancellationToken);
    }
    
    // Novos métodos auxiliares
    public async Task<T> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await Session.Query<T>().FirstOrDefaultAsync(predicate, cancellationToken);
    }
    
    public async Task<bool> AnyAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await Session.Query<T>().AnyAsync(predicate, cancellationToken);
    }
    
    public async Task<int> CountAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await Session.Query<T>().CountAsync(predicate, cancellationToken);
    }
    
    public async Task<List<T>> ToListAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await Session.Query<T>().Where(predicate).ToListAsync(cancellationToken);
    }
}
```

## ?? Exemplo: MongoDB

### Antes (v9.x)

```csharp
public class MongoRepository<T> : IRepository<T> where T : class, IEntity
{
    public async Task<T> GetAsync(Expression<Func<T, bool>> predicate)
    {
        return await Collection.Find(predicate).SingleOrDefaultAsync();
    }
}
```

### Depois (v10.0)

```csharp
public class MongoRepository<T> : IRepository<T> where T : class, IEntity
{
    // Manter método original
    public async Task<T> GetAsync(Expression<Func<T, bool>> predicate)
    {
        return await GetAsync(predicate, CancellationToken.None);
    }
    
    // Nova sobrecarga com CancellationToken
    public async Task<T> GetAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken)
    {
        return await Collection.Find(predicate).SingleOrDefaultAsync(cancellationToken);
    }
    
    // Novos métodos auxiliares
    public async Task<T> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await Collection.Find(predicate).FirstOrDefaultAsync(cancellationToken);
    }
    
    public async Task<bool> AnyAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await Collection.Find(predicate).AnyAsync(cancellationToken);
    }
    
    public async Task<int> CountAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return (int)await Collection.CountDocumentsAsync(predicate, cancellationToken: cancellationToken);
    }
    
    public async Task<List<T>> ToListAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await Collection.Find(predicate).ToListAsync(cancellationToken);
    }
}
```

## ?? Checklist de Atualização

### IRepository\<T\>

- [ ] Adicionar sobrecargas com `CancellationToken` em todos métodos async existentes
- [ ] Implementar `FirstOrDefaultAsync`
- [ ] Implementar `AnyAsync`
- [ ] Implementar `CountAsync`
- [ ] Implementar `ToListAsync`
- [ ] Redirecionar métodos antigos para novos (passando `CancellationToken.None`)

### IUnitOfWork

- [ ] Implementar `BeginTransactionAsync(CancellationToken)`
- [ ] Implementar `BeginTransactionAsync(IsolationLevel, CancellationToken)`
- [ ] Implementar `CommitAsync(CancellationToken)`
- [ ] Implementar `RollbackAsync(CancellationToken)`
- [ ] Implementar `InTransactionAsync<T>(Func<Task<T>>, CancellationToken)`
- [ ] Implementar `IAsyncDisposable.DisposeAsync()`

### Testes

- [ ] Testar todos os novos métodos
- [ ] Testar cancelamento com `CancellationToken`
- [ ] Testar transações async
- [ ] Verificar retrocompatibilidade

## ?? Referência: Entity Framework Core

O `Codout.Framework.EF` já está completamente atualizado. Use-o como referência:

- **Arquivo**: `Codout.Framework.EF\EFRepository.cs`
- **Arquivo**: `Codout.Framework.EF\EFUnitOfWork.cs`

## ?? Dicas

1. **Não quebre retrocompatibilidade**: Mantenha os métodos antigos chamando os novos
2. **Use `CancellationToken.None`**: Como padrão para métodos sem parâmetro
3. **Teste cancelamento**: Verifique se `CancellationToken` realmente cancela
4. **Documente**: Adicione XML comments nos novos métodos

## ?? Suporte

Se encontrar dificuldades:

1. Consulte a implementação do `Codout.Framework.EF`
2. Revise a documentação no `Codout.Framework.Data\README.md`
3. Contate a equipe de arquitetura

---

**Versão**: 10.0.0  
**Status**: Aguardando atualização de NH e Mongo  
**Prioridade**: Alta
