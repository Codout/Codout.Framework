using System;
using Codout.Framework.DAL.Repository;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;

namespace Codout.Framework.Mongo.Configuration;

public static class ConfigureServices
{
    public static IServiceCollection AddMongoDb(this IServiceCollection services, string connectionString, string databaseName)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentNullException(nameof(connectionString), "Connection string cannot be null or empty.");

        if (string.IsNullOrWhiteSpace(databaseName))
            throw new ArgumentNullException(nameof(databaseName), "Database name cannot be null or empty.");

        services.AddSingleton<IMongoClient>(provider =>
            new MongoClient(connectionString));

        services.AddSingleton<IMongoDatabase>(provider =>
        {
            var client = provider.GetRequiredService<IMongoClient>();
            return client.GetDatabase(databaseName);
        });

        services.AddScoped<MongoDbContext>();
        services.AddScoped(typeof(IRepository<>), typeof(MongoRepository<>));

        return services;
    }
}