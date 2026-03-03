# Guia de Migração - Codout.Framework.EF v10.0

## ?? Objetivo

Este guia ajudará você a migrar código existente para a nova versão v10.0 do Codout.Framework.EF com melhorias enterprise.

## ?? Sumário de Mudanças

### ? Compatibilidade Mantida
- ? Interface `IRepository<T>` não foi alterada
- ? Interface `IUnitOfWork` não foi alterada (apenas adicionados métodos async)
- ? `EFRepository<T>` mantém todas as funcionalidades anteriores
- ? `EFUnitOfWork<T>` mantém API existente

### ? Melhorias Críticas
- ?? **Bug crítico corrigido**: Transações no UnitOfWork agora funcionam corretamente
- ?? **Memory leak resolvido**: Repository não faz mais dispose do DbContext
- ? **Performance**: Melhorias em queries read-only

### ?? Novos Recursos
- ? Specification Pattern
- ? Interceptors (Auditoria e Soft Delete)
- ? Builder fluente para configuração
- ? Métodos async adicionais no UnitOfWork
- ? Extensões com CancellationToken

## ?? Mudanças Necessárias

### 1. Configuração do DbContext

#### ? Antes (ainda funciona, mas deprecated)

```csharp
services.AddEFCore<MyDbContext>(configuration);
```

#### ? Depois (recomendado)

```csharp
services.AddEFCore<MyDbContext>(configuration)
    .WithConnectionStringFromConfiguration("DefaultConnection")
    .UseSqlServer()
    .Build();
```

**Motivo**: Maior flexibilidade e suporte a configurações avançadas.

**Ação**: Atualizar na primeira oportunidade. O método antigo continuará funcionando mas mostrará warning.

---

### 2. Unit of Work - Transações

#### ? Antes (bug crítico)

```csharp
// Commit() criava transação automaticamente se não existisse
// e engolia exceções silenciosamente
uow.Commit(); // ?? COMPORTAMENTO INCORRETO
```

#### ? Depois (correto)

```csharp
// Agora você DEVE criar a transação explicitamente
await uow.BeginTransactionAsync();
try
{
    // operações...
    await uow.CommitAsync();
}
catch
{
    await uow.RollbackAsync();
    throw;
}
```

**Motivo**: Prevenir commits acidentais e melhorar controle de transações.

**Ação**: **CRÍTICO** - Revisar TODO código que usa `Commit()` sem `BeginTransaction()` explícito.

---

### 3. InTransaction - Nova Assinatura Async

#### ? Antes

```csharp
var result = uow.InTransaction(() =>
{
    // operação síncrona
    return entity;
});
```

#### ? Depois (agora com suporte async)

```csharp
var result = await uow.InTransactionAsync(async () =>
{
    // operação assíncrona
    return entity;
});
```

**Motivo**: Suporte completo a async/await.

**Ação**: Considerar migrar para versão async para melhor performance.

---

### 4. CancellationToken Support

#### ? Antes

```csharp
var products = await repository.GetAsync(p => p.IsActive);
// Sem suporte a cancelamento
```

#### ? Depois (com extensões)

```csharp
using Codout.Framework.EF; // Para usar extensões

var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
var products = await repository.GetAsync(p => p.IsActive, cts.Token);
```

**Motivo**: Permitir cancelamento de operações longas.

**Ação**: Opcional, mas recomendado para APIs web e operações longas.

---

## ?? Adotando Novos Recursos

### Specification Pattern

**Quando usar**: Queries complexas reutilizáveis em múltiplos lugares.

```csharp
// Criar specification
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

// Usar
var spec = new ActiveProductsSpec();
var products = await repository.ListAsync(spec);
```

**Benefício**: Queries complexas testáveis e reutilizáveis.

---

### Auditoria Automática

**Quando usar**: Entidades que precisam rastrear quem criou/modificou.

```csharp
// 1. Implementar interface
using Codout.Framework.EF.Interceptors;

public class Product : IEntity, IAuditable
{
    public int Id { get; set; }
    // ... outras props
    
    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
}

// 2. Configurar
services.AddEFCore<MyDbContext>(configuration)
    .WithConnectionStringFromConfiguration()
    .UseSqlServer()
    .EnableAuditing() // ? Adicione esta linha
    .Build();

// 3. Implementar provider
services.AddSingleton<ICurrentUserProvider, HttpContextUserProvider>();
```

**Benefício**: Auditoria automática sem código manual.

---

### Soft Delete Automático

**Quando usar**: Nunca deletar dados fisicamente.

