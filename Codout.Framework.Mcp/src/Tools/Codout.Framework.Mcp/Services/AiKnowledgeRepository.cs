using System.Collections.Concurrent;
using System.Text;
using Codout.Framework.Mcp.Models;

namespace Codout.Framework.Mcp.Services;

public sealed class FileSystemAiKnowledgeRepository : IAiKnowledgeRepository
{
    private static readonly IReadOnlyDictionary<string, string> StaticDocuments = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        ["ui-constitution"] = "constitution/ui-crud.md",
        ["components"] = "catalog/components.md",
        ["screen-patterns"] = "catalog/screen-patterns.md",
        ["anti-patterns"] = "catalog/anti-patterns.md",
        ["decision-flow"] = "catalog/decision-flow.md",
        ["crud-recipe"] = "recipes/crud-standard.md",
        ["grid-recipe"] = "recipes/grid-standard.md",
        ["form-simple-recipe"] = "recipes/form-simple.md",
        ["form-complex-recipe"] = "recipes/form-complex.md",
        ["details-actions-recipe"] = "recipes/details-actions.md",
        ["authorization-recipe"] = "recipes/authorization.md",
        ["layout-recipe"] = "recipes/layout.md",
    };

    private readonly string _docsRoot;
    private readonly ConcurrentDictionary<string, KnowledgeDocument> _cache = new(StringComparer.OrdinalIgnoreCase);

    public FileSystemAiKnowledgeRepository(IDocsRootResolver docsRootResolver)
    {
        _docsRoot = docsRootResolver.Resolve();
    }

    public async Task<KnowledgeDocument> GetDocumentAsync(string key, CancellationToken cancellationToken = default)
    {
        if (_cache.TryGetValue(key, out var cached))
        {
            return cached;
        }

        if (!TryResolvePath(key, out var relativePath))
        {
            throw new FileNotFoundException($"Knowledge document '{key}' was not mapped.");
        }

        var fullPath = Path.Combine(_docsRoot, relativePath);
        if (!File.Exists(fullPath))
        {
            throw new FileNotFoundException($"Knowledge document not found at '{fullPath}'.");
        }

        var content = await File.ReadAllTextAsync(fullPath, Encoding.UTF8, cancellationToken);
        var document = new KnowledgeDocument(
            key,
            Path.GetFileNameWithoutExtension(relativePath),
            GetCategory(relativePath),
            relativePath.Replace("\\", "/"),
            content);

        _cache[key] = document;
        return document;
    }

    public async Task<IReadOnlyList<KnowledgeDocument>> GetAllDocumentsAsync(CancellationToken cancellationToken = default)
    {
        var docs = new List<KnowledgeDocument>();

        foreach (var key in StaticDocuments.Keys)
        {
            docs.Add(await GetDocumentAsync(key, cancellationToken));
        }

        foreach (var name in await ListGoldReferenceNamesAsync(cancellationToken))
        {
            docs.Add(await GetDocumentAsync($"gold:{name}", cancellationToken));
        }

        return docs;
    }

    public async Task<IReadOnlyList<KnowledgeDocument>> SearchAsync(string query, int take = 5, CancellationToken cancellationToken = default)
    {
        query ??= string.Empty;
        take = take <= 0 ? 5 : Math.Min(take, 20);
        var tokens = Tokenize(query);
        var docs = await GetAllDocumentsAsync(cancellationToken);

        var ranked = docs
            .Select(document => new
            {
                Document = document,
                Score = Score(document.Content + " " + document.Title + " " + document.RelativePath, tokens)
            })
            .Where(x => x.Score > 0 || x.Document.Content.Contains(query, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(x => x.Score)
            .ThenBy(x => x.Document.Title, StringComparer.OrdinalIgnoreCase)
            .Take(take)
            .Select(x => x.Document)
            .ToList();

        return ranked;
    }

    public async Task<IReadOnlyList<GoldReferenceMatch>> FindGoldReferencesAsync(string query, int take = 5, CancellationToken cancellationToken = default)
    {
        query ??= string.Empty;
        var tokens = Tokenize(query);
        var results = new List<GoldReferenceMatch>();

        foreach (var name in await ListGoldReferenceNamesAsync(cancellationToken))
        {
            var document = await GetDocumentAsync($"gold:{name}", cancellationToken);
            var score = Score(document.Content, tokens);

            if (score <= 0 && !document.Content.Contains(query, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            results.Add(new GoldReferenceMatch(
                Name: name,
                Module: TryExtractBulletValue(document.Content, "Módulo") ?? "Unknown",
                Pattern: TryExtractBulletValue(document.Content, "Tipo") ?? "Unknown",
                Why: ExtractWhy(document.Content),
                Score: score));
        }

        return results
            .OrderByDescending(x => x.Score)
            .ThenBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
            .Take(Math.Max(1, take))
            .ToList();
    }

    public Task<IReadOnlyList<string>> ListGoldReferenceNamesAsync(CancellationToken cancellationToken = default)
    {
        var root = Path.Combine(_docsRoot, "references", "gold");
        if (!Directory.Exists(root))
        {
            return Task.FromResult<IReadOnlyList<string>>(Array.Empty<string>());
        }

        IReadOnlyList<string> files = Directory.EnumerateFiles(root, "*.md", SearchOption.TopDirectoryOnly)
            .Select(Path.GetFileNameWithoutExtension)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x!)
            .OrderBy(x => x, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        return Task.FromResult(files);
    }

    public Task<RepositoryStatus> GetStatusAsync(CancellationToken cancellationToken = default)
    {
        var goldRoot = Path.Combine(_docsRoot, "references", "gold");
        var goldCount = Directory.Exists(goldRoot)
            ? Directory.EnumerateFiles(goldRoot, "*.md", SearchOption.TopDirectoryOnly).Count()
            : 0;

        return Task.FromResult(new RepositoryStatus(
            DocsRootResolved: Directory.Exists(_docsRoot),
            DocsRoot: _docsRoot,
            StaticDocumentCount: StaticDocuments.Count,
            GoldReferenceCount: goldCount));
    }

    private bool TryResolvePath(string key, out string relativePath)
    {
        if (StaticDocuments.TryGetValue(key, out relativePath!))
        {
            return true;
        }

        if (key.StartsWith("gold:", StringComparison.OrdinalIgnoreCase))
        {
            var name = key["gold:".Length..].Trim();
            relativePath = Path.Combine("references", "gold", $"{name}.md");
            return true;
        }

        relativePath = string.Empty;
        return false;
    }

    private static string GetCategory(string relativePath)
    {
        var normalized = relativePath.Replace("\\", "/");
        return normalized.Split('/', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? "misc";
    }

    private static string[] Tokenize(string query)
    {
        return query
            .Split(new[] { ' ', '\t', '\r', '\n', ',', ';', ':', '.', '/', '\\', '-', '_' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(x => x.Length >= 3)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    private static double Score(string content, IReadOnlyList<string> tokens)
    {
        if (tokens.Count == 0)
        {
            return 0;
        }

        double score = 0;
        foreach (var token in tokens)
        {
            var count = CountOccurrences(content, token);
            if (count > 0)
            {
                score += count;
            }
        }

        return score;
    }

    private static int CountOccurrences(string source, string value)
    {
        var count = 0;
        var index = 0;
        while ((index = source.IndexOf(value, index, StringComparison.OrdinalIgnoreCase)) >= 0)
        {
            count++;
            index += value.Length;
        }

        return count;
    }

    private static string? TryExtractBulletValue(string markdown, string key)
    {
        var prefix = $"- **{key}**:";
        foreach (var line in markdown.Split(Environment.NewLine))
        {
            if (line.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                return line[prefix.Length..].Trim();
            }
        }

        return null;
    }

    private static string ExtractWhy(string markdown)
    {
        const string marker = "## Por que é referência";
        var index = markdown.IndexOf(marker, StringComparison.OrdinalIgnoreCase);
        if (index < 0)
        {
            return "Referência canônica do projeto.";
        }

        var rest = markdown[(index + marker.Length)..].Trim();
        var lines = rest.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries)
            .Where(x => x.StartsWith("- "))
            .Take(2)
            .ToArray();

        return lines.Length == 0
            ? "Referência canônica do projeto."
            : string.Join(" ", lines.Select(x => x.TrimStart('-', ' ')));
    }
}
