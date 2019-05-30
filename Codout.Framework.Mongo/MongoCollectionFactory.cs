using System;
using System.Collections.Generic;
using Codout.Framework.DAL.Entity;
using Codout.Framework.DAL.Repository;

namespace Codout.Framework.Mongo
{
    public class MongoCollectionFactory
    {
        private readonly MongoContext _mongoContext;
        private static readonly IDictionary<Type, Type> RegisteredRepositories = new Dictionary<Type, Type>();

        private readonly IDictionary<Type, object> _collections = new Dictionary<Type, object>();

        public MongoCollectionFactory(MongoContext mongoContext)
        {
            _mongoContext = mongoContext;
        }

        public static void RegisterCollection<TInterface, TRepository, TEntity>()
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

            if (_collections.ContainsKey(key))
                return (TInterface)_collections[key];

            if (RegisteredRepositories.ContainsKey(key))
            {
                var instanceType = RegisteredRepositories[key];
                var instance     = (TInterface)Activator.CreateInstance(instanceType, _mongoContext);
                _collections.Add(key, instance);
            }
            else
            {
                return default(TInterface);
            }

            return (TInterface)_collections[key];
        }
    }
}
