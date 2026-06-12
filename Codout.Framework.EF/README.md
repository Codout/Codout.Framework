# Codout.Framework.EF

Implementação dos contratos `IRepository<T>` e `IUnitOfWork` de `Codout.Framework.Data` para Entity Framework Core, com builder fluente de configuração, interceptors de auditoria e soft delete.

## Instalação

```bash
dotnet add package Codout.Framework.EF
```

## Uso

Registre o `DbContext` com o builder fluente `AddEFCore<TContext>`:

```csharp
using Codout.Framework.EF;

builder.Services
    .AddEFCore<MeuDbContext>(builder.Configuration)
    .WithConnectionStringFromConfiguration("DefaultConnection")
    .UseSqlServer()
    .EnableRetryOnFailure()
    .Build();
```

Use `EFRepository<T>` e uma especialização de `EFUnitOfWork<T>` (classe abstrata):

```csharp
public class MeuUnitOfWork(MeuDbContext context) : EFUnitOfWork<MeuDbContext>(context);

var repository = new EFRepository<Cliente>(context);
var cliente = await repository.SaveAsync(new Cliente { Nome = "Maria" }, ct);
await unitOfWork.CommitAsync(ct); // chama SaveChanges e confirma a transação
```

`EFRepository<T>` recebe um `DbContext` no construtor e expõe `All`, `AllReadOnly` (AsNoTracking), `Where`, `WherePaged`, `Get`, `SaveAsync`, `SaveOrUpdateAsync`, `UpdateAsync`, `DeleteAsync` e `IncludeMany`.

## Pacotes relacionados

- `Codout.Framework.Data` — contratos implementados por este pacote.
- `Codout.Framework.NH` e `Codout.Framework.Mongo` — implementações alternativas dos mesmos contratos.

Parte do [Codout.Framework](https://github.com/Codout/Codout.Framework) — licença MIT.
