using Codout.Framework.Mcp.Services;
using Xunit;

namespace Codout.Framework.Mcp.Tests;

public class AiKnowledgeRepositoryTests
{
    private static AiKnowledgeRepository CreateRepository()
    {
        var embedded = new EmbeddedKnowledgeSource(typeof(EmbeddedKnowledgeSource).Assembly);
        var composite = new CompositeKnowledgeSource(new IKnowledgeSource[] { embedded });
        return new AiKnowledgeRepository(composite);
    }

    [Theory]
    [InlineData("ui-constitution")]
    [InlineData("components")]
    [InlineData("screen-patterns")]
    [InlineData("anti-patterns")]
    [InlineData("decision-flow")]
    [InlineData("crud-recipe")]
    [InlineData("grid-recipe")]
    [InlineData("form-simple-recipe")]
    [InlineData("form-complex-recipe")]
    [InlineData("details-actions-recipe")]
    [InlineData("authorization-recipe")]
    [InlineData("layout-recipe")]
    public async Task Static_documents_resolve_with_content(string key)
    {
        var repo = CreateRepository();
        var doc = await repo.GetDocumentAsync(key);

        Assert.Equal(key, doc.Key);
        Assert.False(string.IsNullOrWhiteSpace(doc.Content), $"Document '{key}' is empty.");
        Assert.False(string.IsNullOrWhiteSpace(doc.RelativePath));
    }

    [Fact]
    public async Task Gold_references_list_is_non_empty()
    {
        var repo = CreateRepository();
        var names = await repo.ListGoldReferenceNamesAsync();
        Assert.NotEmpty(names);
    }

    [Fact]
    public async Task Each_gold_reference_loads()
    {
        var repo = CreateRepository();
        var names = await repo.ListGoldReferenceNamesAsync();

        foreach (var name in names)
        {
            var doc = await repo.GetDocumentAsync($"gold:{name}");
            Assert.False(string.IsNullOrWhiteSpace(doc.Content), $"Gold reference '{name}' is empty.");
        }
    }

    [Fact]
    public async Task Status_reports_resolved_with_counts()
    {
        var repo = CreateRepository();
        var status = await repo.GetStatusAsync();

        Assert.True(status.DocsRootResolved);
        Assert.True(status.StaticDocumentCount > 0);
        Assert.True(status.GoldReferenceCount > 0);
    }

    [Fact]
    public async Task Search_returns_matches_for_known_term()
    {
        var repo = CreateRepository();
        var results = await repo.SearchAsync("crud", take: 5);
        Assert.NotEmpty(results);
    }

    [Fact]
    public async Task FindGoldReferences_returns_matches_for_known_query()
    {
        var repo = CreateRepository();
        var results = await repo.FindGoldReferencesAsync("financeiro", take: 3);
        Assert.NotNull(results);
    }

    [Fact]
    public async Task Unknown_key_throws()
    {
        var repo = CreateRepository();
        await Assert.ThrowsAsync<FileNotFoundException>(
            () => repo.GetDocumentAsync("does-not-exist"));
    }
}
