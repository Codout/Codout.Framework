using System.Reflection;
using System.Text;

namespace Codout.Framework.Mcp.Services;

public sealed class EmbeddedKnowledgeSource : IKnowledgeSource
{
    private const string Prefix = "knowledge/";

    private readonly Assembly _assembly;
    private readonly Dictionary<string, string> _names;

    public EmbeddedKnowledgeSource(Assembly assembly)
    {
        _assembly = assembly;

        _names = assembly.GetManifestResourceNames()
            .Select(original => (Original: original, Normalized: Normalize(original)))
            .Where(x => x.Normalized.StartsWith(Prefix, StringComparison.OrdinalIgnoreCase))
            .ToDictionary(x => x.Normalized, x => x.Original, StringComparer.OrdinalIgnoreCase);
    }

    public bool IsResolved => _names.Count > 0;

    public string Description => $"embedded({_assembly.GetName().Name}; {_names.Count} resources)";

    public bool Exists(string relativePath)
        => _names.ContainsKey(ToResourceKey(relativePath));

    public async Task<string> ReadAllTextAsync(string relativePath, CancellationToken cancellationToken = default)
    {
        var key = ToResourceKey(relativePath);
        if (!_names.TryGetValue(key, out var actual))
        {
            throw new FileNotFoundException($"Embedded knowledge resource '{key}' not found.");
        }

        await using var stream = _assembly.GetManifestResourceStream(actual)
            ?? throw new FileNotFoundException($"Embedded knowledge resource '{actual}' could not be opened.");

        using var reader = new StreamReader(stream, Encoding.UTF8);
        return await reader.ReadToEndAsync(cancellationToken);
    }

    public IReadOnlyList<string> ListFiles(string relativeDirectory, string searchPattern = "*.md")
    {
        var dirKey = Prefix + Normalize(relativeDirectory).TrimEnd('/');
        if (dirKey.Length > 0 && !dirKey.EndsWith('/'))
        {
            dirKey += "/";
        }

        var extension = searchPattern.StartsWith("*.", StringComparison.Ordinal)
            ? searchPattern[1..]
            : null;

        var matches = new List<string>();
        foreach (var key in _names.Keys)
        {
            if (!key.StartsWith(dirKey, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var rest = key[dirKey.Length..];
            if (rest.Contains('/'))
            {
                continue;
            }

            if (extension is not null && !rest.EndsWith(extension, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            matches.Add(rest);
        }

        matches.Sort(StringComparer.OrdinalIgnoreCase);
        return matches;
    }

    private static string ToResourceKey(string relativePath)
        => Prefix + Normalize(relativePath).TrimStart('/');

    private static string Normalize(string value)
        => (value ?? string.Empty).Replace('\\', '/');
}
