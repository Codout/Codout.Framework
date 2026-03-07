using System.ComponentModel;
using System.Text.Json;
using Codout.Framework.Mcp.Services;
using ModelContextProtocol.Server;

namespace Codout.Framework.Mcp.Tools;

[McpServerToolType]
public static class CodoutAiTools
{
    [McpServerTool, Description("Retorna a constituição oficial de UI/CRUD do ecossistema Codout.")]
    public static async Task<string> GetUiConstitution(
        IAiKnowledgeRepository repository,
        CancellationToken cancellationToken)
        => (await repository.GetDocumentAsync("ui-constitution", cancellationToken)).Content;

    [McpServerTool, Description("Retorna o catálogo oficial de componentes e abstrações canônicas do ecossistema Codout.")]
    public static async Task<string> GetComponentCatalog(
        IAiKnowledgeRepository repository,
        CancellationToken cancellationToken)
        => (await repository.GetDocumentAsync("components", cancellationToken)).Content;

    [McpServerTool(Name = "get_screen_pattern"), Description("Retorna o padrão de tela mais adequado. Valores aceitos: crud-simple, crud-filtered, crud-kpi, form-complex, details-actions, portal, auth, dashboard.")]
    public static async Task<string> GetScreenPattern(
        [Description("Tipo do padrão de tela.")] string type,
        IAiKnowledgeRepository repository,
        CancellationToken cancellationToken)
    {
        var document = await repository.GetDocumentAsync("screen-patterns", cancellationToken);
        var section = ExtractSection(document.Content, type);

        return string.IsNullOrWhiteSpace(section)
            ? document.Content
            : section;
    }

    [McpServerTool(Name = "find_gold_reference"), Description("Busca as referências ouro mais próximas por entidade, intenção ou padrão.")]
    public static async Task<string> FindGoldReference(
        [Description("Consulta textual, como 'financeiro com kpi' ou 'cadastro simples'.")] string query,
        [Description("Número máximo de resultados.")] int take,
        IAiKnowledgeRepository repository,
        CancellationToken cancellationToken)
    {
        take = take <= 0 ? 3 : Math.Min(take, 10);

        var matches = await repository.FindGoldReferencesAsync(query, take, cancellationToken);
        return JsonSerializer.Serialize(matches, JsonOptions.Pretty);
    }

    [McpServerTool(Name = "get_crud_recipe"), Description("Retorna a receita oficial para implementação de um CRUD padrão.")]
    public static async Task<string> GetCrudRecipe(
        IAiKnowledgeRepository repository,
        CancellationToken cancellationToken)
        => (await repository.GetDocumentAsync("crud-recipe", cancellationToken)).Content;

    [McpServerTool(Name = "get_grid_recipe"), Description("Retorna a receita oficial para listagens administrativas com codout-grid.")]
    public static async Task<string> GetGridRecipe(
        IAiKnowledgeRepository repository,
        CancellationToken cancellationToken)
        => (await repository.GetDocumentAsync("grid-recipe", cancellationToken)).Content;

    [McpServerTool(Name = "get_form_recipe"), Description("Retorna a receita oficial de formulário. Valores aceitos: simple, complex.")]
    public static async Task<string> GetFormRecipe(
        [Description("Complexidade do formulário: simple ou complex.")] string complexity,
        IAiKnowledgeRepository repository,
        CancellationToken cancellationToken)
    {
        var key = complexity.Equals("simple", StringComparison.OrdinalIgnoreCase)
            ? "form-simple-recipe"
            : "form-complex-recipe";

        return (await repository.GetDocumentAsync(key, cancellationToken)).Content;
    }

    [McpServerTool(Name = "get_details_actions_recipe"), Description("Retorna a receita para telas de detalhes com ações auxiliares.")]
    public static async Task<string> GetDetailsActionsRecipe(
        IAiKnowledgeRepository repository,
        CancellationToken cancellationToken)
        => (await repository.GetDocumentAsync("details-actions-recipe", cancellationToken)).Content;

    [McpServerTool(Name = "get_authorization_recipe"), Description("Retorna a receita de autorização e visibilidade canônicas.")]
    public static async Task<string> GetAuthorizationRecipe(
        IAiKnowledgeRepository repository,
        CancellationToken cancellationToken)
        => (await repository.GetDocumentAsync("authorization-recipe", cancellationToken)).Content;

