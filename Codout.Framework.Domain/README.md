# Codout.Framework.Domain

Camada de domínio do Codout.Framework: classes base para entidades, value objects e contratos de auditoria/soft delete, com `Equals`/`GetHashCode` baseados em identidade ou em assinatura de domínio.

## Instalação

```bash
dotnet add package Codout.Framework.Domain
```

## Uso

Declare uma entidade herdando de `Entity<TId>` (o `Id` é `virtual` e o setter é `protected`; use `SetId` quando precisar atribuir manualmente). Para o caso comum de PK `Guid?`, herde de `EntityBase`; para Guid gerado pela aplicação no construtor, use `ClientGeneratedEntity`; para campos de auditoria (`CreatedAt`, `UpdatedAt`, `CreatedBy`, `UpdatedBy`), use `AuditEntityBase` ou `AuditEntity<TId>`:

```csharp
using Codout.Framework.Domain.Base;
using Codout.Framework.Domain.Entities;

public class Cliente : EntityBase // Entity<Guid?>
{
    [DomainSignature] // participa do Equals/GetHashCode enquanto a entidade é transient
    public virtual string Documento { get; set; }

    public virtual string Nome { get; set; }
}

public class Pedido : AuditEntityBase // EntityBase + IAudit
{
    public virtual decimal Total { get; set; }
}
```

Value objects herdam de `ValueObject` e comparam por todas as propriedades (não use `[DomainSignature]` neles — lança `InvalidOperationException`):

```csharp
using Codout.Framework.Domain.Base;

public class Endereco : ValueObject
{
    public string Logradouro { get; set; }
    public string Cidade { get; set; }
    public string Cep { get; set; }
}
```

As interfaces `IAudit`, `ISoftDeletable` (`DeletedAt`) e `ISequence` (`Code`) ficam em `Codout.Framework.Domain.Interfaces` e são reconhecidas pelas implementações de persistência do framework.

## Pacotes relacionados

- [Codout.Framework.Data](https://www.nuget.org/packages/Codout.Framework.Data) — abstrações `IRepository<T>` / `IUnitOfWork` (dependência deste pacote).
- [Codout.Framework.EF](https://www.nuget.org/packages/Codout.Framework.EF) — persistência com Entity Framework Core.
- [Codout.Framework.NH](https://www.nuget.org/packages/Codout.Framework.NH) — persistência com NHibernate.
- [Codout.Framework.Mongo](https://www.nuget.org/packages/Codout.Framework.Mongo) — persistência com MongoDB.
- [Codout.Framework.Application](https://www.nuget.org/packages/Codout.Framework.Application) — camada de aplicação/serviços.

---
Parte do [Codout.Framework](https://github.com/Codout/Codout.Framework) — licença MIT.
