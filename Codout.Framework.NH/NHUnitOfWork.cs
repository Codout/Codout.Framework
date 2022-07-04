using System;
using Codout.Framework.DAL;
using NHibernate;

namespace Codout.Framework.NH
{
    public class NHUnitOfWork : IUnitOfWork
    {
        private bool _disposed;
        private ITransaction _transaction;
        private ISession _session;

        public SessionFactory SessionFactory { get; }

        public ISession Session => _session ??= SessionFactory.OpenSession();

        public NHUnitOfWork(ITenant tenant)
        {
            SessionFactory = new SessionFactory(tenant);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    if (_session is { IsOpen: true })
                    {
                        if (_transaction != null)
                        {
                            if (_transaction.IsActive)
                            {
                                _transaction.Rollback();
                            }

                            _transaction.Dispose();
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

        public void BeginTransaction()
        {
            _transaction = Session.BeginTransaction();
        }

        public void Commit()
        {
            if (!(_transaction is { IsActive: true }))
                BeginTransaction();

            try
            {
                // commit transaction if there is one active
                if (_transaction is { IsActive: true })
                    _transaction.Commit();
            }
            catch
            {
                // rollback if there was an exception
                Rollback();

                throw;
            }
            finally
            {
                _transaction?.Dispose();
            }
        }

        public void Rollback()
        {
            try
            {
                if (_transaction is { IsActive: true })
                    _transaction.Rollback();
            }
            finally
            {
                _transaction?.Dispose();
            }
        }
    }
}
