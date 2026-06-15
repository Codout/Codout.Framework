# Codout.DynamicLinq

Wrapper para datasource de lista paginada: aplica paginação, ordenação, filtragem, agrupamento e agregações sobre qualquer `IQueryable<T>` usando Dynamic LINQ, retornando um resultado pronto para grids (ex.: Kendo UI DataSource).

## Instalação

```bash
dotnet add package Codout.DynamicLinq
```

## Uso

O ponto de entrada é o método de extensão `ToDataSourceResult` (classe `QueryableExtensions`), que recebe um `DataSourceRequest` (com `Take`, `Skip`, `Sort`, `Filter`, `Group` e `Aggregate`) e devolve um `DataSourceResult` com `Data`, `Total`, `Groups`, `Aggregates` e `Errors`:

```csharp
using Codout.DynamicLinq;

[HttpPost]
public DataSourceResult GetPedidos([FromBody] DataSourceRequest request)
{
    IQueryable<Pedido> query = _db.Pedidos.AsQueryable();
    return query.ToDataSourceResult(request);
}
```

Também é possível chamar o overload completo manualmente, informando `take`, `skip`, ordenação (`Sort`), filtro (`Filter`), agregações (`Aggregator`) e agrupamentos (`Group`):

```csharp
using Codout.DynamicLinq;

var result = _db.Pedidos.AsQueryable().ToDataSourceResult(
    take: 10,
    skip: 0,
    sort: new[] { new Sort { Field = "Data", Dir = "desc" } },
    filter: new Filter
    {
        Logic = "and",
        Filters = new[]
        {
            new Filter { Field = "Cliente", Operator = "contains", Value = "Maria" }
        }
    },
    aggregates: Array.Empty<Aggregator>(),
    group: Array.Empty<Group>());

var pagina = result.Data;   // página atual
var total = result.Total;   // total de registros (antes da paginação)
```

Observação: forneça coleções vazias (e não `null`) em `Sort`, `Group` e `Aggregate` quando não utilizá-los. Filtros sobre campos `DateTime` e `decimal` recebem conversão automática de tipo, e o operador `eq` em datas sem hora é expandido para o intervalo do dia.

## Pacotes relacionados

- [Codout.Framework.EF](https://www.nuget.org/packages/Codout.Framework.EF) — repositórios com Entity Framework Core; o `IQueryable` retornado pode ser consumido diretamente pelo `ToDataSourceResult`.
- [Codout.Framework.Data](https://www.nuget.org/packages/Codout.Framework.Data) — abstrações de repositório e Unit of Work usadas em conjunto nas consultas.

---
Parte do [Codout.Framework](https://github.com/Codout/Codout.Framework) — licença MIT.
