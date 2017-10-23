using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Xml;
using Codout.Framework.DAL;
using NHibernate;
using Configuration = NHibernate.Cfg.Configuration;

namespace Codout.Framework.NH
{
    public class SessionFactory
    {
        static readonly object _object = new object();

        public const string DefaultFactoryKey = "nhibernate.current_session";

        private const string DefaultHibernateConfig = "hibernate.cfg.xml";

        private static readonly Dictionary<string, ISessionFactory> SessionFactories = new Dictionary<string, ISessionFactory>();

        public Configuration GetConfiguration(ITenant tenant)
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

            return configuration;
        }

        public ISessionFactory GetSessionFactory(ITenant tenant)
        {
            lock (_object)
            {
                if (!SessionFactories.ContainsKey(tenant.TenantKey))
                {
                    var cfg = GetConfiguration(tenant);

                    var sessionFactory = cfg.BuildSessionFactory();

                    SessionFactories.Add(tenant.TenantKey, sessionFactory);
                }

                return SessionFactories[tenant.TenantKey];
            }
        }

        public ISession CreateSession(IInterceptor interceptor = null, ITenant tenant = null)
        {
            if (tenant == null)
                tenant = new DefaultTenant(ConfigurationManager.AppSettings["AssemblyMappingName"], DefaultFactoryKey, null);

            var session = interceptor != null
                ? GetSessionFactory(tenant).OpenSession(interceptor)
                : GetSessionFactory(tenant).OpenSession();

            session.FlushMode = FlushMode.Commit;

            return session;
        }

        public IStatelessSession CreateStatelessSession(ITenant tenant = null)
        {
            if (tenant == null)
                tenant = new DefaultTenant(ConfigurationManager.AppSettings["AssemblyMappingName"], DefaultFactoryKey, null);

            var session = GetSessionFactory(tenant).OpenStatelessSession();

            return session;
        }

    }
}
