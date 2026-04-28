# Codout.Framework.Mcp

Servidor MCP (Model Context Protocol) que expõe a constituição, catálogo, padrões de tela, recipes e referências ouro do ecossistema Codout para clientes como Claude Code, Claude Desktop, VSCode e Cursor.

O knowledge pack (`docs/ai`) é embutido no assembly, então o tool é totalmente autocontido após `dotnet tool install`.

## Instalação

### Como .NET global tool (recomendado)

```bash
dotnet tool install -g Codout.Framework.Mcp
codout-mcp --validate
```

Atualizar:

```bash
dotnet tool update -g Codout.Framework.Mcp
```

Desinstalar:

```bash
dotnet tool uninstall -g Codout.Framework.Mcp
```

### Rodando direto do repositório (modo dev)

```bash
dotnet run --project src/Tools/Codout.Framework.Mcp/Codout.Framework.Mcp.csproj
```

## CLI

```bash
codout-mcp                # roda o servidor MCP em stdio (padrão)
codout-mcp --validate     # imprime status do knowledge pack e sai
codout-mcp --list-tools   # lista as tools MCP expostas e sai
codout-mcp --version      # imprime versão e sai
codout-mcp --help         # ajuda
```

## Tools expostas

- `get_ui_constitution`
- `get_component_catalog`
- `get_screen_pattern`
- `find_gold_reference`
- `get_crud_recipe`
- `get_grid_recipe`
- `get_form_recipe`
- `get_details_actions_recipe`
- `get_authorization_recipe`
- `get_layout_recipe`
- `list_anti_patterns`
- `get_decision_flow`
- `get_gold_reference`
- `list_gold_references`
- `search_knowledge`
- `get_server_status`

## Configuração de clientes MCP

Após `dotnet tool install -g Codout.Framework.Mcp`, basta apontar o cliente para o executável `codout-mcp`.

### Claude Code

```bash
claude mcp add codout-framework -- codout-mcp
```

Ou editando `~/.claude/mcp.json`:

```json
{
  "mcpServers": {
    "codout-framework": {
      "command": "codout-mcp"
    }
  }
}
```

### Claude Desktop

`%AppData%\Claude\claude_desktop_config.json` (Windows) ou `~/Library/Application Support/Claude/claude_desktop_config.json` (macOS):

```json
{
  "mcpServers": {
    "codout-framework": {
      "command": "codout-mcp"
    }
  }
}
```

### VSCode (extensão Claude/Continue/Cline)

```json
{
  "mcpServers": {
    "codout-framework": {
      "command": "codout-mcp"
    }
  }
}
```

### Cursor

`~/.cursor/mcp.json`:

```json
{
  "mcpServers": {
    "codout-framework": {
      "command": "codout-mcp"
    }
  }
}
```

## Sobrescrevendo o knowledge pack (modo dev / Codout.Club)

Por padrão o servidor lê o knowledge pack embutido. Para apontar para uma cópia externa de `docs/ai` (por exemplo, durante edição da constituição em `Codout.Club`):

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

A fonte de filesystem tem prioridade sobre a embutida quando o diretório existir; senão o servidor cai automaticamente para o pacote embutido.

## Diagnóstico

```bash
codout-mcp --validate
```

Saída esperada (exemplo):

```
Source       : filesystem(D:/source/Codout/Codout.Club/docs/ai) -> embedded(Codout.Framework.Mcp; 24 resources)
Resolved     : True
StaticDocs   : 12
GoldRefs     : 7
Filesystem   : filesystem(D:/source/Codout/Codout.Club/docs/ai) (resolved=True)
Embedded     : embedded(Codout.Framework.Mcp; 24 resources) (resolved=True)
```

## Stack

- .NET 10
- `ModelContextProtocol`
- `Microsoft.Extensions.Hosting`
- Knowledge pack (`docs/ai/**/*.md`) embutido como `EmbeddedResource`
