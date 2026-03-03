using System;
using Codout.Framework.Storage;
using Codout.Framework.Storage.Azure;
using Codout.Framework.Storage.Configuration;
using Microsoft.Extensions.Configuration;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extension methods for configuring Azure Storage services
/// </summary>
public static class AzureStorageServiceCollectionExtensions
{
    /// <summary>
    /// Adds Azure Blob Storage as the IStorage implementation
    /// </summary>
    public static IServiceCollection AddAzureStorage(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("AzureStorage")
            ?? throw new InvalidOperationException("AzureStorage connection string not found in configuration.");

        return services.AddAzureStorage(connectionString);
    }

    /// <summary>
    /// Adds Azure Blob Storage with a connection string
    /// </summary>
    public static IServiceCollection AddAzureStorage(
        this IServiceCollection services,
        string connectionString)
    {
        return services.AddAzureStorage(new AzureStorageOptions
        {
            ConnectionString = connectionString
        });
    }

    /// <summary>
    /// Adds Azure Blob Storage with custom options
    /// </summary>
    public static IServiceCollection AddAzureStorage(
        this IServiceCollection services,
        AzureStorageOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        services.AddSingleton<IStorage>(new AzureStorage(options));

        return services;
    }

    /// <summary>
    /// Adds Azure Blob Storage with configuration builder
    /// </summary>
    public static IServiceCollection AddAzureStorage(
        this IServiceCollection services,
        Action<AzureStorageOptions> configureOptions)
    {
        ArgumentNullException.ThrowIfNull(configureOptions);

        var options = new AzureStorageOptions();
        configureOptions(options);

        return services.AddAzureStorage(options);
    }
}
