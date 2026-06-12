# Codout.Multitenancy

Módulo de multitenancy para aplicações ASP.NET Core: resolve o tenant por requisição via middleware e expõe `IAppTenant`/`TenantContext` no container de DI com escopo de request.

> **Softprime.Multitenancy**: a mesma pasta gera também o pacote `Softprime.Multitenancy`, um build `netstandard2.0` de compatibilidade com exatamente o mesmo código-fonte (o pacote principal `Codout.Multitenancy` tem target `net10.0`).

## Instalação

```bash
dotnet add package Codout.Multitenancy
```

## Uso

Implemente `ITenantResolver` (ou herde de `MemoryCacheTenantResolver` para ter cache em memória). O tenant resolvido deve implementar `IAppTenant` (`TenantKey`, `DataBaseType`, `ConnectionString`):

```csharp
using Codout.Multitenancy;
using Microsoft.AspNetCore.Http;

public class AppTenant : IAppTenant
{
    public string TenantKey { get; set; }
    public DataBaseType DataBaseType { get; set; }
    public string ConnectionString { get; set; }
}

public class HostTenantResolver : ITenantResolver
{
    public Task<TenantContext> ResolveAsync(HttpContext context)
    {
        var tenant = new AppTenant { TenantKey = context.Request.Host.Host };
        return Task.FromResult(new TenantContext(tenant));
    }
}
```

Registre com `AddMultitenancy<TResolver>()` e ative o middleware com `UseMultitenancy()`:

```csharp
builder.Services.AddMultitenancy<HostTenantResolver>();

var app = builder.Build();
app.UseMultitenancy(); // antes dos endpoints que dependem do tenant
```

A partir daí, injete `IAppTenant` (ou `ITenant<IAppTenant>`/`TenantContext`) em serviços scoped, ou leia direto do request com `HttpContext.GetTenantContext()` / `HttpContext.GetTenant<AppTenant>()`.

## Pacotes relacionados

- [Codout.Framework.Data](https://www.nuget.org/packages/Codout.Framework.Data) — abstrações `IRepository<T>` / `IUnitOfWork`.
- [Codout.Framework.EF](https://www.nuget.org/packages/Codout.Framework.EF) — persistência com Entity Framework Core (connection string por tenant).
- [Codout.Framework.NH](https://www.nuget.org/packages/Codout.Framework.NH) — persistência com NHibernate.
- [Codout.Framework.Application](https://www.nuget.org/packages/Codout.Framework.Application) — camada de aplicação/serviços.

---
Parte do [Codout.Framework](https://github.com/Codout/Codout.Framework) — licença MIT.
