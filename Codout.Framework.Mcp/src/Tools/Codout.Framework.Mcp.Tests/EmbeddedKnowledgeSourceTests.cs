using System.Reflection;
using Codout.Framework.Mcp.Services;
using Xunit;

namespace Codout.Framework.Mcp.Tests;

public class EmbeddedKnowledgeSourceTests
{
    private static EmbeddedKnowledgeSource CreateSource()
    {
        var assembly = typeof(EmbeddedKnowledgeSource).Assembly;
        return new EmbeddedKnowledgeSource(assembly);
    }

    [Fact]
    public void Source_resolves_when_resources_are_embedded()
    {
        var source = CreateSource();
        Assert.True(source.IsResolved, $"Embedded source not resolved. Description: {source.Description}");
    }

    [Theory]
    [InlineData("constitution/ui-crud.md")]
    [InlineData("catalog/components.md")]
    [InlineData("catalog/screen-patterns.md")]
    [InlineData("catalog/anti-patterns.md")]
    [InlineData("catalog/decision-flow.md")]
    [InlineData("recipes/crud-standard.md")]
    [InlineData("recipes/grid-standard.md")]
    [InlineData("recipes/form-simple.md")]
    [InlineData("recipes/form-complex.md")]
    [InlineData("recipes/details-actions.md")]
    [InlineData("recipes/authorization.md")]
    [InlineData("recipes/layout.md")]
    public async Task Static_documents_are_embedded_and_readable(string relativePath)
    {
        var source = CreateSource();
        Assert.True(source.Exists(relativePath), $"Resource '{relativePath}' is not embedded.");

        var content = await source.ReadAllTextAsync(relativePath);
        Assert.False(string.IsNullOrWhiteSpace(content), $"Resource '{relativePath}' is empty.");
    }

    [Fact]
    public void Gold_references_are_listed()
    {
        var source = CreateSource();
        var files = source.ListFiles("references/gold", "*.md");
        Assert.NotEmpty(files);
    }

    [Fact]
    public void Search_pattern_filters_files()
    {
        var source = CreateSource();
        var mdFiles = source.ListFiles("catalog", "*.md");
        var jsonFiles = source.ListFiles("catalog", "*.json");

        Assert.NotEmpty(mdFiles);
        Assert.All(mdFiles, f => Assert.EndsWith(".md", f));
        Assert.All(jsonFiles, f => Assert.EndsWith(".json", f));
    }
}
