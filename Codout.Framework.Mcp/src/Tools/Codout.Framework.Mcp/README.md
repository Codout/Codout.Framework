# Codout.Framework.Mcp

Servidor MCP em C# para servir o conhecimento curado do ecossistema Codout a partir da pasta `docs/ai`.

## O que este projeto entrega
- transporte `stdio`
- tools reais para constituição, catálogo, padrões, recipes e referências ouro
- busca textual em documentos curados
- descoberta robusta da pasta `docs/ai`
- validação inicial do knowledge pack na subida

## Estrutura sugerida no repositório
```text
/src
  /Tools
    /Codout.Framework.Mcp
```

## Dependências
- .NET 8
- pacote `ModelContextProtocol`
- `Microsoft.Extensions.Hosting`

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

## Configuração
O servidor tenta localizar automaticamente `docs/ai` em cenários comuns:
- pasta configurada em `CodoutAi:DocsRoot`
- variável de ambiente `CODOUT_MCP_CodoutAi__DocsRoot`
- `docs/ai` no repositório atual
- `../Codout.Club/docs/ai` em repositório irmão

Você pode forçar um caminho absoluto via variável de ambiente.

## Exemplo de configuração no Claude Desktop / VSCode
```json
{
  "mcpServers": {
    "codout-framework": {
      "command": "dotnet",
      "args": [
        "run",
        "--project",
        "src/Tools/Codout.Framework.Mcp/Codout.Framework.Mcp.csproj"
      ],
      "env": {
        "CODOUT_MCP_CodoutAi__DocsRoot": "D:/source/Codout/Codout.Club/docs/ai"
      }
    }
  }
}
```

## Integração recomendada
1. Copiar `docs/ai` para o repositório consumidor.
2. Copiar `src/Tools/Codout.Framework.Mcp` para o repositório `Codout.Framework`.
3. Adicionar o projeto à solution do framework.
4. Restaurar e compilar.
5. Configurar o cliente MCP.

## Limite honesto
Sem um ambiente com `dotnet`, esta entrega pode ser validada estruturalmente, mas não compilada aqui.
