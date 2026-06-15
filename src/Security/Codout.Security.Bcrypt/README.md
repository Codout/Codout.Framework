# Codout.Security.Bcrypt

Implementação de `IPasswordHasher` (de [Codout.Security.Core](https://www.nuget.org/packages/Codout.Security.Core)) usando Bcrypt via [BCrypt.Net-Next](https://www.nuget.org/packages/BCrypt.Net-Next).

## Instalação

```bash
dotnet add package Codout.Security.Bcrypt
```

## Uso

Registre o algoritmo no DI com o método de extensão `UseBcrypt` (de `BcryptExtensions`), encadeado ao builder do Core:

```csharp
using Codout.Security.Bcrypt;
using Codout.Security.Core;

builder.Services
    .UpgradePasswordSecurity()
    .UseBcrypt();
```

O custo do Bcrypt é controlado por `BcryptOptions` (e não pelo `WithStrength` do builder): `WorkFactor` é o log2 do número de rounds (faixa válida 4–31, padrão `12`) e `SaltRevision` escolhe a revisão do salt (enum `BcryptSaltRevision`: `Revision2`, `Revision2A`, `Revision2B`, `Revision2X`, `Revision2Y`; padrão `Revision2B`):

```csharp
builder.Services
    .UpgradePasswordSecurity()
    .UseBcrypt(options =>
    {
        options.WorkFactor = 14;
        options.SaltRevision = BcryptSaltRevision.Revision2B;
    });
```

A implementação registrada é `BcryptPasswordHash` (scoped). `VerifyHashedPassword` retorna `PasswordVerificationResult.SuccessRehashNeeded` quando o hash armazenado usa `WorkFactor` menor que o configurado, permitindo upgrade transparente dos hashes existentes.

## Pacotes relacionados

- [Codout.Security.Core](https://www.nuget.org/packages/Codout.Security.Core) — abstrações e builder
- [Codout.Security.Argon2](https://www.nuget.org/packages/Codout.Security.Argon2) — Argon2id via libsodium
- [Codout.Security.Scrypt](https://www.nuget.org/packages/Codout.Security.Scrypt) — Scrypt via libsodium

---
Parte do [Codout.Framework](https://github.com/Codout/Codout.Framework) — licença MIT.
