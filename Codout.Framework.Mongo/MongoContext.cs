using System;
using System.Collections.Generic;
using Codout.Framework.DAL.Entity;
using Codout.Framework.DAL.Repository;
using MongoDB.Driver;

namespace Codout.Framework.Mongo
{
    /// <summary>
    /// DBContext para MongoDB (Deve conter o arquivo appsettings.json na app com as configurações de conexão)
    /// </summary>
    public class MongoContext
    {
        public readonly IMongoDatabase Database;

        private readonly IDictionary<Type, object> _collections = new Dictionary<Type, object>();

        public MongoContext(MongoOptions options)
        {
            IMongoClient client = new MongoClient(options.ConnectionString);
            Database = client.GetDatabase(options.DatabaseName);
        }

        /// <summary>
        /// Repositório Genérico que será controlado
        /// </summary>
        /// <typeparam name="TEntity">Tipo do objeto</typeparam>
        /// <returns>Repositório concreto</returns>
        public IRepository<TEntity> Repository<TEntity>() where TEntity : class, IEntity
        {
            if (!_collections.ContainsKey(typeof(TEntity)))
                _collections.Add(typeof(TEntity), new MongoCollection<TEntity>(this));
            return _collections[typeof(TEntity)] as IRepository<TEntity>;
        }

    }
}
