using System.Text;

namespace Codout.Framework.Mcp.Services;

public sealed class FileSystemKnowledgeSource : IKnowledgeSource
{
    private readonly string _root;

    public FileSystemKnowledgeSource(string root)
    {
        _root = string.IsNullOrWhiteSpace(root) ? string.Empty : Path.GetFullPath(root);
    }

    public bool IsResolved => !string.IsNullOrWhiteSpace(_root) && Directory.Exists(_root);

    public string Description => string.IsNullOrWhiteSpace(_root) ? "filesystem(<unset>)" : $"filesystem({_root})";

    public bool Exists(string relativePath)
        => IsResolved && File.Exists(Path.Combine(_root, NormalizeForFs(relativePath)));

    public async Task<string> ReadAllTextAsync(string relativePath, CancellationToken cancellationToken = default)
    {
        if (!IsResolved)
        {
            throw new DirectoryNotFoundException($"DocsRoot '{_root}' is not available.");
        }

        var fullPath = Path.Combine(_root, NormalizeForFs(relativePath));
        if (!File.Exists(fullPath))
        {
            throw new FileNotFoundException($"Knowledge document not found at '{fullPath}'.");
        }

        return await File.ReadAllTextAsync(fullPath, Encoding.UTF8, cancellationToken);
    }

    public IReadOnlyList<string> ListFiles(string relativeDirectory, string searchPattern = "*.md")
    {
        if (!IsResolved)
        {
            return Array.Empty<string>();
        }

        var dir = Path.Combine(_root, NormalizeForFs(relativeDirectory));
        if (!Directory.Exists(dir))
        {
            return Array.Empty<string>();
        }

        return Directory.EnumerateFiles(dir, searchPattern, SearchOption.TopDirectoryOnly)
            .Select(Path.GetFileName)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x!)
            .OrderBy(x => x, StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    private static string NormalizeForFs(string relativePath)
        => relativePath.Replace('/', Path.DirectorySeparatorChar).Replace('\\', Path.DirectorySeparatorChar);
}
