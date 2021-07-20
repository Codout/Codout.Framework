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
        static readonly object _object = new object();

        private const string DefaultHibernateConfig = "hibernate.cfg.xml";

        private static readonly Dictionary<string, ISessionFactory> SessionFactories = new Dictionary<string, ISessionFactory>();

        public FluentConfiguration GetConfiguration(ITenant tenant)
        {
            var assemblyMappingName = tenant.AssemblyMappingName;

            if (string.IsNullOrWhiteSpace(assemblyMappingName))
                throw new InvalidOperationException("AssemblyMappingName não encontrado, por favor defina o assembly que contém os mapeamentos da classe no Web.Config/App.Config ou na classe de implementação da interface ITenant");

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

            var fluentlyCfg = Fluently.Configure(configuration).Mappings(m => m.FluentMappings.AddFromAssembly(Assembly.Load(assemblyMappingName)));

            return fluentlyCfg;
        }

        public ISessionFactory GetSessionFactory(ITenant tenant)
        {
            lock (_object)
            {
                if (SessionFactories.ContainsKey(tenant.TenantKey)) 
                    return SessionFactories[tenant.TenantKey];

                var cfg = GetConfiguration(tenant);

                var sessionFactory = cfg.BuildSessionFactory();

                SessionFactories.Add(tenant.TenantKey, sessionFactory);

                return SessionFactories[tenant.TenantKey];
            }
        }

        public ISession OpenSession(ITenant tenant)
        {
            if (tenant == null)
                throw new Exception($"O objeto ${nameof(ITenant)} não pode ser nulo");

            var session = GetSessionFactory(tenant).OpenSession();

            session.FlushMode = FlushMode.Commit;

            return session;
        }

        public IStatelessSession OpenStatelessSession(ITenant tenant)
        {
            if (tenant == null)
                throw new Exception($"O objeto ${nameof(ITenant)} não pode ser nulo");

            var session = GetSessionFactory(tenant).OpenStatelessSession();

            return session;
        }

    }
}
