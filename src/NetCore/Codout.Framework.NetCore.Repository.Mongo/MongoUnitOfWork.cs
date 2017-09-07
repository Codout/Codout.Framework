using Codout.Framework.NetStandard.Domain.Entity;
using Codout.Framework.NetStandard.Repository;
using System;
using System.Collections.Generic;

namespace Codout.Framework.NetCore.Repository.Mongo
{
    /// <summary>
    /// Unit of Work para repositório genérico com MongoDB
    /// </summary>
    public abstract class MongoUnitOfWork<T> : IUnitOfWork where T : MongoDbContext, new()
    {
        private readonly IDictionary<Type, object> _repositories = new Dictionary<Type, object>();
        public MongoDbContext MongoDbContext { get; }

        protected MongoUnitOfWork()
        {
            MongoDbContext = new T();
            //MongoDbContext = new MongoDbContext();
        }

        /// <summary>
        /// Efetua o SaveChanges do contexto (sessão) em questão
        /// </summary>
        public void SaveChanges()
        {
        }

        /// <summary>
        /// Repositório Genérico que será controlado
        /// </summary>
        /// <typeparam name="T">Tipo do objeto</typeparam>
        /// <returns>Repositório concreto</returns>
        public IRepository<TEntity> Repository<TEntity>() where TEntity : class, IEntity
        {
            if (!_repositories.ContainsKey(typeof(TEntity)))
                _repositories.Add(typeof(TEntity), new MongoRepository<TEntity>(MongoDbContext));
            return _repositories[typeof(TEntity)] as IRepository<TEntity>;
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~MongoUnitOfWork() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion

    }
}
