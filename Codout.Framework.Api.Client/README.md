# Codout.Framework.Api.Client

Cliente HTTP genérico e tipado para consumir as Web APIs CRUD construídas com Codout.Framework.Api, incluindo tratamento de erros padronizado.

## Instalação

```bash
dotnet add package Codout.Framework.Api.Client
```

## Uso

`RestApiClient<T, TId>` implementa `IRestApi<TDto, TId>` (`GetAsync`, `PostAsync`, `PutAsync`, `DeleteAsync`, `GetAllAsync`) sobre um `HttpClient` configurado pela classe base `ApiClientBase`:

```csharp
using Codout.DynamicLinq;
using Codout.Framework.Api.Client;

public class ClienteDto : EntityDtoBase<Guid>
{
    public string Nome { get; set; }
}

var api = new RestApiClient<ClienteDto, Guid>("api/clientes", "https://api.exemplo.com/");
// Ou, com autenticação via header "ApiKey":
// var api = new RestApiClient<ClienteDto, Guid>("api/clientes", "https://api.exemplo.com/", apiKey);

var criado = await api.PostAsync(new ClienteDto { Nome = "Maria" });
var cliente = await api.GetAsync(criado.Id);

DataSourceResult pagina = await api.GetAllAsync(new DataSourceRequest { Take = 20, Skip = 0 });
await api.DeleteAsync(criado.Id);
```

Falhas HTTP são convertidas em `ApiClientException`, que carrega o `ApiException` retornado pelo servidor:

```csharp
try
{
    await api.GetAsync(id);
}
catch (ApiClientException ex)
{
    Console.WriteLine($"{ex.ApiException.StatusCode}: {ex.ApiException.Message}");
}
```

## Pacotes relacionados

- [Codout.Framework.Api](https://www.nuget.org/packages/Codout.Framework.Api) — lado servidor: controllers REST que este cliente consome.
- [Codout.Framework.Api.Dto](https://www.nuget.org/packages/Codout.Framework.Api.Dto) — DTOs base compartilhados entre cliente e servidor.

---
Parte do [Codout.Framework](https://github.com/Codout/Codout.Framework) — licença MIT.
