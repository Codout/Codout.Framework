using System;
using System.Collections.Generic;
using Codout.Framework.DAL.Entity;
using Codout.Framework.DAL.Repository;
using NHibernate;

namespace Codout.Framework.NH
{
    public class RepositoryFactory : IDisposable
    {
        private bool _disposed = false;

        private static readonly IDictionary<Type, Type> RegisteredRepositories = new Dictionary<Type, Type>();

        private readonly IDictionary<Type, object> _repositories = new Dictionary<Type, object>();

        private ISession Session { get; }

        public RepositoryFactory(ISession session)
        {
            Session = session;
        }

        public static void RegisterRepository<TInterface, TRepository, TEntity>()
            where TInterface : IRepository<TEntity>
            where TRepository : class, IRepository<TEntity>
            where TEntity : class, IEntity
        {
            if (!RegisteredRepositories.ContainsKey(typeof(TInterface)))
            {
                RegisteredRepositories.Add(typeof(TInterface), typeof(TRepository));
            }
        }

        public TInterface Get<TInterface>()
        {
            var key = typeof(TInterface);

            if (_repositories.ContainsKey(key))
                return (TInterface)_repositories[key];

            if (RegisteredRepositories.ContainsKey(key))
            {
                var instanceType = RegisteredRepositories[key];
                var instance = (TInterface)Activator.CreateInstance(instanceType, Session);
                _repositories.Add(key, instance);
            }
            else
            {
                return default(TInterface);
            }

            return (TInterface)_repositories[key];
        }

        public void Dispose() => Dispose(true);

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                _repositories?.Clear();
                Session?.Dispose();
            }

            _disposed = true;
        }
    }
}
