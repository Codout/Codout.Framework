# Codout.Security.Argon2

Implementação de `IPasswordHasher` (de [Codout.Security.Core](https://www.nuget.org/packages/Codout.Security.Core)) usando Argon2id via [Sodium.Core](https://www.nuget.org/packages/Sodium.Core) (libsodium).

## Instalação

```bash
dotnet add package Codout.Security.Argon2
```

## Uso

Registre o algoritmo no DI com o método de extensão `UseArgon2` (de `Argon2Extensions`), encadeado ao builder do Core. A força (`PasswordHasherStrength.Interactive`, `Moderate` ou `Sensitive`) controla CPU e memória usados pelo Argon2:

```csharp
using Codout.Security.Argon2;
using Codout.Security.Core;

builder.Services
    .UpgradePasswordSecurity()
    .WithStrength(PasswordHasherStrength.Sensitive)
    .UseArgon2();
```

Para controle fino, configure `Argon2Options`. Quando `OpsLimit` e `MemLimit` (em bytes) são definidos juntos, eles sobrepõem o `Strength`:

```csharp
builder.Services
    .UpgradePasswordSecurity()
    .UseArgon2(options =>
    {
        options.OpsLimit = 4;                  // iterações (custo de CPU)
        options.MemLimit = 256 * 1024 * 1024;  // 256 MiB de RAM
    });
```

A implementação registrada é `ArgonPasswordHash` (scoped). `HashPassword` gera um hash no formato `$argon2id$...`; `VerifyHashedPassword` retorna `PasswordVerificationResult.SuccessRehashNeeded` quando o hash armazenado foi gerado com parâmetros (`m`/`t`) inferiores aos atualmente configurados, permitindo upgrade transparente dos hashes.

## Pacotes relacionados

- [Codout.Security.Core](https://www.nuget.org/packages/Codout.Security.Core) — abstrações e builder
- [Codout.Security.Bcrypt](https://www.nuget.org/packages/Codout.Security.Bcrypt) — Bcrypt via BCrypt.Net-Next
- [Codout.Security.Scrypt](https://www.nuget.org/packages/Codout.Security.Scrypt) — Scrypt via libsodium

---
Parte do [Codout.Framework](https://github.com/Codout/Codout.Framework) — licença MIT.
