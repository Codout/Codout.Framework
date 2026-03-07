# Integration Guide

## 1. Codout.Club
Copie a pasta `docs/ai` para a raiz do repositório consumidor.

## 2. Codout.Framework
Copie `src/Tools/Codout.Framework.Mcp` para o repositório do framework.

## 3. Solution
Adicione o projeto `src/Tools/Codout.Framework.Mcp/Codout.Framework.Mcp.csproj` à solution do framework.

## 4. Configuração
Defina `CODOUT_MCP_CodoutAi__DocsRoot` se os docs estiverem em outro repositório, por exemplo `D:/source/Codout/Codout.Club/docs/ai`.

## 5. Execução
Rode o projeto MCP com `dotnet run --project src/Tools/Codout.Framework.Mcp/Codout.Framework.Mcp.csproj`.
