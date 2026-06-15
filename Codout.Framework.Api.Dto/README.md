# Codout.Framework.Api.Dto

Tipos base para classes DTO (Data Transfer Objects) trafegadas entre as APIs do Codout.Framework e seus clientes.

## Instalação

```bash
dotnet add package Codout.Framework.Api.Dto
```

## Uso

O pacote fornece os contratos `IDto` e `IEntityDto<TId>` e as implementações base `Dto` e `EntityDto<TId>` (esta última expõe a propriedade `Id`). Observação: por razões históricas, os tipos residem no namespace `Codout.Framework.Api.Client`.

```csharp
using Codout.Framework.Api.Client;

public class ClienteDto : EntityDto<Guid>
{
    public string Nome { get; set; }
    public string Email { get; set; }
}
```

DTOs sem identidade podem derivar de `Dto` ou implementar `IDto` diretamente:

```csharp
using Codout.Framework.Api.Client;

public class ResumoVendasDto : Dto
{
    public decimal Total { get; set; }
    public int Quantidade { get; set; }
}
```

Esses mesmos tipos são usados como restrição genérica em `RestApiEntityBase<TEntity, TDto, TId>` (servidor), `RestApiClient<T, TId>` (cliente) e `CrudAppServiceBase<TEntity, TDto, TId>` (application service), garantindo um contrato único de transporte. O pacote tem target `netstandard2.1` e não possui dependências externas.

## Pacotes relacionados

- [Codout.Framework.Api](https://www.nuget.org/packages/Codout.Framework.Api) — controllers REST que recebem/retornam estes DTOs.
- [Codout.Framework.Api.Client](https://www.nuget.org/packages/Codout.Framework.Api.Client) — cliente HTTP tipado que consome APIs usando estes DTOs.
- [Codout.Framework.Application](https://www.nuget.org/packages/Codout.Framework.Application) — serviços de aplicação que mapeiam entidades para estes DTOs.

---
Parte do [Codout.Framework](https://github.com/Codout/Codout.Framework) — licença MIT.
