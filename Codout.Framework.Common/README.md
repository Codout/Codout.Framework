# Codout.Framework.Common

Biblioteca de funções comuns do Codout.Framework: métodos de extensão, helpers, validações brasileiras (CPF/CNPJ), anotações de validação e utilitários de criptografia para uso geral em aplicações .NET.

## Instalação

```bash
dotnet add package Codout.Framework.Common
```

## O que contém

- **Extensions/** — extensões para `string` (`StringExtensions`: `RemoveAccents`, `OnlyNumbers`, `StripHtml`, `Truncate`, `ToEnum<T>`...), datas (`DateTimeExtensions`), números (`NumericExtensions`), coleções (`LinqExtensions`), IO, streams, tasks e validações (`ValidationExtensions`: `IsCpf`, `IsCnpj`, `IsEmail`...).
- **Helpers/** — `SlugHelper`/`SlugExtensions`, `Inflector`, `EnumHelper`, `GeoHelper`, `NumberToText`, `AsyncHelper`, `LimitedList`, entre outros.
- **Annotations/** — atributos de validação como `CpfCnpjAttribute`, `RequiredIfAttribute`, `DateRangeAttribute`, `MustBeTrueAttribute`.
- **Security/** — `Crypto`, `CryptoString`, `CryptoFile`, `RandomPassword`, `SimpleHash`.
- **Constants/** — padrões de regex reutilizáveis (`RegexPattern`).

## Uso

Validação de CPF/CNPJ e manipulação de strings:

```csharp
using Codout.Framework.Common.Extensions;
using Codout.Framework.Common.Helpers;

bool ok = "123.456.789-09".OnlyNumbers().IsCpf();

string slug = "Título do Artigo 2026".ToSlug();   // "titulo-do-artigo-2026"
string limpo = "Café com Açúcar".RemoveAccents(); // "Cafe com Acucar"
```

Anotação de validação em um modelo:

```csharp
using Codout.Framework.Common.Annotations;

public class ClienteDto
{
    [CpfCnpj(ErrorMessage = "CPF ou CNPJ inválido")]
    public string Documento { get; set; }
}
```

## Pacotes relacionados

- [Codout.Framework.Domain](https://www.nuget.org/packages/Codout.Framework.Domain) — entidades base de domínio do ecossistema Codout, em que estas extensões e anotações se encaixam naturalmente.
- [Codout.Framework.Data](https://www.nuget.org/packages/Codout.Framework.Data) — abstrações de repositório e Unit of Work do mesmo ecossistema.

---
Parte do [Codout.Framework](https://github.com/Codout/Codout.Framework) — licença MIT.
