using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Codout.Framework.Storage;

/// <summary>
/// Defines the contract for cloud storage operations
/// </summary>
public interface IStorage
{
    #region Upload Operations

    /// <summary>
    /// Uploads a file to the specified container
    /// </summary>
    /// <param name="file">The file stream to upload</param>
    /// <param name="container">The container name</param>
    /// <param name="fileName">The file name</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The URI of the uploaded file</returns>
    Task<Uri> UploadAsync(Stream file, string container, string fileName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Uploads a file with custom metadata
    /// </summary>
    Task<Uri> UploadAsync(Stream file, string container, string fileName, IDictionary<string, string>? metadata, CancellationToken cancellationToken = default);

    /// <summary>
    /// Uploads a file with progress reporting
    /// </summary>
    Task<Uri> UploadAsync(Stream file, string container, string fileName, IProgress<long>? progress, CancellationToken cancellationToken = default);

    #endregion

    #region Download Operations

    /// <summary>
    /// Downloads a file from the specified container
    /// </summary>
    Task<Stream> DownloadAsync(string container, string fileName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a read-only stream for the file
    /// </summary>
    Task<Stream> GetStreamAsync(string container, string fileName, CancellationToken cancellationToken = default);

    #endregion

    #region Delete Operations

    /// <summary>
    /// Deletes a file from the specified container
    /// </summary>
    Task DeleteAsync(string container, string fileName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes multiple files from the specified container
    /// </summary>
    Task DeleteManyAsync(string container, IEnumerable<string> fileNames, CancellationToken cancellationToken = default);

    #endregion

    #region Copy/Move Operations

    /// <summary>
    /// Moves a file from one container to another
    /// </summary>
    Task<Uri> MoveToAsync(string fromContainer, string toContainer, string fileName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Copies a file from one container to another
    /// </summary>
    Task<Uri> CopyToAsync(string fromContainer, string toContainer, string fileName, CancellationToken cancellationToken = default);

    #endregion

    #region Query Operations

    /// <summary>
    /// Checks if a file exists in the specified container
    /// </summary>
    Task<bool> ExistsAsync(string container, string fileName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists all files in the specified container
    /// </summary>
    Task<IEnumerable<StorageItem>> ListAsync(string container, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists files in the specified container with a prefix filter
    /// </summary>
    Task<IEnumerable<StorageItem>> ListAsync(string container, string? prefix, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the metadata for a specific file
    /// </summary>
    Task<StorageMetadata> GetMetadataAsync(string container, string fileName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sets custom metadata for a specific file
    /// </summary>
    Task SetMetadataAsync(string container, string fileName, IDictionary<string, string> metadata, CancellationToken cancellationToken = default);

    #endregion

    #region URI Operations

    /// <summary>
    /// Gets the URI for a specific blob
    /// </summary>
    Uri GetBlobUri(string container, string fileName);

    /// <summary>
    /// Gets a SAS (Shared Access Signature) URI with temporary access
    /// </summary>
    Task<Uri> GetSasUriAsync(string container, string fileName, TimeSpan expiresIn, CancellationToken cancellationToken = default);

    #endregion
}