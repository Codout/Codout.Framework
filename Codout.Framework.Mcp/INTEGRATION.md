# Integration Guide

O servidor MCP é distribuído como **.NET global tool** com o knowledge pack embutido. Em projetos novos, normalmente basta instalar o tool e apontar o cliente MCP para `codout-mcp`.

## 1. Instalação do tool

```bash
dotnet tool install -g Codout.Framework.Mcp
```

Confirme:

```bash
codout-mcp --validate
codout-mcp --list-tools
```

## 2. Registrar no cliente MCP

### Claude Code

```bash
claude mcp add codout-framework -- codout-mcp
```

### Claude Desktop / VSCode / Cursor

Configuração mínima (ver detalhes específicos por cliente em [src/Tools/Codout.Framework.Mcp/README.md](src/Tools/Codout.Framework.Mcp/README.md)):

```json
{
  "mcpServers": {
    "codout-framework": {
      "command": "codout-mcp"
    }
  }
}
```

## 3. (Opcional) Apontar para `docs/ai` externo

Útil para iterar a constituição em `Codout.Club` sem republicar o tool:

```json
{
  "mcpServers": {
    "codout-framework": {
      "command": "codout-mcp",
      "env": {
        "CODOUT_MCP_CodoutAi__DocsRoot": "D:/source/Codout/Codout.Club/docs/ai"
      }
    }
  }
}
```

## 4. Modo dev (sem instalar o tool)

```bash
git clone https://github.com/Codout/Codout.Framework
cd Codout.Framework
dotnet run --project Codout.Framework.Mcp/src/Tools/Codout.Framework.Mcp/Codout.Framework.Mcp.csproj -- --validate
```

Aponte o cliente MCP para `dotnet run --project <csproj>` em vez de `codout-mcp`.

## 5. Release

Push de tag `mcp-vX.Y.Z` dispara o workflow [`mcp-release.yml`](.github/workflows/mcp-release.yml) que:

1. Faz build, testes e `--validate`.
2. Empacota o `.nupkg`.
3. Publica em [nuget.org](https://www.nuget.org) usando o secret `NUGET_API_KEY`.

```bash
git tag mcp-v6.2.3
git push origin mcp-v6.2.3
```
