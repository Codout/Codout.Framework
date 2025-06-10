using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;

namespace Codout.Framework.Mongo;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCodoutMongoDb(this IServiceCollection services, IConfiguration configuration)
    {
        var mongoDbSettings = configuration.GetSection("MongoDbSettings").Get<MongoDbSettings>();
        if (mongoDbSettings == null)
        {
            throw new ArgumentNullException(nameof(mongoDbSettings), "MongoDbSettings section is missing in the configuration.");
        }
        services.AddSingleton(mongoDbSettings);
        services.AddSingleton<IMongoClient>(sp => new MongoClient(mongoDbSettings.ConnectionString));
        services.AddScoped<IMongoDatabase>(sp => sp.GetRequiredService<IMongoClient>().GetDatabase(mongoDbSettings.DatabaseName));
        return services;
    }
}