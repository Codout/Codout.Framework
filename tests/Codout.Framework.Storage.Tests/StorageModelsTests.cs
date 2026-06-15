using FluentAssertions;
using Xunit;

namespace Codout.Framework.Storage.Tests;

public class StorageModelsTests
{
    [Fact]
    public void StorageItem_DeveTerDefaultsSeguros()
    {
        var item = new StorageItem();

        item.Name.Should().BeEmpty();
        item.ContentType.Should().BeEmpty();
        item.Size.Should().Be(0);
        item.ETag.Should().BeNull();
        item.IsDirectory.Should().BeFalse();
        item.LastModified.Should().Be(default);
    }

    [Fact]
    public void StorageItem_DevePermitirPreencherTodasAsPropriedades()
    {
        var agora = DateTimeOffset.UtcNow;
        var uri = new Uri("https://conta.blob.core.windows.net/c/f.txt");

        var item = new StorageItem
        {
            Name = "f.txt",
            Uri = uri,
            ContentType = "text/plain",
            Size = 42,
            LastModified = agora,
            ETag = "\"abc\"",
            IsDirectory = false
        };

        item.Name.Should().Be("f.txt");
        item.Uri.Should().BeSameAs(uri);
        item.ContentType.Should().Be("text/plain");
        item.Size.Should().Be(42);
        item.LastModified.Should().Be(agora);
        item.ETag.Should().Be("\"abc\"");
    }

    [Fact]
    public void StorageMetadata_DeveInicializarCustomMetadataVazio()
    {
        var metadata = new StorageMetadata();

        metadata.CustomMetadata.Should().NotBeNull().And.BeEmpty();
        metadata.ContentType.Should().BeEmpty();
        metadata.Size.Should().Be(0);
        metadata.ETag.Should().BeNull();
        metadata.ContentEncoding.Should().BeNull();
        metadata.CacheControl.Should().BeNull();
    }

    [Fact]
    public void StorageMetadata_DeveAceitarMetadadosCustomizados()
    {
        var metadata = new StorageMetadata();
        metadata.CustomMetadata["autor"] = "codout";

        metadata.CustomMetadata.Should().ContainKey("autor").WhoseValue.Should().Be("codout");
    }
}
