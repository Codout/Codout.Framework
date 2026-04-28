using Codout.Framework.Mcp.Services;
using Xunit;

namespace Codout.Framework.Mcp.Tests;

public class CompositeKnowledgeSourceTests
{
    [Fact]
    public void Composite_falls_back_to_embedded_when_filesystem_unresolved()
    {
        var fs = new FileSystemKnowledgeSource(string.Empty);
        var embedded = new EmbeddedKnowledgeSource(typeof(EmbeddedKnowledgeSource).Assembly);
        var composite = new CompositeKnowledgeSource(new IKnowledgeSource[] { fs, embedded });

        Assert.False(fs.IsResolved);
        Assert.True(embedded.IsResolved);
        Assert.True(composite.IsResolved);
        Assert.True(composite.Exists("constitution/ui-crud.md"));
    }

    [Fact]
    public async Task Composite_reads_from_first_resolved_source()
    {
        var fs = new FileSystemKnowledgeSource(string.Empty);
        var embedded = new EmbeddedKnowledgeSource(typeof(EmbeddedKnowledgeSource).Assembly);
        var composite = new CompositeKnowledgeSource(new IKnowledgeSource[] { fs, embedded });

        var content = await composite.ReadAllTextAsync("constitution/ui-crud.md");
        Assert.False(string.IsNullOrWhiteSpace(content));
    }

    [Fact]
    public async Task Composite_throws_for_missing_path_across_all_sources()
    {
        var fs = new FileSystemKnowledgeSource(string.Empty);
        var embedded = new EmbeddedKnowledgeSource(typeof(EmbeddedKnowledgeSource).Assembly);
        var composite = new CompositeKnowledgeSource(new IKnowledgeSource[] { fs, embedded });

        await Assert.ThrowsAsync<FileNotFoundException>(
            () => composite.ReadAllTextAsync("nonexistent/path.md"));
    }
}
