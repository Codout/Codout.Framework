using System;
using System.Collections.Generic;
using System.Data.Entity;
using Codout.Framework.DAL;
using Codout.Framework.DAL.Entity;
using Codout.Framework.DAL.Repository;

namespace Codout.Framework.EF
{
    /// <summary>
    /// Unit of Work para repositório genérico com EntityFrameworkCore
    /// </summary>
    public abstract class EFUnitOfWork<T> : IUnitOfWork where T : DbContext
    {
        private readonly IDictionary<Type, object> _repositories = new Dictionary<Type, object>();

        protected EFUnitOfWork(T instance)
        {
            DbContext = instance;
        }

        /// <summary>
        /// Conexto do EntityFrameworkCore
        /// </summary>
        public DbContext DbContext { get; }

        /// <summary>
        /// Efetua o SaveChanges do contexto (sessão) em questão
        /// </summary>
        public void SaveChanges()
        {
            using var dbContextTransaction = DbContext.Database.BeginTransaction();

            try
            {
                DbContext.SaveChanges();

                dbContextTransaction.Commit();
            }
            catch (Exception)
            {
                try
                {
                    dbContextTransaction.Rollback();
                }
                catch (Exception)
                {
                    //Igore  
                }
            }
        }

        public void CancelChanges()
        {
            using var dbContextTransaction = DbContext.Database.BeginTransaction();

            try
            {
                dbContextTransaction.Rollback();
            }
            catch (Exception)
            {
                //Igore  
            }
        }

        /// <summary>
        /// Repositório Genérico que será controlado
        /// </summary>
        /// <typeparam name="TEntity">Tipo do objeto</typeparam>
        /// <returns>Repositório concreto</returns>
        public IRepository<TEntity> Repository<TEntity>() where TEntity : class, IEntity
        {
            if (!_repositories.ContainsKey(typeof(TEntity)))
                _repositories.Add(typeof(TEntity), new EFRepository<TEntity>(DbContext));
            return _repositories[typeof(TEntity)] as IRepository<TEntity>;
        }

        #region IDisposable Support
        private bool _disposed;

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
        #endregion IDisposable Support

    }
}
