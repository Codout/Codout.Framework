using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Xml;
using Codout.Framework.DAL;
using FluentNHibernate.Cfg;
using NHibernate;
using Configuration = NHibernate.Cfg.Configuration;

namespace Codout.Framework.NH
{
    public class SessionFactory
    {
        private static readonly object _object = new object();

        private static readonly Dictionary<string, ISessionFactory> SessionFactories = new Dictionary<string, ISessionFactory>();

        private const string DefaultHibernateConfig = "hibernate.cfg.xml";

        public ITenant Tenant { get; }

        public SessionFactory(ITenant tenant)
        {
            Tenant = tenant;
        }

        private FluentConfiguration GetConfiguration(ITenant tenant)
        {
            var assemblyMappingName = tenant.AssemblyMappingName;

            if (string.IsNullOrWhiteSpace(assemblyMappingName))
                throw new InvalidOperationException("A propriedade AssemblyMappingName em ITenant não foi informada, por favor informe o nome do assembly que contém os mapeamentos ORM");

            var configuration = new Configuration();
            var hibernateConfig = DefaultHibernateConfig;

            if (Path.IsPathRooted(hibernateConfig) == false)
                hibernateConfig = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, hibernateConfig);

            if (File.Exists(hibernateConfig))
                configuration.Configure(new XmlTextReader(hibernateConfig));

            if (!string.IsNullOrWhiteSpace(tenant.ConnectionString))
            {
                configuration.SetProperty("connection.connection_string", tenant.ConnectionString);
                if (configuration.Properties.ContainsKey("connection.connection_string_name"))
                    configuration.Properties.Remove("connection.connection_string_name");
            }

            var fluentlyCfg = Fluently.Configure(configuration)
                .Mappings(m => m.FluentMappings.AddFromAssembly(Assembly.Load(assemblyMappingName)));

            return fluentlyCfg;
        }

        private ISessionFactory GetSessionFactory()
        {
            lock (_object)
            {
                if (SessionFactories.ContainsKey(Tenant.TenantKey)) 
                    return SessionFactories[Tenant.TenantKey];

                var cfg = GetConfiguration(Tenant);

                var sessionFactory = cfg.BuildSessionFactory();

                SessionFactories.Add(Tenant.TenantKey, sessionFactory);

                return SessionFactories[Tenant.TenantKey];
            }
        }

        public ISession OpenSession()
        {
            var session = GetSessionFactory().OpenSession();

            session.FlushMode = FlushMode.Commit;

            return session;
        }

        public IStatelessSession OpenStatelessSession()
        {
            return GetSessionFactory().OpenStatelessSession();
        }

    }
}
