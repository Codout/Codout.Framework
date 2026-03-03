using System;

namespace Codout.Framework.Storage.Exceptions;

/// <summary>
/// Base exception for storage operations
/// </summary>
public class StorageException : Exception
{
    public string? Container { get; set; }
    public string? FileName { get; set; }

    public StorageException(string message) : base(message)
    {
    }

    public StorageException(string message, Exception innerException) : base(message, innerException)
    {
    }

    public StorageException(string message, string container, string fileName) : base(message)
    {
        Container = container;
        FileName = fileName;
    }

    public StorageException(string message, string container, string fileName, Exception innerException) 
        : base(message, innerException)
    {
        Container = container;
        FileName = fileName;
    }
}

/// <summary>
/// Exception thrown when a file is not found in storage
/// </summary>
public class StorageNotFoundException : StorageException
{
    public StorageNotFoundException(string container, string fileName)
        : base($"File '{fileName}' not found in container '{container}'.", container, fileName)
    {
    }

    public StorageNotFoundException(string container, string fileName, Exception innerException)
        : base($"File '{fileName}' not found in container '{container}'.", container, fileName, innerException)
    {
    }
}

/// <summary>
/// Exception thrown when attempting to upload a file that already exists
/// </summary>
public class StorageFileAlreadyExistsException : StorageException
{
    public StorageFileAlreadyExistsException(string container, string fileName)
        : base($"File '{fileName}' already exists in container '{container}'.", container, fileName)
    {
    }
}

/// <summary>
/// Exception thrown when a container operation fails
/// </summary>
public class StorageContainerException : StorageException
{
    public StorageContainerException(string container, string message)
        : base(message, container, string.Empty)
    {
    }

    public StorageContainerException(string container, string message, Exception innerException)
        : base(message, container, string.Empty, innerException)
    {
    }
}

/// <summary>
/// Exception thrown when storage quota is exceeded
/// </summary>
public class StorageQuotaExceededException : StorageException
{
    public long QuotaLimit { get; set; }
    public long CurrentUsage { get; set; }

    public StorageQuotaExceededException(long quotaLimit, long currentUsage)
        : base($"Storage quota exceeded. Limit: {quotaLimit} bytes, Current: {currentUsage} bytes.")
    {
        QuotaLimit = quotaLimit;
        CurrentUsage = currentUsage;
    }
}
