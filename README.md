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
    <a href="https://github.com/Codout/Codout.Framework/actions/workflows/build.yml">
      <img src="https://img.shields.io/github/actions/workflow/status/Codout/Codout.Framework/.github/workflows/dotnet.yml?label=build&logo=github&style=flat-square" alt="Build Status">
    </a>
    <a href="https://www.nuget.org/packages/Codout.Framework.Domain">
      <img src="https://img.shields.io/nuget/v/Codout.Framework.Domain?style=flat-square" alt="NuGet Version">
    </a>
    <a href="https://www.nuget.org/packages/Codout.Framework.Domain">
      <img src="https://img.shields.io/nuget/dt/Codout.Framework.Domain?style=flat-square" alt="NuGet Version">
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

O **Codout.Framework** é um *starter kit* de Clean Architecture para aplicações .NET, projetado para fornecer:

* **Modularidade**: Selecione apenas os módulos que você precisa (EF Core, MongoDB, NHibernate, Multi-Tenancy, etc.).
* **Escalabilidade**: Arquitetura preparada para crescer junto com seu produto.
* **Manutenibilidade**: Código limpo, separação de responsabilidades e padrões de projeto consolidados.

Use-o como base para APIs RESTful, microsserviços, backends de aplicações corporativas e mais.

---

## 📦 Estrutura de Módulos

| Projeto               | Responsabilidade                                                               |
| --------------------- | ------------------------------------------------------------------------------ |
| **Domain**            | Entidades, objetos de valor, interfaces de repositório e regras de negócio.    |
| **Common**            | Helpers, extensões, logging, validações e abstrações de uso geral.             |
| **DAL**               | Interfaces de Unit of Work e repositórios gerais.                              |
| **EF**                | Implementação de repositórios com Entity Framework Core.                       |
| **Mongo**             | Implementação de Unit of Work e repositórios para MongoDB.                     |
| **NH**                | Repositórios baseados em NHibernate.                                           |
| **Api**               | Projeto ASP.NET Core com controllers, middleware, autenticação e configuração. |
| **Api.Dto**           | Data Transfer Objects para requests e responses da API.                        |
| **Api.Client**        | Cliente HTTP tipado para consumo de APIs (interna/externa).                    |
| **Kendo.DynamicLinq** | Extensões para consultas dinâmicas LINQ com componentes Kendo UI.              |
| **Multitenancy**      | Suporte para aplicações multi-tenant, resolução de conexão por cliente.        |
| **Mailer**            | Abstrações e implementações de envio de e-mail (AWS SES, SendGrid).            |
| **Zenvia**            | Integração com Zenvia para envio de SMS e notificações.                        |
| **DP**                | Data Processors genéricos para pipelines de transformação de dados.            |
| **Shared**            | Código compartilhado entre .NET Core e .NET Framework Full.                    |
| **UML**               | Diagramas de pacotes e classes para referência arquitetural.                   |
| **Tests**             | Projetos de teste unitário e de integração para cada módulo.                   |

---

## 🚀 Começando Rápido

### Pré-requisitos

* .NET SDK 9.x ou superior
* IDE de sua preferência (Visual Studio, VS Code, Rider)
* (Opcional) Docker para MongoDB ou outros serviços auxiliares

### Passos

1. **Clone o repositório**

   ```bash
   git clone https://github.com/Codout/Codout.Framework.git
   cd Codout.Framework
   ```

2. **Compile e execute testes**

   ```bash
   dotnet build Codout.Framework.sln --configuration Release
   dotnet test tests/**/*.Test.csproj
   ```

3. **Adicione ao seu projeto via NuGet**

   ```powershell
   Install-Package Codout.Framework.Common
   Install-Package Codout.Framework.EF
   Install-Package Codout.Framework.Api
   ```

---

## 🏗️ Exemplo de Uso

```csharp
// Startup.cs (ASP.NET Core)
public void ConfigureServices(IServiceCollection services)
{
    // Configura Domain e Common
    services.AddCodoutDomain();
    services.AddCodoutCommon(Configuration);

    // Opte pela implementação de dados: EF, Mongo ou NH
    services.AddCodoutEf(options => options.UseSqlServer(connectionString));
    // ou
    // services.AddCodoutMongo(options => options.ConnectionString = mongoUri);

    // API
    services.AddCodoutApi();
}
```

---

## 🎨 Roadmap

* [x] Domain & Common Essentials
* [x] EF Core & Migrations
* [x] MongoDB e replicação automática
* [x] NHibernate + FluentMigrator
* [ ] Suporte a gRPC e mensageria (RabbitMQ, Kafka)
* [ ] Dashboard de monitoramento e métricas

Contribuições e sugestões são muito bem-vindas!

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
  Desenvolvido com ❤️ por **Codout**
</p>
