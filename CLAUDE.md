# Instruções para Claude e outros agentes de IA

## Encoding

**Todo arquivo criado ou editado neste repositório deve ser salvo em UTF-8
(sem BOM, salvo quando o tipo de arquivo o exigir).** Não introduza nem
preserve conteúdo em Windows-1252, ISO-8859-1 ou qualquer outra codificação
de byte único. Se encontrar um arquivo legado em outra codificação, converta
para UTF-8 antes de editar.

Convenção do repositório (também declarada em `.editorconfig`):
- `charset = utf-8`
- `end_of_line = lf` (CRLF apenas em `*.cmd` / `*.bat`)
- `insert_final_newline = true`

## Versionamento

- **Cada `.csproj` publicável tem seu próprio `<Version>`.** A versão NÃO é mais centralizada em `Directory.Build.props` — não reintroduza-a lá.
- O mapeamento `short-name → caminho.csproj` está em `.github/release-packages.json`. Sempre consulte esse arquivo para resolver o nome curto de um pacote.
- Versionamento segue [SemVer](https://semver.org). Em caso de ambiguidade sobre o nível de bump, **pergunte ao usuário** (patch / minor / major) usando `AskUserQuestion`.

## Como gerar uma nova versão e publicar um pacote

Quando o usuário pedir "gerar nova versão e publicar do pacote X" (ou equivalente), siga exatamente esta sequência. Se o usuário pedir apenas "bumpar" / "preparar release" sem mencionar "publicar", **pare antes do passo 6** (não criar tag).

1. **Resolva pacote + bump level**. Se o usuário não foi explícito:
   - Use `.github/release-packages.json` para listar os nomes curtos válidos.
   - Use `AskUserQuestion` para confirmar o pacote e/ou o nível de bump quando não estiver óbvio pela natureza das mudanças.
2. **Bump no `.csproj` do pacote alvo.** Edite apenas o `<Version>` daquele projeto. NÃO toque em outros `.csproj` nem em `Directory.Build.props`.
3. **Atualize `CHANGELOG.md`** seguindo o formato per-pacote já estabelecido:
   ```markdown
   ## YYYY-MM-DD

   ### Codout.Framework.X 6.A.B

   #### Fixed | Added | Changed | Removed
   - Descrição em português, frase começando com letra maiúscula.
   ```
   Se já existe uma seção sob a data de hoje, complemente em vez de duplicar o cabeçalho. Para mudanças cross-cutting (workflow, build, encoding, etc.) use `### Build` ou `### Repository` sob a mesma data.
4. **Commit** no padrão Conventional Commits, com o tipo refletindo a mudança (`fix`, `feat`, `chore`, `docs`, `ci`, `refactor`) e escopo opcional usando o short-name (`fix(ef): ...`, `chore(mailer-razor): ...`). Mensagem em modo imperativo, primeira linha ≤ 72 chars, footer `https://claude.ai/code/session_<id>`.
5. **Push do commit** para o branch de trabalho (geralmente `claude/*` ou direto em `master` se o usuário autorizar). Confirme que o commit está em `origin/master` antes do passo seguinte:
   ```bash
   git fetch origin master --quiet
   git merge-base --is-ancestor HEAD origin/master && echo "on master" || echo "NOT on master"
   ```
   Se não estiver em master, **pare e avise o usuário** — o workflow `release.yml` rejeita tags fora de master.
6. **Crie e push da tag** no formato `<short>-v<X.Y.Z>`:
   ```bash
   git tag <short>-v<X.Y.Z>
   git push origin <short>-v<X.Y.Z>
   ```
   Isso dispara `.github/workflows/release.yml`, que builda, roda `dotnet test` na solution e publica no NuGet.org via `secrets.NUGET_API_KEY`. **Pushar a tag é a ação que realmente publica.** Só faça nesse passo se a intenção do usuário foi "publicar". Caso contrário, pare no passo 5 e relate o estado.
7. **Relate ao usuário** os SHAs do commit, o nome da tag, e link para a run do workflow no GitHub Actions assim que disponível.

## Casos especiais

- **Codout.Framework.Mcp**: usa workflow próprio (`.github/workflows/mcp-release.yml`) com passo `--validate` específico do CLI. Tag deve ser `mcp-v<X.Y.Z>` (NÃO use `mcp` como short-name no `release.yml` — está intencionalmente bloqueado por padrão de tag para evitar duplo release).
- **Mass release** (todos os pacotes de uma vez via `.github/workflows/mass-release.yml`): só execute se o usuário pedir explicitamente "mass release" ou "publicar todos". Mesmo assim, oriente o usuário a disparar via Actions UI com `dry_run: true` primeiro e revisar os artifacts antes de re-disparar com `dry_run: false`. Não tente disparar pela CLI sem autorização explícita.

## O que NÃO fazer

- ❌ Reintroduzir `<Version>` em `Directory.Build.props`.
- ❌ Bumpar pacotes que não foram tocados pela mudança — cada pacote anda no próprio ritmo.
- ❌ Criar / pushar tag sem o usuário ter pedido "publicar" (a tag é a ação visível externamente que publica no NuGet.org).
- ❌ Disparar `mass-release.yml` sem o usuário pedir explicitamente, nem mesmo via `gh workflow run`.
- ❌ Adivinhar nível de bump em mudanças ambíguas — pergunte.
- ❌ Skippar a verificação de master-ancestry antes de criar a tag.
