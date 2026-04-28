namespace Codout.Framework.Mcp.Services;

public sealed class CompositeKnowledgeSource : IKnowledgeSource
{
    private readonly IReadOnlyList<IKnowledgeSource> _sources;

    public CompositeKnowledgeSource(IEnumerable<IKnowledgeSource> sources)
    {
        _sources = sources
            .Where(s => s is not null)
            .OrderByDescending(s => s.IsResolved)
            .ToArray();
    }

    public bool IsResolved => _sources.Any(s => s.IsResolved);

    public string Description => string.Join(" -> ", _sources.Select(s => s.Description));

    public bool Exists(string relativePath)
        => _sources.Any(s => s.IsResolved && s.Exists(relativePath));

    public async Task<string> ReadAllTextAsync(string relativePath, CancellationToken cancellationToken = default)
    {
        foreach (var source in _sources)
        {
            if (source.IsResolved && source.Exists(relativePath))
            {
                return await source.ReadAllTextAsync(relativePath, cancellationToken);
            }
        }

        throw new FileNotFoundException(
            $"Knowledge document '{relativePath}' was not found in any source ({Description}).");
    }

    public IReadOnlyList<string> ListFiles(string relativeDirectory, string searchPattern = "*.md")
    {
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var result = new List<string>();

        foreach (var source in _sources.Where(s => s.IsResolved))
        {
            foreach (var file in source.ListFiles(relativeDirectory, searchPattern))
            {
                if (seen.Add(file))
                {
                    result.Add(file);
                }
            }
        }

        result.Sort(StringComparer.OrdinalIgnoreCase);
        return result;
    }
}
