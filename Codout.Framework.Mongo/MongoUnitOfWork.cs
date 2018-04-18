using System;
using System.Collections.Generic;
using Codout.Framework.DAL;
using Codout.Framework.DAL.Entity;
using Codout.Framework.DAL.Repository;
using Codout.Framework.NetCore.Repository.Mongo;

namespace Codout.Framework.Mongo
{
    /// <summary>
    /// Unit of Work para repositório genérico com MongoDB
    /// </summary>
    public abstract class MongoUnitOfWork<T> : IUnitOfWork where T : MongoDbContext
    {
        private readonly IDictionary<Type, object> _repositories = new Dictionary<Type, object>();

        protected MongoUnitOfWork(T instance)
        {
            MongoDbContext = instance;
        }

        /// <summary>
        /// Contexto do MongoDB
        /// </summary>
        public MongoDbContext MongoDbContext { get; }

        /// <summary>
        /// Efetua o SaveChanges do contexto (sessão) em questão
        /// </summary>
        public void SaveChanges()
        {
        }

        /// <summary>
        /// Repositório Genérico que será controlado
        /// </summary>
        /// <typeparam name="TEntity">Tipo do objeto</typeparam>
        /// <returns>Repositório concreto</returns>
        public IRepository<TEntity> Repository<TEntity>() where TEntity : class, IEntity
        {
            if (!_repositories.ContainsKey(typeof(TEntity)))
                _repositories.Add(typeof(TEntity), new MongoRepository<TEntity>(MongoDbContext));
            return _repositories[typeof(TEntity)] as IRepository<TEntity>;
        }

        #region IDisposable Support
        private bool _disposedValue;

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                }

                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion

    }
}
