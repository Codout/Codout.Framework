using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Codout.Framework.Data;
using Codout.Framework.Data.Entity;
using NHibernate;

namespace Codout.Framework.NH;

/// <summary>
/// Implementação de Unit of Work para NHibernate com suporte completo async
/// </summary>
public class NHUnitOfWork(ISession session) : IUnitOfWork
{
    private ITransaction? _transaction;
    private bool _disposed;

    /// <summary>
    /// Exposição da sessão ativa
    /// </summary>
    public ISession Session => session ?? throw new ArgumentNullException(nameof(session));

    #region Synchronous Transaction Methods

    public void BeginTransaction()
    {
        BeginTransaction(IsolationLevel.ReadCommitted);
    }

    public void BeginTransaction(IsolationLevel isolationLevel)
    {
        if (_transaction != null && _transaction.IsActive)
            throw new InvalidOperationException("Uma transação já está em andamento.");

        _transaction = Session.BeginTransaction(isolationLevel);
    }

    public void Commit()
    {
        if (_transaction == null || !_transaction.IsActive)
            throw new InvalidOperationException("Nenhuma transação ativa para commit. Chame BeginTransaction() primeiro.");

        try
        {
            Session.Flush();
            _transaction.Commit();
        }
        catch
        {
            Rollback();
            throw;
        }
        finally
        {
            _transaction?.Dispose();
            _transaction = null;
        }
    }

    public void Commit(IsolationLevel isolationLevel)
    {
        // IsolationLevel é definido no BeginTransaction
        Commit();
    }

    public void Rollback()
    {
        if (_transaction == null)
            return;

        try
        {
            if (_transaction.IsActive)
                _transaction.Rollback();
        }
        finally
        {
            _transaction?.Dispose();
            _transaction = null;
        }
    }

    public T InTransaction<T>(Func<T> work) where T : class, IEntity
    {
        if (work == null) throw new ArgumentNullException(nameof(work));

        var shouldManageTransaction = _transaction == null;

        if (shouldManageTransaction)
            BeginTransaction();

        try
        {
            var result = work();
            
            if (shouldManageTransaction)
                Commit();
            
            return result;
        }
        catch
        {
            if (shouldManageTransaction)
                Rollback();
            throw;
        }
    }

    #endregion

    #region Asynchronous Transaction Methods

    public Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        return BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);
    }

    public Task BeginTransactionAsync(IsolationLevel isolationLevel, CancellationToken cancellationToken = default)
    {
        if (_transaction != null && _transaction.IsActive)
            throw new InvalidOperationException("Uma transação já está em andamento.");

        // NHibernate BeginTransaction não é async nativo
        _transaction = Session.BeginTransaction(isolationLevel);
        return Task.CompletedTask;
    }

    public async Task CommitAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction == null || !_transaction.IsActive)
            throw new InvalidOperationException("Nenhuma transação ativa para commit. Chame BeginTransactionAsync() primeiro.");

        try
        {
            await Session.FlushAsync(cancellationToken);
            await _transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await RollbackAsync(cancellationToken);
            throw;
        }
        finally
        {
            if (_transaction != null)
            {
                _transaction.Dispose();
                _transaction = null;
            }
        }
    }

    public async Task RollbackAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction == null)
            return;

        try
        {
            if (_transaction.IsActive)
                await _transaction.RollbackAsync(cancellationToken);
        }
        finally
        {
            if (_transaction != null)
            {
                _transaction.Dispose();
                _transaction = null;
            }
        }
    }

    public async Task<T> InTransactionAsync<T>(Func<Task<T>> work, CancellationToken cancellationToken = default) where T : class, IEntity
    {
        if (work == null) throw new ArgumentNullException(nameof(work));

        var shouldManageTransaction = _transaction == null;

        if (shouldManageTransaction)
            await BeginTransactionAsync(cancellationToken);

        try
        {
            var result = await work();
            
            if (shouldManageTransaction)
                await CommitAsync(cancellationToken);
            
            return result;
        }
        catch
        {
            if (shouldManageTransaction)
                await RollbackAsync(cancellationToken);
            throw;
        }
    }

    #endregion

    #region IDisposable / IAsyncDisposable

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                try
                {
                    Rollback();
                }
                catch
                {
                    // Ignorar exceções no dispose
                }

                Session?.Dispose();
            }

            _disposed = true;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual async ValueTask DisposeAsyncCore()
    {
        try
        {
            await RollbackAsync();
        }
        catch
        {
            // Ignorar exceções no dispose
        }

        Session?.Dispose();
    }

    public async ValueTask DisposeAsync()
    {
        await DisposeAsyncCore();
        Dispose(false);
        GC.SuppressFinalize(this);
    }

    #endregion
}
