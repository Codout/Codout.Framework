<p align="center">
  <a href="https://github.com/Codout/Codout.Framework">
    <img src="https://raw.githubusercontent.com/Codout/Codout.Framework/refs/heads/master/logo-nuget.png" alt="Codout.Framework Logo" width="120">
  </a>
  <h1 align="center">Codout.Framework</h1>
  <p align="center">
    🔧 Framework Clean Architecture modular e escalável para .NET <br>
    🚀 Acelere entregas de projetos com as melhores práticas e uma base pronta para produção
  </p>

  <p align="center">
    <a href="https://github.com/Codout/Codout.Framework/actions/workflows/dotnet.yml">
      <img src="https://img.shields.io/github/actions/workflow/status/Codout/Codout.Framework/dotnet.yml?label=build&logo=github&style=flat-square" alt="Build Status">
    </a>
    <a href="https://www.nuget.org/packages/Codout.Framework.Domain">
      <img src="https://img.shields.io/nuget/v/Codout.Framework.Domain?style=flat-square" alt="NuGet Version">
    </a>
    <a href="https://www.nuget.org/packages/Codout.Framework.Domain">
      <img src="https://img.shields.io/nuget/dt/Codout.Framework.Domain?style=flat-square" alt="NuGet Downloads">
    </a>
    <a href="LICENSE">
      <img src="https://img.shields.io/badge/license-MIT-green.svg?style=flat-square" alt="License">
    </a>
    <a href="https://github.com/Codout/Codout.Framework/issues">
      <img src="https://img.shields.io/github/issues/Codout/Codout.Framework?style=flat-square" alt="Issues">
    </a>
  </p>
</p>

---

## 🎯 Visão Geral

O **Codout.Framework** é um conjunto de pacotes NuGet para aplicações .NET em
Clean Architecture, projetado para fornecer:

* **Modularidade**: instale apenas os pacotes que você precisa (EF Core, MongoDB, NHibernate, Multi-Tenancy, Storage, Mailer, Security, etc.).
* **Escalabilidade**: abstrações de repositório/Unit of Work independentes de ORM.
* **Manutenibilidade**: separação de responsabilidades e padrões consolidados (DDD, Repository, UoW).

Use-o como base para APIs RESTful, microsserviços e backends corporativos.

---

## 📦 Pacotes

### Núcleo

| Pacote | Responsabilidade |
| ------ | ---------------- |
| `Codout.Framework.Domain` | Entidades DDD, value objects, identidade client-generated, auditoria. |
| `Codout.Framework.Data` | Abstrações de `IRepository<T>` / `IUnitOfWork` independentes de ORM. |
| `Codout.Framework.Common` | Extensões, helpers, validações (CPF/CNPJ, e-mail), criptografia utilitária. |

### Persistência (escolha uma implementação)

| Pacote | Responsabilidade |
| ------ | ---------------- |
| `Codout.Framework.EF` | Repositórios e Unit of Work com Entity Framework Core. |
| `Codout.Framework.Mongo` | Repositórios e Unit of Work para MongoDB. |
| `Codout.Framework.NH` | Repositórios e Unit of Work com NHibernate/FluentNHibernate. |

### Camada de API

| Pacote | Responsabilidade |
| ------ | ---------------- |
| `Codout.Framework.Api` | Base de controllers, middleware e configuração ASP.NET Core. |
| `Codout.Framework.Api.Dto` | DTOs de request/response. |
| `Codout.Framework.Api.Client` | Cliente HTTP tipado para consumo de APIs. |
| `Codout.Framework.Application` | Serviços de aplicação (CRUD genérico sobre repositórios). |
| `Codout.DynamicLinq` | Consultas dinâmicas LINQ (filtro/ordenação/paginação/agregação). |

### Transversais

| Pacote | Responsabilidade |
| ------ | ---------------- |
| `Codout.Multitenancy` | Resolução de tenant por request, cache e pipeline per-tenant. (`Softprime.Multitenancy` é o build netstandard2.0 de compatibilidade.) |
| `Codout.Mailer` (+ `.AWS`, `.SendGrid`, `.Razor`) | Abstração de envio de e-mail, dispatchers SES/SendGrid e templates Razor. |
| `Codout.Framework.Storage` (+ `.Azure`) | Abstração de storage de arquivos e implementação Azure Blob. |
| `Codout.Security.Core` (+ `.Argon2`, `.Bcrypt`, `.Scrypt`) | Hash de senhas com upgrade incremental de algoritmo/parâmetros. |
| `Codout.Image.Extensions` | Extração/manipulação de regiões de imagens. |
| `Codout.Framework.Mcp` | Servidor MCP com o conhecimento do framework para agentes de IA. |

---

## 🚀 Começando Rápido

### Pré-requisitos

* .NET SDK 10.x
* IDE de sua preferência (Visual Studio, VS Code, Rider)

### Build e testes do repositório

```bash
git clone https://github.com/Codout/Codout.Framework.git
cd Codout.Framework
dotnet build Codout.Framework.sln --configuration Release
dotnet test Codout.Framework.sln --configuration Release
```

Os projetos de teste vivem em `tests/` e rodam na solution.

### Instalação via NuGet

```bash
dotnet add package Codout.Framework.Domain
dotnet add package Codout.Framework.EF   # ou .Mongo / .NH
```

---

## 🏗️ Exemplo de Uso

```csharp
// Entidade de domínio com Id gerado no cliente
public class Cliente : ClientGeneratedEntity
{
    public string Nome { get; set; } = string.Empty;
}

// Repositório + Unit of Work (implementação EF Core)
public class ClienteService(IRepository<Cliente> repository, IUnitOfWork uow)
{
    public async Task CadastrarAsync(string nome, CancellationToken ct)
    {
        await repository.SaveAsync(new Cliente { Nome = nome }, ct);
        await uow.CommitAsync(ct);
    }
}
```

Cada pacote tem um README próprio com exemplos específicos — veja a página do
pacote no NuGet.org ou a pasta correspondente neste repositório.

---

## 🗺️ Roadmap

O plano de evolução de qualidade do repositório está em [ROADMAP.md](ROADMAP.md).

---

## 🤝 Contribuindo

1. Faça um fork deste repositório
2. Crie uma branch: `git checkout -b feature/nova-funcionalidade`
3. Implemente e escreva testes
4. Abra um Pull Request detalhando sua proposta

Leia o [CONTRIBUTING.md](CONTRIBUTING.md) para mais detalhes.

---

## 📜 Licença

Este projeto está licenciado sob a [MIT License](LICENSE).

---

<p align="center">
  Desenvolvido com ❤️ por <strong>Codout</strong>
</p>
