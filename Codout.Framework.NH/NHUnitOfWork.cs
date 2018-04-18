using System;
using System.Collections.Generic;
using System.Data;
using Codout.Framework.DAL;
using Codout.Framework.DAL.Entity;
using Codout.Framework.DAL.Repository;
using NHibernate;

namespace Codout.Framework.NH
{
    public class NHUnitOfWork : IUnitOfWork
    {
        private readonly IDictionary<Type, object> _repositories = new Dictionary<Type, object>();
        private static readonly SessionFactory _sessionFactory = new SessionFactory();
        private ISession _session;
        private IStatelessSession _statelessSession;
        private readonly ITenant _tenant;
        private bool _disposed;

        public NHUnitOfWork(ITenant tenant = null)
        {
            _tenant = tenant;
        }

        public ISession Session => _session ?? (_session = _sessionFactory.OpenSession(_tenant));

        public IStatelessSession StatelessSession => _statelessSession ?? (_statelessSession = _sessionFactory.OpenStatelessSession(_tenant));

        public SessionFactory SessionFactory => _sessionFactory;

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    if (_session != null && _session.IsOpen)
                    {
                        if (_session.Transaction != null)
                        {
                            if (_session.Transaction.IsActive)
                            {
                                _session.Transaction.Rollback();
                            }

                            _session.Transaction.Dispose();
                        }

                        _session.Dispose();

                        _session = null;
                    }
                }
            }
            _disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void SaveChanges()
        {
            using (var tx = _session.BeginTransaction(IsolationLevel.ReadCommitted))
            {
                try
                {
                    //forces a flush of the current unit of work
                    tx.Commit();
                }
                catch
                {
                    try
                    {
                        tx.Rollback();
                    }
                    catch
                    {
                        // ignored
                    }

                    throw;
                }
            }
        }

        public IRepository<TEntity> Repository<TEntity>() where TEntity : class, IEntity
        {
            if (!_repositories.ContainsKey(typeof(TEntity)))
                _repositories.Add(typeof(TEntity), new NHRepository<TEntity>(Session));
            return _repositories[typeof(TEntity)] as IRepository<TEntity>;
        }
    }
}
