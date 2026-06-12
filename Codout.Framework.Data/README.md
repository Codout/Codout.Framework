# Codout.Framework.Data

Abstrações de acesso a dados para .NET: repositório genérico (`IRepository<T>`), unidade de trabalho (`IUnitOfWork`), entidades (`IEntity`, `IEntity<TId>`), especificações (`ISpecification<T>`) e auditoria (`IAuditable`, `ISoftDeletable`), independentes do provedor de persistência.

## Instalação

```bash
dotnet add package Codout.Framework.Data
```

## Uso

Defina suas entidades implementando `IEntity<TId>` e dependa apenas dos contratos:

```csharp
using Codout.Framework.Data;
using Codout.Framework.Data.Repository;

public class ClienteService(IRepository<Cliente> repository, IUnitOfWork unitOfWork)
{
    public async Task<Cliente> CriarAsync(string nome, CancellationToken ct)
    {
        var cliente = await repository.SaveAsync(new Cliente { Nome = nome }, ct);
        await unitOfWork.CommitAsync(ct);
        return cliente;
    }

    public Task<List<Cliente>> BuscarAsync(string nome, CancellationToken ct) =>
        repository.ToListAsync(c => c.Nome.Contains(nome), ct);
}
```

`IRepository<T>` expõe consultas (`All`, `AllReadOnly`, `Where`, `WherePaged`, `Get`, `FirstOrDefaultAsync`, `AnyAsync`, `CountAsync`) e comandos (`Save`, `SaveOrUpdate`, `Update`, `Delete`, `Merge`, `Refresh`) em versões síncronas e assíncronas com `CancellationToken`. `IUnitOfWork` oferece `BeginTransaction`, `Commit`, `Rollback` e `InTransactionAsync`.

## Pacotes relacionados

- `Codout.Framework.EF` — implementação para Entity Framework Core.
- `Codout.Framework.NH` — implementação para NHibernate.
- `Codout.Framework.Mongo` — implementação para MongoDB.

Parte do [Codout.Framework](https://github.com/Codout/Codout.Framework) — licença MIT.
