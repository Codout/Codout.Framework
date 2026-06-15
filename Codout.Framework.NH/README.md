# Codout.Framework.NH

Implementação dos contratos `IRepository<T>` e `IUnitOfWork` de `Codout.Framework.Data` para NHibernate, com registro de `ISessionFactory` e `ISession` no container de DI via FluentNHibernate.

## Instalação

```bash
dotnet add package Codout.Framework.NH
```

## Uso

Registre os serviços com `AddNHibernateServices`, que configura `ISessionFactory` (singleton), `ISession` e `IStatelessSession` (scoped) e o ciclo de vida da fábrica:

```csharp
using Codout.Framework.NH;

builder.Services.AddNHibernateServices(builder.Configuration);
```

Os assemblies de mapeamento são lidos da seção de configuração `NHibernate:MappingAssemblies` (array de nomes de assembly).

Use `NHRepository<T>`, que recebe um `ISession` no construtor:

```csharp
using Codout.Framework.NH;

var repository = new NHRepository<Cliente>(session); // ISession
var cliente = await repository.SaveAsync(new Cliente { Nome = "Maria" }, ct);
var ativos = await repository.ToListAsync(c => c.Ativo, ct);
```

`NHUnitOfWork` implementa `IUnitOfWork` sobre a sessão, com `BeginTransaction`, `CommitAsync`, `RollbackAsync` e `InTransactionAsync`.

## Pacotes relacionados

- `Codout.Framework.Data` — contratos implementados por este pacote.
- `Codout.Framework.EF` e `Codout.Framework.Mongo` — implementações alternativas dos mesmos contratos.

Parte do [Codout.Framework](https://github.com/Codout/Codout.Framework) — licença MIT.
