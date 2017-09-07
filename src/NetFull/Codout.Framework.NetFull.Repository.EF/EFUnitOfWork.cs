using Codout.Framework.NetStandard.Domain.Entity;
using Codout.Framework.NetStandard.Repository;
using System;
using System.Collections.Generic;
using System.Data.Entity;

namespace Codout.Framework.NetFull.Repository.EF
{
    /// <summary>
    /// Unit of Work para repositório genérico com EntityFramework Full (6.1.3)
    /// </summary>
    public abstract class EFUnitOfWork<T> : IUnitOfWork where T : DbContext, new()
    {
        private readonly IDictionary<Type, object> _repositories = new Dictionary<Type, object>();

        protected EFUnitOfWork()
        {
            DbContext = new T();
        }

        private bool _disposed;

        /// <summary>
        /// Conexto do EntityFrameworkCore
        /// </summary>
        public DbContext DbContext { get; }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    DbContext.Dispose();
                }
            }
            _disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Efetua o SaveChanges do contexto (sessão) em questão
        /// </summary>
        public void SaveChanges()
        {
            DbContext.SaveChanges();
        }

        /// <summary>
        /// Repositório Genérico que será controlado
        /// </summary>
        /// <typeparam name="T">Tipo do objeto</typeparam>
        /// <returns>Repositório concreto</returns>
        public IRepository<TEntity> Repository<TEntity>() where TEntity : class, IEntity
        {
            if (!_repositories.ContainsKey(typeof(TEntity)))
                _repositories.Add(typeof(TEntity), new EFRepository<TEntity>(DbContext));
            return _repositories[typeof(TEntity)] as IRepository<TEntity>;
        }
    }
}
