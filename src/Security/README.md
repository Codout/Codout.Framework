# Codout.Security

Biblioteca para hashing seguro de senhas com suporte a múltiplos algoritmos via injeção de dependência.

## Pacotes

| Pacote | Algoritmo | NuGet |
|---|---|---|
| `Codout.Security.Core` | Abstrações e interfaces | Dependência obrigatória |
| `Codout.Security.Argon2` | Argon2id (via libsodium) | Recomendado |
| `Codout.Security.Bcrypt` | BCrypt | Amplamente suportado |
| `Codout.Security.Scrypt` | Scrypt (via libsodium) | Alternativa |

## Instalação

Instale o pacote do algoritmo desejado (o Core é incluído automaticamente):

```bash
dotnet add package Codout.Security.Argon2
# ou
dotnet add package Codout.Security.Bcrypt
# ou
dotnet add package Codout.Security.Scrypt
```

## Configuração

### Argon2 (recomendado)

```csharp
using Codout.Security.Argon2;
using Codout.Security.Core;

services.UpgradePasswordSecurity()
    .WithStrength(PasswordHasherStrength.Sensitive)
    .UseArgon2();
```

Com parâmetros customizados:

```csharp
services.UpgradePasswordSecurity()
    .WithStrength(PasswordHasherStrength.Sensitive)
    .UseArgon2(options =>
    {
        options.OpsLimit = 4;          // Custo computacional
        options.MemLimit = 1073741824; // 1 GB de RAM
    });
```

### BCrypt

```csharp
using Codout.Security.Bcrypt;
using Codout.Security.Core;

services.UpgradePasswordSecurity()
    .UseBcrypt();
```

Com parâmetros customizados:

```csharp
services.UpgradePasswordSecurity()
    .UseBcrypt(options =>
    {
        options.WorkFactor = 12;                              // Padrão: 12
        options.SaltRevision = BcryptSaltRevision.Revision2B; // Padrão: 2B
    });
```

### Scrypt

```csharp
using Codout.Security.Scrypt;
using Codout.Security.Core;

services.UpgradePasswordSecurity()
    .WithStrength(PasswordHasherStrength.Sensitive)
    .UseScrypt();
```

Com parâmetros customizados:

```csharp
services.UpgradePasswordSecurity()
    .WithStrength(PasswordHasherStrength.Sensitive)
    .UseScrypt(options =>
    {
        options.OpsLimit = 4194304;
        options.MemLimit = 1073741824;
    });
```

## Uso

Injete `IPasswordHasher` no seu serviço:

```csharp
public class AccountService(IPasswordHasher passwordHasher)
{
    public string CreateHash(string password)
    {
        return passwordHasher.HashPassword(password);
    }

    public bool VerifyPassword(string hashedPassword, string providedPassword)
    {
        var result = passwordHasher.VerifyHashedPassword(hashedPassword, providedPassword);

        switch (result)
        {
            case PasswordVerificationResult.Success:
                return true;

            case PasswordVerificationResult.SuccessRehashNeeded:
                // Senha válida, mas o hash precisa ser atualizado
                var newHash = passwordHasher.HashPassword(providedPassword);
                // Persistir newHash no banco de dados
                return true;

            case PasswordVerificationResult.Failed:
            default:
                return false;
        }
    }
}
```

## Strength

O enum `PasswordHasherStrength` controla o custo computacional para Argon2 e Scrypt:

| Nível | Uso de RAM | Indicação |
|---|---|---|
| `Interactive` | ~16 MB | Sessões interativas, login rápido |
| `Moderate` | ~128 MB | Uso geral |
| `Sensitive` | ~1 GB | Dados altamente sensíveis |

> **Nota:** O `Strength` não se aplica ao BCrypt, que usa `WorkFactor` para controlar o custo.

## SuccessRehashNeeded

Os providers Argon2, BCrypt e Scrypt detectam automaticamente quando um hash existente foi gerado com parâmetros mais fracos que os atuais. Quando isso acontece, `VerifyHashedPassword` retorna `SuccessRehashNeeded` em vez de `Success`.

Isso permite migrar hashes antigos de forma transparente — basta regerar o hash quando esse resultado for retornado.
