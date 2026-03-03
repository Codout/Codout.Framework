namespace Codout.Framework.Storage.Configuration;

/// <summary>
/// Configuration options for storage providers
/// </summary>
public class StorageOptions
{
    /// <summary>
    /// Gets or sets the connection string for the storage provider
    /// </summary>
    public string? ConnectionString { get; set; }

    /// <summary>
    /// Gets or sets the default container name
    /// </summary>
    public string? DefaultContainer { get; set; }

    /// <summary>
    /// Gets or sets whether to create containers automatically if they don't exist
    /// </summary>
    public bool AutoCreateContainer { get; set; } = true;

    /// <summary>
    /// Gets or sets the maximum retry attempts for failed operations
    /// </summary>
    public int MaxRetryAttempts { get; set; } = 3;

    /// <summary>
    /// Gets or sets the delay between retry attempts in seconds
    /// </summary>
    public int RetryDelaySeconds { get; set; } = 2;

    /// <summary>
    /// Gets or sets whether to enable CDN for blob URLs
    /// </summary>
    public bool EnableCdn { get; set; }

    /// <summary>
    /// Gets or sets the CDN endpoint URL
    /// </summary>
    public string? CdnEndpoint { get; set; }

    /// <summary>
    /// Gets or sets the default SAS token expiration time in hours
    /// </summary>
    public int DefaultSasExpirationHours { get; set; } = 24;

    /// <summary>
    /// Gets or sets whether to validate file names
    /// </summary>
    public bool ValidateFileNames { get; set; } = true;

    /// <summary>
    /// Gets or sets the maximum file size in bytes (0 = unlimited)
    /// </summary>
    public long MaxFileSizeBytes { get; set; }
}

/// <summary>
/// Azure-specific storage options
/// </summary>
public class AzureStorageOptions : StorageOptions
{
    /// <summary>
    /// Gets or sets the Azure Storage account name
    /// </summary>
    public string? AccountName { get; set; }

    /// <summary>
    /// Gets or sets the Azure Storage account key
    /// </summary>
    public string? AccountKey { get; set; }

    /// <summary>
    /// Gets or sets whether to use Azure Managed Identity
    /// </summary>
    public bool UseManagedIdentity { get; set; }

    /// <summary>
    /// Gets or sets the default public access type for containers
    /// </summary>
    public string PublicAccessType { get; set; } = "Blob";
}

/// <summary>
/// AWS S3-specific storage options
/// </summary>
public class AwsStorageOptions : StorageOptions
{
    /// <summary>
    /// Gets or sets the AWS access key ID
    /// </summary>
    public string? AccessKeyId { get; set; }

    /// <summary>
    /// Gets or sets the AWS secret access key
    /// </summary>
    public string? SecretAccessKey { get; set; }

    /// <summary>
    /// Gets or sets the AWS region
    /// </summary>
    public string Region { get; set; } = "us-east-1";

    /// <summary>
    /// Gets or sets the S3 bucket name
    /// </summary>
    public string? BucketName { get; set; }

    /// <summary>
    /// Gets or sets whether to use server-side encryption
    /// </summary>
    public bool UseServerSideEncryption { get; set; } = true;
}

/// <summary>
/// File system storage options (for development)
/// </summary>
public class FileSystemStorageOptions : StorageOptions
{
    /// <summary>
    /// Gets or sets the root path for file storage
    /// </summary>
    public string RootPath { get; set; } = "./storage";

    /// <summary>
    /// Gets or sets the base URL for generating file URIs
    /// </summary>
    public string? BaseUrl { get; set; }
}
