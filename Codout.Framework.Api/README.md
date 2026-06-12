# Codout.Framework.Api

Biblioteca abstrata para construção de Web APIs RESTful CRUD em ASP.NET Core, com controller base genérico plugado na camada de Application Service do Codout.Framework.

## Instalação

```bash
dotnet add package Codout.Framework.Api
```

## Uso

Herde de `RestApiEntityBase<TEntity, TDto, TId>` para expor um CRUD completo (`Get`, `Post`, `Put`, `Delete` e `GetAll` paginado via `DataSourceRequest`) implementando o contrato `IRestApi<TDto, TId>`:

```csharp
using Codout.Framework.Api;
using Codout.Framework.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/clientes")]
public class ClientesController(ICrudAppService<Cliente, ClienteDto, Guid> appService)
    : RestApiEntityBase<Cliente, ClienteDto, Guid>(appService);
```

Registre o middleware de tratamento de exceções (`ApiExceptionMiddleware`), que serializa erros como `ApiException` / `ApiErrorMessage` em JSON:

```csharp
using Codout.Framework.Api.Middleware;

var app = builder.Build();

app.ConfigureExceptionMiddleware();

app.MapControllers();
app.Run();
```

Todos os endpoints são `virtual` e podem ser sobrescritos no controller concreto.

## Pacotes relacionados

- [Codout.Framework.Api.Client](https://www.nuget.org/packages/Codout.Framework.Api.Client) — cliente HTTP tipado para consumir APIs construídas com este pacote.
- [Codout.Framework.Api.Dto](https://www.nuget.org/packages/Codout.Framework.Api.Dto) — DTOs base (`EntityDto<TId>`) compartilhados entre servidor e cliente.
- [Codout.Framework.Application](https://www.nuget.org/packages/Codout.Framework.Application) — camada Application Service (`ICrudAppService`) consumida pelos controllers.
- [Codout.Framework.Domain](https://www.nuget.org/packages/Codout.Framework.Domain) — entidades base de domínio (`Entity<TId>`).

---
Parte do [Codout.Framework](https://github.com/Codout/Codout.Framework) — licença MIT.
