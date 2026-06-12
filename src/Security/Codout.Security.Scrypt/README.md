# Codout.Security.Scrypt

Implementação de `IPasswordHasher` (de [Codout.Security.Core](https://www.nuget.org/packages/Codout.Security.Core)) usando Scrypt via [Sodium.Core](https://www.nuget.org/packages/Sodium.Core) (libsodium).

## Instalação

```bash
dotnet add package Codout.Security.Scrypt
```

## Uso

Registre o algoritmo no DI com o método de extensão `UseScrypt` (de `ScryptExtensions`), encadeado ao builder do Core. A força (`PasswordHasherStrength.Interactive`, `Moderate` ou `Sensitive`) controla CPU e memória usados pelo Scrypt:

```csharp
using Codout.Security.Core;
using Codout.Security.Scrypt;

builder.Services
    .UpgradePasswordSecurity()
    .WithStrength(PasswordHasherStrength.Moderate)
    .UseScrypt();
```

Para controle fino, configure `ScryptOptions`. Quando `OpsLimit` e `MemLimit` (em bytes) são definidos juntos, eles sobrepõem o `Strength`:

```csharp
builder.Services
    .UpgradePasswordSecurity()
    .UseScrypt(options =>
    {
        options.OpsLimit = 524288;             // custo de CPU
        options.MemLimit = 128 * 1024 * 1024;  // 128 MiB de RAM
    });
```

A implementação registrada é `ScryptPasswordHash` (scoped). `HashPassword` gera um hash no formato `$7$...` (escrypt/libsodium); `VerifyHashedPassword` retorna `PasswordVerificationResult.SuccessRehashNeeded` quando o hash armazenado foi gerado com parâmetro `N` inferior ao atualmente configurado, permitindo upgrade transparente dos hashes.

## Pacotes relacionados

- [Codout.Security.Core](https://www.nuget.org/packages/Codout.Security.Core) — abstrações e builder
- [Codout.Security.Argon2](https://www.nuget.org/packages/Codout.Security.Argon2) — Argon2id via libsodium
- [Codout.Security.Bcrypt](https://www.nuget.org/packages/Codout.Security.Bcrypt) — Bcrypt via BCrypt.Net-Next

---
Parte do [Codout.Framework](https://github.com/Codout/Codout.Framework) — licença MIT.
