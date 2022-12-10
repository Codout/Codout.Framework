﻿using System;
using Codout.Framework.DAL;
using NHibernate;
using IsolationLevel = System.Data.IsolationLevel;

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
                        if (_transaction is { IsActive: true })
                        {
                            try
                            {
                                // rollback if there was an exception
                                if (_transaction is { IsActive: true })
                                    _transaction.Rollback();
                            }
                            catch
                            {
                                //Ignore
                            }
                        }

                        _transaction?.Dispose();

                        _transaction = null;

                        _session?.Dispose();

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
            BeginTransaction(IsolationLevel.ReadCommitted);
        }

        public void BeginTransaction(IsolationLevel isolationLevel)
        {
            _transaction = Session.BeginTransaction(isolationLevel);
        }

        public void Commit()
        {
            Commit(IsolationLevel.ReadCommitted);
        }

        public void Commit(IsolationLevel isolationLevel)
        {
            if (!(_transaction is { IsActive: true }))
                BeginTransaction(isolationLevel);

            try
            {
                // commit transaction if there is one active
                if (_transaction is { IsActive: true })
                    _transaction.Commit();
            }
            catch
            {
                try
                {
                    // rollback if there was an exception
                    if (_transaction is { IsActive: true })
                        _transaction.Rollback();
                }
                catch
                {
                    //Ignore
                }

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
                try
                {
                    // rollback if there was an exception
                    if (_transaction is { IsActive: true })
                        _transaction.Rollback();
                }
                catch
                {
                    //Ignore
                }
            }
            finally
            {
                _transaction?.Dispose();
                _transaction = null;
            }
        }
    }
}
