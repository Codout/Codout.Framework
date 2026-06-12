# FINDINGS — Suítes de teste (Common, Security, Image.Extensions)

Achados registrados durante a criação das suítes de teste. Nenhum código de
produção foi alterado; os comportamentos abaixo estão documentados nos testes
como *characterization tests* marcados com `// BUG?:`.

## Codout.Framework.Common

### 1. `StringExtensions.RemoveAccents` lança exceção em .NET moderno (e usa codepage errado)
- Arquivo: `Codout.Framework.Common/Extensions/StringExtensions.cs`
- O método usa `Encoding.GetEncoding("iso-8859-8")` (hebraico) para "remover
  acentos". Em .NET (Core/5+) esse encoding não está registrado por padrão
  (exigiria `CodePagesEncodingProvider`), então a chamada lança
  `ArgumentException` em runtime. Mesmo se o encoding estivesse disponível, o
  codepage correto para transliterar latinos seria outro (ex.: normalização
  Unicode FormD, como o `SlugHelper` já faz).
- Efeito colateral: `RemoveCharactersSpecial(..., replaceAccents: true)`
  (caminho padrão) também lança a mesma exceção.
- Teste: `StringExtensionsTests.RemoveAccents_ComportamentoAtual`.

### 2. `StringExtensions.Chop(string backDownTo)` lança exceção quando o padrão não existe
- Arquivo: `Codout.Framework.Common/Extensions/StringExtensions.cs`
- Quando `LastIndexOf` retorna `-1`, o código chama `Remove(-1, 0)`, que lança
  `ArgumentOutOfRangeException` em vez de retornar a string original.
- Teste: `StringExtensionsTests.Chop_ComStringAlvoInexistente_LancaExcecao`.

### 3. `StringExtensions.HtmlEncode` faz duplo escape do `&` das entidades
- Arquivo: `Codout.Framework.Common/Extensions/StringExtensions.cs`
- A substituição de `&` por `&amp;` acontece **depois** da geração das
  entidades, então `é` vira `&amp;eacute;` em vez de `&eacute;`. O roundtrip
  com `HtmlDecode` ainda funciona porque o decode desfaz na ordem inversa, mas
  a saída não é HTML-encoding válido para consumo externo.
- Teste: `StringExtensionsTests.HtmlEncode_HtmlDecode_SaoInversos`.

### 4. `DateTimeExtensions.GetAge` erra a idade no dia do aniversário
- Arquivo: `Codout.Framework.Common/Extensions/DateTimeExtensions.cs`
- A comparação `dateOfBirth.DayOfYear < dateAsAt.DayOfYear` é estrita: no
  próprio dia do aniversário a idade é subtraída em 1 (nascido em 15/06/2000,
  em 15/06/2020 o método retorna 19 em vez de 20). A comparação por `DayOfYear`
  também sofre desvio de 1 dia em anos bissextos.
- Teste: `DateTimeExtensionsTests.GetAge_NoDiaDoAniversario_ComportamentoAtual`.

### 5. `ValidationExtensions.IsEmail` considera string vazia/whitespace válida
- Arquivo: `Codout.Framework.Common/Extensions/ValidationExtensions.cs`
- `string.IsNullOrWhiteSpace(email) || Regex.IsMatch(...)` faz com que `""` e
  `"   "` retornem `true`. Pode ser intencional (campo opcional), mas é
  surpreendente para um validador chamado `IsEmail`.
- Teste: `ValidationExtensionsTests.IsEmail_StringVazia_RetornaTrue`.

### 6. `EnumHelper.GetLocalizedName` lê `Description` em vez de `Name` do `DisplayAttribute` (observação)
- Arquivo: `Codout.Framework.Common/Helpers/EnumHelper.cs`
- O método chama `DisplayAttribute.GetDescription()`; se o atributo for usado
  como `[Display(Name = "...")]` (uso mais comum), o retorno é `null`. Os
  testes usam `[Display(Description = "...")]` para refletir o comportamento
  atual.
- Teste: `EnumHelperTests.GetLocalizedName_ComDisplayAttribute_RetornaNome`.

## Codout.Security (src/Security)

### 7. `ArgonPasswordHash.NeedsRehash` usa tabela de parâmetros que não corresponde ao Sodium.Core
- Arquivo: `src/Security/Codout.Security.Argon2/ArgonPasswordHash.cs`
- `GetExpectedParameters()` assume as constantes do libsodium para argon2id
  (`Interactive: t=2, m=65536 KiB; Moderate: t=3, m=262144 KiB`), mas o
  Sodium.Core 1.4 gera hashes com outras constantes (verificado empiricamente):
  - `Interactive`: `m=32768, t=4`
  - `Moderate`: `m=131072, t=6`
- Como o `m` armazenado é sempre menor que o esperado, **todo** hash gerado via
  `Strength` (Interactive/Moderate) retorna `SuccessRehashNeeded` ao ser
  verificado pelo mesmo hasher — nunca `Success` — induzindo rehash perpétuo a
  cada login. Com `Argon2Options.OpsLimit`/`MemLimit` explícitos o roundtrip
  funciona corretamente (`Success`).
- Testes:
  - `Argon2Tests.VerifyHashedPassword_SenhaCorreta_ComStrength_ComportamentoAtual`
    (characterization do bug)
  - `Argon2Tests.VerifyHashedPassword_SenhaCorreta_ComLimitesCustomizados_RetornaSuccess`
    (caminho que funciona)
- Obs.: o equivalente em Scrypt (`ScryptPasswordHash.NeedsRehash`) está correto
  — o roundtrip com `Strength.Interactive` retorna `Success`
  (`ScryptTests.VerifyHashedPassword_SenhaCorreta_RetornaSuccess`).

## Codout.Image.Extensions

Nenhum bug encontrado nas APIs testadas (`Extract`, `DrawRectangles`,
`DrawPoints`, `DrawRectanglesAndPoints`). Os métodos de `GeometryExtensions`
são `internal` e não foram testados para não alterar o código de produção
(faltaria `InternalsVisibleTo`).
