namespace Codout.Framework.Mcp.Models;

public sealed record RepositoryStatus(
    bool DocsRootResolved,
    string DocsRoot,
    int StaticDocumentCount,
    int GoldReferenceCount);
