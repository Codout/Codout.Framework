using System;
using System.Collections.Generic;

namespace Codout.Framework.Storage;

/// <summary>
/// Represents metadata information for a stored file
/// </summary>
public class StorageMetadata
{
    /// <summary>
    /// Gets or sets the content type (MIME type)
    /// </summary>
    public string ContentType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the file size in bytes
    /// </summary>
    public long Size { get; set; }

    /// <summary>
    /// Gets or sets the last modified date
    /// </summary>
    public DateTimeOffset LastModified { get; set; }

    /// <summary>
    /// Gets or sets the ETag (entity tag) for concurrency control
    /// </summary>
    public string? ETag { get; set; }

    /// <summary>
    /// Gets or sets custom metadata key-value pairs
    /// </summary>
    public IDictionary<string, string> CustomMetadata { get; set; } = new Dictionary<string, string>();

    /// <summary>
    /// Gets or sets the content encoding
    /// </summary>
    public string? ContentEncoding { get; set; }

    /// <summary>
    /// Gets or sets the cache control header
    /// </summary>
    public string? CacheControl { get; set; }
}
