# Codout.Framework.Application

Implementação base da camada Application Service do Codout.Framework: serviços CRUD genéricos que orquestram `IRepository<TEntity>` / `IUnitOfWork` e mapeiam entidades para DTOs com AutoMapper.

## Instalação

```bash
dotnet add package Codout.Framework.Application
```

## Uso

Registre os serviços no container (registra `ICrudAppService<,,>` como `CrudAppServiceBase<,,>` e o AutoMapper com o `MappingProfile` base, que mapeia `Entity<TId>` ⇄ `EntityDto<TId>`):

```csharp
using Codout.Framework.Application;

builder.Services.AddCrudAppServices();
```

Herde de `CrudAppServiceBase<TEntity, TDto, TId>` para customizar um serviço. A classe implementa `ICrudAppService<TEntity, TDto, TId>` com `GetAsync`, `SaveAsync`, `UpdateAsync`, `DeleteAsync` e `GetAllAsync(DataSourceRequest)` — todos `virtual`:

```csharp
using AutoMapper;
using Codout.Framework.Application;
using Codout.Framework.Data;
using Codout.Framework.Data.Repository;

public class ClienteAppService(
    IUnitOfWork unitOfWork,
    IRepository<Cliente> repository,
    IMapper mapper)
    : CrudAppServiceBase<Cliente, ClienteDto, Guid>(unitOfWork, repository, mapper)
{
    public override async Task<ClienteDto> SaveAsync(ClienteDto input)
    {
        // validações de negócio aqui
        return await base.SaveAsync(input);
    }
}
```

`AppServiceBase<TEntity>` expõe `UnitOfWork`, `Repository` e `Mapper` para serviços que não seguem o padrão CRUD.

## Pacotes relacionados

- [Codout.Framework.Data](https://www.nuget.org/packages/Codout.Framework.Data) — abstrações `IRepository<T>` e `IUnitOfWork` consumidas pelos serviços.
- [Codout.Framework.Domain](https://www.nuget.org/packages/Codout.Framework.Domain) — entidades base (`Entity<TId>`).
- [Codout.Framework.Api](https://www.nuget.org/packages/Codout.Framework.Api) — controllers REST que consomem `ICrudAppService`.
- Persistência: [Codout.Framework.EF](https://www.nuget.org/packages/Codout.Framework.EF), [Codout.Framework.NH](https://www.nuget.org/packages/Codout.Framework.NH), [Codout.Framework.Mongo](https://www.nuget.org/packages/Codout.Framework.Mongo).

---
Parte do [Codout.Framework](https://github.com/Codout/Codout.Framework) — licença MIT.
