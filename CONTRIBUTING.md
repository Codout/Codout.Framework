# Contribuindo com o Codout.Framework

Obrigado pelo interesse em contribuir! Este guia resume o essencial.

## Antes de começar

- Procure uma [issue existente](https://github.com/Codout/Codout.Framework/issues)
  ou abra uma nova descrevendo o problema/proposta antes de PRs grandes.
- O repositório usa UTF-8 (sem BOM), LF e newline final — já configurado em
  `.editorconfig`.

## Fluxo

1. Faça um fork e crie uma branch: `git checkout -b feature/minha-mudanca`
2. Implemente a mudança **com testes** (os projetos de teste ficam em `tests/`)
3. Garanta build e testes verdes:
   ```bash
   dotnet build Codout.Framework.sln
   dotnet test Codout.Framework.sln
   ```
4. Abra um Pull Request descrevendo motivação e impacto

## Commits

Seguimos [Conventional Commits](https://www.conventionalcommits.org/pt-br/):
`fix:`, `feat:`, `chore:`, `docs:`, `ci:`, `refactor:`, `test:` — com escopo
opcional pelo nome curto do pacote (ex.: `fix(ef): ...`). Mensagens em modo
imperativo, primeira linha com até 72 caracteres.

## Versionamento e releases

- Cada pacote tem `<Version>` no próprio `.csproj` e segue
  [SemVer](https://semver.org) — não versione pacotes que sua mudança não toca.
- O CHANGELOG.md é por pacote, sob a data da mudança.
- Releases são disparados por tag (`<short-name>-v<X.Y.Z>`) e publicados no
  NuGet.org via GitHub Actions. O mapeamento de nomes curtos está em
  `.github/release-packages.json`. Criação de tags é responsabilidade dos
  mantenedores.

## Compatibilidade

- Mudanças que quebram API pública exigem bump **major** e destaque no
  CHANGELOG.
- Não remova membros `[Obsolete]` fora de um major.

## Licença

Ao contribuir, você concorda que sua contribuição será licenciada sob a
[MIT License](LICENSE).
