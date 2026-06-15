# Codout.Security.Core

Abstrações para hashing de senhas (`IPasswordHasher`, `IPasswordHashBuilder`) com integração ao container de DI do .NET — o algoritmo concreto (Argon2, Bcrypt ou Scrypt) é plugado por um pacote irmão.

## Instalação

```bash
dotnet add package Codout.Security.Core
```

Este pacote contém apenas as abstrações. Instale também um dos pacotes de algoritmo (veja "Pacotes relacionados") para registrar uma implementação de `IPasswordHasher`.

## Uso

Registre o builder no DI com `PasswordHasherServiceExtensions` e configure a força do hash com `WithStrength` (enum `PasswordHasherStrength`: `Interactive`, `Moderate` ou `Sensitive`). Em seguida, encadeie o método `Use*` do pacote de algoritmo escolhido:

```csharp
using Codout.Security.Core;
using Codout.Security.Argon2; // ou .Bcrypt / .Scrypt

builder.Services
    .UpgradePasswordSecurity() // ou UseCustomHashPasswordBuilder()
    .WithStrength(PasswordHasherStrength.Moderate)
    .UseArgon2();
```

Consuma `IPasswordHasher` por injeção de dependência:

```csharp
using Codout.Security.Core;

public class AccountService(IPasswordHasher hasher)
{
    public string Register(string password) => hasher.HashPassword(password);

    public bool Login(string hashed, string provided)
    {
        var result = hasher.VerifyHashedPassword(hashed, provided);
        // PasswordVerificationResult: Failed, Success ou SuccessRehashNeeded
        return result != PasswordVerificationResult.Failed;
    }
}
```

Quando `VerifyHashedPassword` retorna `PasswordVerificationResult.SuccessRehashNeeded`, a senha está correta, mas foi gerada com parâmetros mais fracos que os atuais — re-hasheie e persista o novo hash.

## Pacotes relacionados

- [Codout.Security.Argon2](https://www.nuget.org/packages/Codout.Security.Argon2) — Argon2id via libsodium
- [Codout.Security.Bcrypt](https://www.nuget.org/packages/Codout.Security.Bcrypt) — Bcrypt via BCrypt.Net-Next
- [Codout.Security.Scrypt](https://www.nuget.org/packages/Codout.Security.Scrypt) — Scrypt via libsodium

---
Parte do [Codout.Framework](https://github.com/Codout/Codout.Framework) — licença MIT.