    [McpServerTool(Name = "get_layout_recipe"), Description("Retorna a receita de layout, toolbar e containers compartilhados.")]
    public static async Task<string> GetLayoutRecipe(
        IAiKnowledgeRepository repository,
        CancellationToken cancellationToken)
        => (await repository.GetDocumentAsync("layout-recipe", cancellationToken)).Content;

    [McpServerTool(Name = "list_anti_patterns"), Description("Lista os anti-padrões proibidos no ecossistema Codout.")]
    public static async Task<string> ListAntiPatterns(
        IAiKnowledgeRepository repository,
        CancellationToken cancellationToken)
        => (await repository.GetDocumentAsync("anti-patterns", cancellationToken)).Content;

    [McpServerTool(Name = "get_decision_flow"), Description("Retorna o fluxo de decisão para criação de novas telas.")]
    public static async Task<string> GetDecisionFlow(
        IAiKnowledgeRepository repository,
        CancellationToken cancellationToken)
        => (await repository.GetDocumentAsync("decision-flow", cancellationToken)).Content;

    [McpServerTool(Name = "get_gold_reference"), Description("Retorna o markdown integral de uma referência ouro específica.")]
    public static async Task<string> GetGoldReference(
        [Description("Nome do arquivo sem extensão, por exemplo 'receivable-account'.")] string name,
        IAiKnowledgeRepository repository,
        CancellationToken cancellationToken)
        => (await repository.GetDocumentAsync($"gold:{name}", cancellationToken)).Content;

    [McpServerTool(Name = "list_gold_references"), Description("Lista os nomes das referências ouro disponíveis.")]
    public static async Task<string> ListGoldReferences(
        IAiKnowledgeRepository repository,
        CancellationToken cancellationToken)
    {
        var names = await repository.ListGoldReferenceNamesAsync(cancellationToken);
        return JsonSerializer.Serialize(names, JsonOptions.Pretty);
    }

    [McpServerTool(Name = "search_knowledge"), Description("Busca textual em todos os documentos curados do knowledge pack.")]
    public static async Task<string> SearchKnowledge(
        [Description("Consulta textual.")] string query,
        [Description("Quantidade máxima de resultados.")] int take,
        IAiKnowledgeRepository repository,
        CancellationToken cancellationToken)
    {
        take = take <= 0 ? 5 : Math.Min(take, 20);
        var docs = await repository.SearchAsync(query, take, cancellationToken);

        var payload = docs.Select(d => new
        {
            d.Key,
            d.Title,
            d.Category,
            d.RelativePath
        });

        return JsonSerializer.Serialize(payload, JsonOptions.Pretty);
    }

    [McpServerTool(Name = "get_server_status"), Description("Retorna o status atual do servidor MCP e do knowledge pack.")]
    public static async Task<string> GetServerStatus(
        IAiKnowledgeRepository repository,
        CancellationToken cancellationToken)
    {
        var status = await repository.GetStatusAsync(cancellationToken);
        return JsonSerializer.Serialize(status, JsonOptions.Pretty);
    }

    private static string ExtractSection(string markdown, string type)
    {
        var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["crud-simple"] = "## Padrão A",
            ["crud-filtered"] = "## Padrão B",
            ["crud-kpi"] = "## Padrão C",
            ["form-complex"] = "## Padrão D",
            ["details-actions"] = "## Padrão E",
            ["portal"] = "## Padrão F",
            ["auth"] = "## Padrão G",
            ["dashboard"] = "## Padrão H",
        };

        if (!map.TryGetValue(type, out var marker))
        {
            return string.Empty;
        }

        var start = markdown.IndexOf(marker, StringComparison.OrdinalIgnoreCase);
        if (start < 0)
        {
            return string.Empty;
        }

        var next = markdown.IndexOf("\n## ", start + marker.Length, StringComparison.OrdinalIgnoreCase);
        return next < 0 ? markdown[start..].Trim() : markdown[start..next].Trim();
    }

    private static class JsonOptions
    {
        public static readonly JsonSerializerOptions Pretty = new()
        {
            WriteIndented = true
        };
    }
}
