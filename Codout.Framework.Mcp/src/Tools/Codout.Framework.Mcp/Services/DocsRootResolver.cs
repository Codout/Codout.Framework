using Codout.Framework.Mcp.Options;
using Microsoft.Extensions.Options;

namespace Codout.Framework.Mcp.Services;

public sealed class DocsRootResolver : IDocsRootResolver
{
    private readonly CodoutAiOptions _options;

    public DocsRootResolver(IOptions<CodoutAiOptions> options)
    {
        _options = options.Value;
    }

    public string Resolve()
    {
        var candidates = new List<string>();

        if (!string.IsNullOrWhiteSpace(_options.DocsRoot))
        {
            candidates.Add(_options.DocsRoot!);
        }

        var cwd = Directory.GetCurrentDirectory();
        var baseDir = AppContext.BaseDirectory;

        candidates.Add(Path.Combine(cwd, "docs", "ai"));
        candidates.Add(Path.Combine(baseDir, "docs", "ai"));

        foreach (var root in EnumerateAncestors(cwd).Concat(EnumerateAncestors(baseDir)).Distinct(StringComparer.OrdinalIgnoreCase))
        {
            candidates.Add(Path.Combine(root, "docs", "ai"));
            candidates.Add(Path.Combine(root, "Codout.Club", "docs", "ai"));

            var parent = Directory.GetParent(root)?.FullName;
            if (!string.IsNullOrWhiteSpace(parent))
            {
                candidates.Add(Path.Combine(parent, "Codout.Club", "docs", "ai"));
                candidates.Add(Path.Combine(parent, "Codout.Club-master", "docs", "ai"));
            }
        }

        foreach (var candidate in candidates.Where(x => !string.IsNullOrWhiteSpace(x)))
        {
            var full = Path.GetFullPath(candidate);
            if (Directory.Exists(full))
            {
                return full;
            }
        }

        return Path.GetFullPath(Path.Combine(cwd, "docs", "ai"));
    }

    private static IEnumerable<string> EnumerateAncestors(string path)
    {
        var current = new DirectoryInfo(Path.GetFullPath(path));
        while (current is not null)
        {
            yield return current.FullName;
            current = current.Parent;
        }
    }
}
