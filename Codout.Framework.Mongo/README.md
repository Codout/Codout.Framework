# Codout.Framework.Mongo

Implementação dos contratos `IRepository<T>` e `IUnitOfWork` de `Codout.Framework.Data` para MongoDB, sobre o driver oficial `MongoDB.Driver`.

## Instalação

```bash
dotnet add package Codout.Framework.Mongo
```

## Uso

Registre os serviços com `AddMongoDb`, que configura `IMongoClient`, `IMongoDatabase` e `IRepository<T>` no container de DI:

```csharp
using Codout.Framework.Mongo.Configuration;

builder.Services.AddMongoDb(
    connectionString: "mongodb://localhost:27017",
    databaseName: "minha-base");
```

Injete `IRepository<T>` ou instancie `MongoRepository<T>` diretamente:

```csharp
using Codout.Framework.Mongo;

var repository = new MongoRepository<Cliente>(database); // IMongoDatabase
var cliente = await repository.SaveAsync(new Cliente { Nome = "Maria" }, ct);
var ativos = await repository.ToListAsync(c => c.Ativo, ct);
```

Cada entidade é mapeada para uma coleção com o nome do tipo em minúsculas (ex.: `Cliente` -> `cliente`). `MongoUnitOfWork` (recebe `IMongoClient`) dá suporte a transações, que exigem replica set no servidor.

## Pacotes relacionados

- `Codout.Framework.Data` — contratos implementados por este pacote.
- `Codout.Framework.EF` e `Codout.Framework.NH` — implementações alternativas dos mesmos contratos.

Parte do [Codout.Framework](https://github.com/Codout/Codout.Framework) — licença MIT.
