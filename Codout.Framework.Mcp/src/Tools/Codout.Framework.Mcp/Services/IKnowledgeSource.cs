namespace Codout.Framework.Mcp.Services;

public interface IKnowledgeSource
{
    bool IsResolved { get; }

    string Description { get; }

    bool Exists(string relativePath);

    Task<string> ReadAllTextAsync(string relativePath, CancellationToken cancellationToken = default);

    IReadOnlyList<string> ListFiles(string relativeDirectory, string searchPattern = "*.md");
}
