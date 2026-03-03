using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Codout.Framework.EF.Interceptors;

namespace Codout.Framework.EF;

/// <summary>
/// Builder fluente para configuração avançada do Entity Framework Core
/// </summary>
public class EFCoreBuilder<TContext> where TContext : DbContext
{
    private readonly IServiceCollection _services;
    private readonly IConfiguration _configuration;
    private Action<DbContextOptionsBuilder>? _configureOptions;
    private readonly List<IInterceptor> _interceptors = [];
    private ServiceLifetime _lifetime = ServiceLifetime.Scoped;
    private bool _enableSensitiveDataLogging;
    private bool _enableDetailedErrors;
    private bool _enableRetryOnFailure;
    private int _maxRetryCount = 3;
    private TimeSpan _maxRetryDelay = TimeSpan.FromSeconds(30);
    private string? _connectionString;

    internal EFCoreBuilder(IServiceCollection services, IConfiguration configuration)
    {
        _services = services ?? throw new ArgumentNullException(nameof(services));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
    }

    /// <summary>
    /// Define a connection string manualmente
    /// </summary>
    public EFCoreBuilder<TContext> WithConnectionString(string connectionString)
    {
        _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        return this;
    }

    /// <summary>
    /// Carrega a connection string da configuração
    /// </summary>
    public EFCoreBuilder<TContext> WithConnectionStringFromConfiguration(string key = "DefaultConnection")
    {
        _connectionString = _configuration.GetConnectionString(key);
        if (string.IsNullOrEmpty(_connectionString))
            throw new InvalidOperationException($"Connection string '{key}' não encontrada na configuração.");
        return this;
    }

    /// <summary>
    /// Usa SQL Server como provider
    /// </summary>
    public EFCoreBuilder<TContext> UseSqlServer(Action<Microsoft.EntityFrameworkCore.Infrastructure.SqlServerDbContextOptionsBuilder>? sqlServerOptionsAction = null)
    {
        _configureOptions = options =>
        {
            if (string.IsNullOrEmpty(_connectionString))
                throw new InvalidOperationException("Connection string não configurada. Use WithConnectionString() primeiro.");

            options.UseSqlServer(_connectionString, sqlOptions =>
            {
                if (_enableRetryOnFailure)
                {
                    sqlOptions.EnableRetryOnFailure(
                        maxRetryCount: _maxRetryCount,
                        maxRetryDelay: _maxRetryDelay,
                        errorNumbersToAdd: null);
                }

                sqlServerOptionsAction?.Invoke(sqlOptions);
            });
        };
        return this;
    }

    /// <summary>
    /// Adiciona interceptor customizado
    /// </summary>
    public EFCoreBuilder<TContext> AddInterceptor<TInterceptor>() where TInterceptor : class, IInterceptor
    {
        _services.AddSingleton<TInterceptor>();
        return this;
    }

    /// <summary>
    /// Habilita auditoria automática
    /// </summary>
    public EFCoreBuilder<TContext> EnableAuditing()
    {
        _services.AddSingleton<AuditableInterceptor>();
        return this;
    }

    /// <summary>
    /// Habilita soft delete automático
    /// </summary>
    public EFCoreBuilder<TContext> EnableSoftDelete()
    {
        _services.AddSingleton<SoftDeleteInterceptor>();
        return this;
    }

    /// <summary>
    /// Habilita logging de dados sensíveis (apenas desenvolvimento)
    /// </summary>
    public EFCoreBuilder<TContext> EnableSensitiveDataLogging()
    {
        _enableSensitiveDataLogging = true;
        return this;
    }

    /// <summary>
    /// Habilita erros detalhados
    /// </summary>
    public EFCoreBuilder<TContext> EnableDetailedErrors()
    {
        _enableDetailedErrors = true;
        return this;
    }

    /// <summary>
    /// Habilita retry automático em falhas
    /// </summary>
    public EFCoreBuilder<TContext> EnableRetryOnFailure(int maxRetryCount = 3, int maxRetryDelaySeconds = 30)
    {
        _enableRetryOnFailure = true;
        _maxRetryCount = maxRetryCount;
        _maxRetryDelay = TimeSpan.FromSeconds(maxRetryDelaySeconds);
        return this;
    }

    /// <summary>
    /// Define o lifetime do DbContext
    /// </summary>
    public EFCoreBuilder<TContext> WithLifetime(ServiceLifetime lifetime)
    {
        _lifetime = lifetime;
        return this;
    }

    /// <summary>
    /// Configuração customizada do DbContextOptionsBuilder
    /// </summary>
    public EFCoreBuilder<TContext> ConfigureOptions(Action<DbContextOptionsBuilder> configure)
    {
        var previousConfig = _configureOptions;
        _configureOptions = options =>
        {
            previousConfig?.Invoke(options);
            configure(options);
        };
        return this;
    }

    /// <summary>
    /// Finaliza a configuração e registra o DbContext
    /// </summary>
    public IServiceCollection Build()
    {
        if (_configureOptions == null)
            throw new InvalidOperationException("Provider não configurado. Use UseSqlServer() ou outro provider.");

        _services.AddDbContext<TContext>((serviceProvider, options) =>
        {
            _configureOptions(options);

            if (_enableSensitiveDataLogging)
                options.EnableSensitiveDataLogging();

            if (_enableDetailedErrors)
                options.EnableDetailedErrors();

            // Adiciona interceptors registrados
            var auditableInterceptor = serviceProvider.GetService<AuditableInterceptor>();
            if (auditableInterceptor != null)
                options.AddInterceptors(auditableInterceptor);

            var softDeleteInterceptor = serviceProvider.GetService<SoftDeleteInterceptor>();
            if (softDeleteInterceptor != null)
                options.AddInterceptors(softDeleteInterceptor);

        }, _lifetime);

        return _services;
    }
}

/// <summary>
/// Extensões do ServiceCollection para o builder
/// </summary>
public static class EFCoreBuilderExtensions
{
    /// <summary>
    /// Adiciona Entity Framework Core com configuração fluente
    /// </summary>
    public static EFCoreBuilder<TContext> AddEFCore<TContext>(
        this IServiceCollection services,
        IConfiguration configuration) where TContext : DbContext
    {
        return new EFCoreBuilder<TContext>(services, configuration);
    }
}