```csharp
// 1. Implementar interface
using Codout.Framework.EF.Interceptors;

public class Product : IEntity, ISoftDeletable
{
    public int Id { get; set; }
    // ... outras props
    
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }
}

// 2. Configurar
services.AddEFCore<MyDbContext>(configuration)
    .WithConnectionStringFromConfiguration()
    .UseSqlServer()
    .EnableSoftDelete() // ? Adicione esta linha
    .Build();

// 3. Adicionar query filter no DbContext
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.Entity<Product>()
        .HasQueryFilter(p => !p.IsDeleted);
}
```

**Benefício**: Soft delete automático + histórico preservado.

---

## ?? Checklist de Migração

### Prioridade ALTA (Bugs Críticos)

- [ ] **Revisar todas chamadas a `Commit()` sem `BeginTransaction()`**
  - Arquivo: Buscar por `\.Commit\(\)` em todo projeto
  - Ação: Adicionar `BeginTransaction()` antes ou usar `InTransaction()`

- [ ] **Verificar dispose de DbContext em repositórios customizados**
  - Se você sobrescreveu `Dispose()`, remova o dispose do DbContext
  
### Prioridade MÉDIA (Melhorias)

- [ ] Atualizar configuração do DbContext para usar builder fluente
- [ ] Adicionar suporte a CancellationToken em operações críticas
- [ ] Migrar para métodos async do UnitOfWork (`BeginTransactionAsync`, `CommitAsync`)

### Prioridade BAIXA (Novos Recursos)

- [ ] Avaliar uso de Specification Pattern para queries complexas
- [ ] Implementar IAuditable em entidades importantes
- [ ] Implementar ISoftDeletable em vez de delete físico
- [ ] Configurar retry policies para resiliência
- [ ] Adicionar telemetria e health checks

---

## ?? Como Encontrar Código Afetado

### 1. Buscar transações sem BeginTransaction

```regex
\.Commit\(\)(?!.*BeginTransaction)
```

### 2. Buscar uso de InTransaction síncrono

```regex
\.InTransaction\((?!.*async)
```

### 3. Buscar repositórios com dispose customizado

```regex
class.*Repository.*\n.*Dispose\(
```

---

## ?? Avisos de Breaking Changes

### Nenhum Breaking Change Real

A versão v10.0 foi projetada para ser **100% retrocompatível**. Todos os "breaking changes" são na verdade:

1. **Correções de bugs** que já deveriam funcionar assim
2. **Deprecation warnings** para métodos legados
3. **Melhorias de design** que não quebram código existente

### Deprecations (Warnings)

```csharp
// Gera CS0618 warning (ainda compila e funciona)
services.AddEFCore<MyDbContext>(configuration);

// Suprimir warning temporariamente
#pragma warning disable CS0618
services.AddEFCore<MyDbContext>(configuration);
#pragma warning restore CS0618
```

---

## ?? Recursos Adicionais

- [README.md](README.md) - Documentação completa
- [Interceptors](Interceptors/) - Código dos interceptors
- [Specifications](Specifications/) - Código do Specification Pattern

---

## ?? Suporte

Se encontrar problemas durante a migração:

1. Verifique se seguiu todos os passos do checklist
2. Revise a documentação no README.md
3. Consulte a equipe de arquitetura da Codout

---

## ?? Impacto Estimado

| Projeto | Esforço de Migração | Risco |
|---------|-------------------|-------|
| Pequeno (<10 entidades) | 1-2 horas | Baixo |
| Médio (10-50 entidades) | 4-8 horas | Médio |
| Grande (>50 entidades) | 2-3 dias | Médio |

**Nota**: O maior esforço é na revisão de transações. O resto é opcional.

---

## ? Exemplo Completo de Migração

### Antes

```csharp
// Startup.cs
services.AddEFCore<MyDbContext>(configuration);

// ProductService.cs
public void CreateProduct(Product product)
{
    repository.Save(product);
    uow.Commit(); // ?? Bug: não chamou BeginTransaction
}
```

### Depois

```csharp
// Startup.cs
services.AddEFCore<MyDbContext>(configuration)
    .WithConnectionStringFromConfiguration()
    .UseSqlServer()
    .EnableAuditing()
    .EnableSoftDelete()
    .EnableRetryOnFailure()
    .Build();

services.AddSingleton<ICurrentUserProvider, HttpContextUserProvider>();

// Product.cs
public class Product : IEntity, IAuditable, ISoftDeletable
{
    public int Id { get; set; }
    public string Name { get; set; }
    
    // IAuditable
    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
    
    // ISoftDeletable
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }
    
    public bool IsTransient() => Id == 0;
}

// ProductService.cs
public async Task CreateProductAsync(Product product, CancellationToken ct = default)
{
    await uow.InTransactionAsync(async () =>
    {
        await repository.SaveAsync(product);
        return product;
    }, ct);
}
```

---

**Data**: Janeiro 2025  
**Versão**: v10.0.0  
**Status**: Estável para produção
