namespace Codout.Framework.Mcp.Models;

public sealed record KnowledgeDocument(
    string Key,
    string Title,
    string Category,
    string RelativePath,
    string Content);
