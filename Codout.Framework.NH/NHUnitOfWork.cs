using System;
using System.Data;
using Codout.Framework.DAL;
using NHibernate;

namespace Codout.Framework.NH
{
    public class NHUnitOfWork : IUnitOfWork
    {
        private static readonly SessionFactory _sessionFactory = new SessionFactory();
        private ITenant _tenant;
        private bool _disposed;
        private ISession _session;

        public NHUnitOfWork(ITenant tenant)
        {
            _tenant = tenant;
        }

        public ITenant Tenant => _tenant;

        public SessionFactory SessionFactory => _sessionFactory;

        public ISession Session => _session ??= _sessionFactory.OpenSession(_tenant);

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    if (_session is {IsOpen: true})
                    {
                        var transaction = _session.GetCurrentTransaction();

                        if (transaction != null)
                        {
                            if (transaction.IsActive)
                            {
                                transaction.Rollback();
                            }

                            transaction.Dispose();
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
            using var tx = _session.BeginTransaction(IsolationLevel.ReadCommitted);
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

        public void CancelChanges()
        {
            using var tx = _session.BeginTransaction(IsolationLevel.ReadCommitted);
            try
            {
                tx.Rollback();
            }
            catch
            {
                // ignored
            }
        }
    }
}
