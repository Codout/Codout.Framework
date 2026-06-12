using Codout.Framework.Storage.Configuration;
using FluentAssertions;
using Xunit;

namespace Codout.Framework.Storage.Tests;

public class StorageOptionsTests
{
    [Fact]
    public void StorageOptions_DeveTerDefaultsSensatos()
    {
        var options = new StorageOptions();

        options.ConnectionString.Should().BeNull();
        options.DefaultContainer.Should().BeNull();
        options.AutoCreateContainer.Should().BeTrue();
        options.MaxRetryAttempts.Should().Be(3);
        options.RetryDelaySeconds.Should().Be(2);
        options.EnableCdn.Should().BeFalse();
        options.CdnEndpoint.Should().BeNull();
        options.DefaultSasExpirationHours.Should().Be(24);
        options.ValidateFileNames.Should().BeTrue();
        options.MaxFileSizeBytes.Should().Be(0);
    }

    [Fact]
    public void AzureStorageOptions_DeveHerdarDeStorageOptionsComDefaults()
    {
        var options = new AzureStorageOptions();

        options.Should().BeAssignableTo<StorageOptions>();
        options.AccountName.Should().BeNull();
        options.AccountKey.Should().BeNull();
        options.UseManagedIdentity.Should().BeFalse();
        options.PublicAccessType.Should().Be("Blob");
    }

    [Fact]
    public void AwsStorageOptions_DeveTerRegiaoEEncriptacaoPadrao()
    {
        var options = new AwsStorageOptions();

        options.Should().BeAssignableTo<StorageOptions>();
        options.Region.Should().Be("us-east-1");
        options.UseServerSideEncryption.Should().BeTrue();
        options.BucketName.Should().BeNull();
    }

    [Fact]
    public void FileSystemStorageOptions_DeveTerRootPathPadrao()
    {
        // Observação: existe FileSystemStorageOptions mas não há implementação
        // IStorage de file system no repositório (veja tests/FINDINGS-C.md).
        var options = new FileSystemStorageOptions();

        options.RootPath.Should().Be("./storage");
        options.BaseUrl.Should().BeNull();
    }
}
