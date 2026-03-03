using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Codout.Framework.Data;
using Codout.Framework.Data.Entity;
using MongoDB.Driver;

namespace Codout.Framework.Mongo;

/// <summary>
/// Unit of Work para MongoDB com suporte a transações (requer replica set)
/// </summary>
public class MongoUnitOfWork : IUnitOfWork
{
    private readonly IMongoClient _client;
    private IClientSessionHandle? _session;
    private bool _disposed;

    public MongoUnitOfWork(IMongoClient client)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
    }

    #region Synchronous Transaction Methods

    public void BeginTransaction()
    {
        BeginTransaction(IsolationLevel.ReadCommitted);
    }

    public void BeginTransaction(IsolationLevel isolationLevel)
    {
        if (_session != null)
            throw new InvalidOperationException("Uma transação já está em andamento.");

        _session = _client.StartSession();
        
        var options = new TransactionOptions(
            readConcern: ReadConcern.Snapshot,
            writeConcern: WriteConcern.WMajority);
        
        _session.StartTransaction(options);
    }

    public void Commit()
    {
        if (_session == null || !_session.IsInTransaction)
            throw new InvalidOperationException("Nenhuma transação ativa para commit. Chame BeginTransaction() primeiro.");

        try
        {
            _session.CommitTransaction();
        }
        catch
        {
            Rollback();
            throw;
        }
        finally
        {
            _session?.Dispose();
            _session = null;
        }
    }

    public void Commit(IsolationLevel isolationLevel)
    {
        // IsolationLevel é definido no BeginTransaction
        Commit();
    }

    public void Rollback()
    {
        if (_session == null)
            return;

        try
        {
            if (_session.IsInTransaction)
                _session.AbortTransaction();
        }
        finally
        {
            _session?.Dispose();
            _session = null;
        }
    }

    public T InTransaction<T>(Func<T> work) where T : class, IEntity
    {
        if (work == null) throw new ArgumentNullException(nameof(work));

        var shouldManageTransaction = _session == null;

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

    public async Task BeginTransactionAsync(IsolationLevel isolationLevel, CancellationToken cancellationToken = default)
    {
        if (_session != null)
            throw new InvalidOperationException("Uma transação já está em andamento.");

        _session = await _client.StartSessionAsync(cancellationToken: cancellationToken);
        
        var options = new TransactionOptions(
            readConcern: ReadConcern.Snapshot,
            writeConcern: WriteConcern.WMajority);
        
        _session.StartTransaction(options);
    }

    public async Task CommitAsync(CancellationToken cancellationToken = default)
    {
        if (_session == null || !_session.IsInTransaction)
            throw new InvalidOperationException("Nenhuma transação ativa para commit. Chame BeginTransactionAsync() primeiro.");

        try
        {
            await _session.CommitTransactionAsync(cancellationToken);
        }
        catch
        {
            await RollbackAsync(cancellationToken);
            throw;
        }
        finally
        {
            if (_session != null)
            {
                _session.Dispose();
                _session = null;
            }
        }
    }

    public async Task RollbackAsync(CancellationToken cancellationToken = default)
    {
        if (_session == null)
            return;

        try
        {
            if (_session.IsInTransaction)
                await _session.AbortTransactionAsync(cancellationToken);
        }
        finally
        {
            if (_session != null)
            {
                _session.Dispose();
                _session = null;
            }
        }
    }

    public async Task<T> InTransactionAsync<T>(Func<Task<T>> work, CancellationToken cancellationToken = default) where T : class, IEntity
    {
        if (work == null) throw new ArgumentNullException(nameof(work));

        var shouldManageTransaction = _session == null;

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
                _session?.Dispose();
                _session = null;
            }

            _disposed = true;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual ValueTask DisposeAsyncCore()
    {
        if (_session != null)
        {
            _session.Dispose();
            _session = null;
        }

        return ValueTask.CompletedTask;
    }

    public async ValueTask DisposeAsync()
    {
        await DisposeAsyncCore();
        Dispose(false);
        GC.SuppressFinalize(this);
    }

    #endregion

    /// <summary>
    /// Obtém a sessão MongoDB atual (para uso em operações dentro da transação)
    /// </summary>
    public IClientSessionHandle? CurrentSession => _session;
}
