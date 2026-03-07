using Codout.Framework.Mcp.Models;

namespace Codout.Framework.Mcp.Services;

public interface IAiKnowledgeRepository
{
    Task<KnowledgeDocument> GetDocumentAsync(string key, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<KnowledgeDocument>> GetAllDocumentsAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<GoldReferenceMatch>> FindGoldReferencesAsync(string query, int take = 5, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<string>> ListGoldReferenceNamesAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<KnowledgeDocument>> SearchAsync(string query, int take = 5, CancellationToken cancellationToken = default);
    Task<RepositoryStatus> GetStatusAsync(CancellationToken cancellationToken = default);
}
