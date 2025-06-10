using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using FluentNHibernate.Cfg;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using NHibernate;
using NHibernate.Cfg;
using NHibernate.Mapping.ByCode;
using NHibernate.Tool.hbm2ddl;

namespace Codout.Framework.NH;

/// <summary>
/// Provedor end-to-end para criar, expor e gerenciar o ciclo de vida do ISessionFactory.
/// Encapsula a inicialização preguiçosa, scan de assemblies configurados e descarte seguro do ISessionFactory.
/// </summary>
public class SessionFactoryProvider : IHostedService, IDisposable
{
    private readonly IConfiguration _configuration;
    private readonly Lazy<ISessionFactory> _factory;

    /// <summary>
    /// Instância preguiçosa do NHibernate ISessionFactory.
    /// </summary>
    public ISessionFactory Factory => _factory.Value;

    /// <summary>
    /// Constrói o provedor usando IConfiguration para localização do XML e mapeamentos por assembly.
    /// </summary>
    public SessionFactoryProvider(IConfiguration configuration)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _factory = new Lazy<ISessionFactory>(BuildFactory, LazyThreadSafetyMode.ExecutionAndPublication);
    }

    /// <summary>
    /// Obtém a fábrica a partir de hibernate.cfg.xml e classes de mapeamento encontradas nos assemblies configurados.
    /// </summary>
    private ISessionFactory BuildFactory()
    {
        var configFile = _configuration.GetValue<string?>("NHibernate:ConfigFile") ?? "hibernate.cfg.xml";
        var xmlPath = Path.IsPathRooted(configFile)
            ? configFile
            : Path.Combine(AppDomain.CurrentDomain.BaseDirectory, configFile);

        if (!File.Exists(xmlPath))
            throw new FileNotFoundException($"Arquivo NHibernate não encontrado: {xmlPath}");

        var cfg = new Configuration();
        cfg.Configure(xmlPath);
        
        // Obter nome da connection string do XML
        var connectionStringName = cfg.Properties[NHibernate.Cfg.Environment.ConnectionStringName];

        if (string.IsNullOrEmpty(connectionStringName))
            throw new InvalidOperationException("Connection string name não encontrada no XML.");

        // Agora carrega a connection string do appsettings.json
        var connectionString = _configuration.GetConnectionString(connectionStringName);

        if (string.IsNullOrEmpty(connectionString))
            throw new InvalidOperationException($"Connection string '{connectionStringName}' não encontrada em appsettings.json.");

        cfg.SetProperty(NHibernate.Cfg.Environment.ConnectionString, connectionString);

        var fluentlyCfg = Fluently.Configure(cfg);

        var assemblyNames = _configuration.GetSection("NHibernate:MappingAssemblies").Get<string[]>();
        var assemblies = assemblyNames.Select(name => Assembly.Load(new AssemblyName(name))).ToArray();

        foreach (var assembly in assemblies)
            fluentlyCfg.Mappings(m => m.FluentMappings.AddFromAssembly(assembly));    

        var autoUpdate = _configuration.GetValue<bool>("NHibernate:AutoUpdateSchema");
        if (autoUpdate)
            new SchemaUpdate(cfg).Execute(useStdOut: true, doUpdate: true);

        return fluentlyCfg.BuildSessionFactory();
    }

    // IHostedService: nenhuma ação na inicialização
    public Task StartAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    // Fecha a fábrica no shutdown
    public Task StopAsync(CancellationToken cancellationToken)
    {
        if (_factory.IsValueCreated)
            _factory.Value.Close();
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        if (_factory.IsValueCreated)
            _factory.Value.Dispose();
    }
}