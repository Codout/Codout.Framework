using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Codout.Framework.Data;
using Codout.Framework.Data.Entity;
using Microsoft.EntityFrameworkCore.Storage;

namespace Codout.Framework.EF;

/// <summary>
/// Unit of Work para repositório genérico com EntityFrameworkCore
/// </summary>
public abstract class EFUnitOfWork<T>(T instance) : IUnitOfWork where T : DbContext
{
    private IDbContextTransaction? _transaction;
    private bool _disposed;

    /// <summary>
    /// Contexto do EntityFrameworkCore
    /// </summary>
    public DbContext DbContext { get; } = instance ?? throw new ArgumentNullException(nameof(instance));

    public void BeginTransaction(IsolationLevel isolationLevel)
    {
        if (_transaction != null)
            throw new InvalidOperationException("Uma transação já está em andamento.");

        _transaction = DbContext.Database.BeginTransaction(isolationLevel);
    }

    public void BeginTransaction()
    {
        BeginTransaction(IsolationLevel.ReadCommitted);
    }

    public async Task BeginTransactionAsync(IsolationLevel isolationLevel, CancellationToken cancellationToken = default)
    {
        if (_transaction != null)
            throw new InvalidOperationException("Uma transação já está em andamento.");

        _transaction = await DbContext.Database.BeginTransactionAsync(isolationLevel, cancellationToken);
    }

    public Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        return BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);
    }

    public T1 InTransaction<T1>(Func<T1> work) where T1 : class, IEntity
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

    public async Task<T1> InTransactionAsync<T1>(Func<Task<T1>> work, CancellationToken cancellationToken = default) where T1 : class, IEntity
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

    public void Commit(IsolationLevel isolationLevel)
    {
        // IsolationLevel é definido no BeginTransaction, não no Commit
        Commit();
    }

    /// <summary>
    /// Efetua o SaveChanges do contexto (sessão) em questão
    /// </summary>
    public void Commit()
    {
        if (_transaction == null)
            throw new InvalidOperationException("Nenhuma transação ativa para commit. Chame BeginTransaction() primeiro.");

        try
        {
            DbContext.SaveChanges();
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

    public async Task CommitAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction == null)
            throw new InvalidOperationException("Nenhuma transação ativa para commit. Chame BeginTransactionAsync() primeiro.");

        try
        {
            await DbContext.SaveChangesAsync(cancellationToken);
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
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }
    }

    public void Rollback()
    {
        if (_transaction == null)
            return;

        try
        {
            _transaction.Rollback();
        }
        finally
        {
            _transaction?.Dispose();
            _transaction = null;
        }
    }

    public async Task RollbackAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction == null)
            return;

        try
        {
            await _transaction.RollbackAsync(cancellationToken);
        }
        finally
        {
            if (_transaction != null)
            {
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }
    }

    #region IDisposable Support

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _transaction?.Dispose();
                _transaction = null;
                DbContext.Dispose();
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
        if (_transaction != null)
        {
            await _transaction.DisposeAsync();
            _transaction = null;
        }

        await DbContext.DisposeAsync();
    }

    public async ValueTask DisposeAsync()
    {
        await DisposeAsyncCore();
        Dispose(false);
        GC.SuppressFinalize(this);
    }

    #endregion IDisposable Support
}