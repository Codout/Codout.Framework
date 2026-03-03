using System;

namespace Codout.Framework.Storage;

/// <summary>
/// Represents a storage item (file/blob) in a container
/// </summary>
public class StorageItem
{
    /// <summary>
    /// Gets or sets the file name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the full URI of the file
    /// </summary>
    public Uri Uri { get; set; } = null!;

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
    /// Gets or sets the ETag for concurrency control
    /// </summary>
    public string? ETag { get; set; }

    /// <summary>
    /// Gets or sets whether this item is a directory/folder (for hierarchical storage)
    /// </summary>
    public bool IsDirectory { get; set; }
}
