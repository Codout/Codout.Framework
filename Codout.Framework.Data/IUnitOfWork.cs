using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Codout.Framework.Data.Entity;

namespace Codout.Framework.Data;

/// <summary>
/// Defines the contract for a Unit of Work pattern implementation
/// </summary>
public interface IUnitOfWork : IDisposable, IAsyncDisposable
{
    /// <summary>
    /// Commits the current transaction
    /// </summary>
    void Commit();
    
    /// <summary>
    /// Commits the current transaction with specified isolation level
    /// </summary>
    /// <param name="isolationLevel">The isolation level for the transaction</param>
    void Commit(IsolationLevel isolationLevel);
    
    /// <summary>
    /// Commits the current transaction asynchronously
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation</param>
    Task CommitAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Rolls back the current transaction
    /// </summary>
    void Rollback();
    
    /// <summary>
    /// Rolls back the current transaction asynchronously
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation</param>
    Task RollbackAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Begins a new transaction
    /// </summary>
    void BeginTransaction();
    
    /// <summary>
    /// Begins a new transaction with specified isolation level
    /// </summary>
    /// <param name="isolationLevel">The isolation level for the transaction</param>
    void BeginTransaction(IsolationLevel isolationLevel);
    
    /// <summary>
    /// Begins a new transaction asynchronously
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation</param>
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Begins a new transaction asynchronously with specified isolation level
    /// </summary>
    /// <param name="isolationLevel">The isolation level for the transaction</param>
    /// <param name="cancellationToken">A token to cancel the operation</param>
    Task BeginTransactionAsync(IsolationLevel isolationLevel, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Executes work within a transaction scope
    /// </summary>
    /// <typeparam name="T">The entity type</typeparam>
    /// <param name="work">The work to execute</param>
    /// <returns>The result of the work</returns>
    T InTransaction<T>(Func<T> work) where T : class, IEntity;
    
    /// <summary>
    /// Executes work within a transaction scope asynchronously
    /// </summary>
    /// <typeparam name="T">The entity type</typeparam>
    /// <param name="work">The async work to execute</param>
    /// <param name="cancellationToken">A token to cancel the operation</param>
    /// <returns>The result of the work</returns>
    Task<T> InTransactionAsync<T>(Func<Task<T>> work, CancellationToken cancellationToken = default) where T : class, IEntity;
}