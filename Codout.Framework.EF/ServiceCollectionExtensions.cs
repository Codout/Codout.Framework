using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Codout.Framework.EF;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddEFCore<T>(this IServiceCollection services, IConfiguration configuration) where T : DbContext
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrEmpty(connectionString))
        {
            throw new ArgumentNullException(nameof(connectionString), "DefaultConnection string is missing in the configuration.");
        }
        services.AddDbContext<T>(options => options.UseSqlServer(connectionString));
        return services;
    }
}