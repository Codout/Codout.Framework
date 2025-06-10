using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NHibernate;

namespace Codout.Framework.NH;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registra serviços NHibernate no DI: ISessionFactory, ISession e orchestrador de lifecycle.
    /// </summary>
    public static IServiceCollection AddNHibernateServices(this IServiceCollection services, IConfiguration configuration)
    {
        if (services == null) throw new ArgumentNullException(nameof(services));
        if (configuration == null) throw new ArgumentNullException(nameof(configuration));

        // Registra provider no DI
        services.AddSingleton<SessionFactoryProvider>();
        // Registra ISessionFactory como singleton
        services.AddSingleton(provider => provider.GetRequiredService<SessionFactoryProvider>().Factory);
        // Registra ISession como scoped
        services.AddScoped(sp => sp.GetRequiredService<ISessionFactory>().OpenSession());
        // Registra IStatelessSession como scoped
        services.AddScoped(sp => sp.GetRequiredService<ISessionFactory>().OpenStatelessSession());
        // Hosted service para cleanup
        services.AddSingleton<IHostedService>(sp => sp.GetRequiredService<SessionFactoryProvider>());

        return services;
    }
}