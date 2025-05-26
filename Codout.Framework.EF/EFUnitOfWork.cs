using System;
using System.Data;
using System.Data.Entity;
using Codout.Framework.DAL;

namespace Codout.Framework.EF;

/// <summary>
///     Unit of Work para repositório genérico com EntityFrameworkCore
/// </summary>
public abstract class EFUnitOfWork<T>(T instance) : IUnitOfWork where T : DbContext
{
    private DbContextTransaction _transaction;

    /// <summary>
    ///     Conexto do EntityFrameworkCore
    /// </summary>
    public DbContext DbContext { get; } = instance;

    public void BeginTransaction(IsolationLevel isolationLevel)
    {
        _transaction = DbContext.Database.BeginTransaction();
    }

    public void BeginTransaction()
    {
        BeginTransaction(IsolationLevel.ReadCommitted);
    }

    public void Commit(IsolationLevel isolationLevel)
    {
        _transaction?.Commit();
    }

    /// <summary>
    ///     Efetua o SaveChanges do contexto (sessão) em questão
    /// </summary>
    public void Commit()
    {
        if (_transaction == null)
            BeginTransaction();

        try
        {
            DbContext.SaveChanges();
            _transaction?.Commit();
        }
        catch (Exception)
        {
            Rollback();
        }
        finally
        {
            _transaction?.Dispose();
            _transaction = null;
        }
    }

    public void Rollback()
    {
        try
        {
            if (_transaction != null)
                _transaction.Rollback();
        }
        finally
        {
            _transaction?.Dispose();
            _transaction = null;
        }
    }

    #region IDisposable Support

    private bool _disposed;

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
            if (disposing)
            {
                DbContext.Dispose();

                if (_transaction != null)
                {
                    _transaction?.Dispose();
                    _transaction = null;
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