using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Codout.Framework.Data.Entity;

namespace Codout.Framework.Data.Repository;

/// <summary>
/// Defines the contract for a generic repository pattern implementation
/// </summary>
/// <typeparam name="T">The entity type</typeparam>
public interface IRepository<T> : IDisposable where T : class, IEntity
{
    #region Synchronous Query Methods

    /// <summary>
    /// Gets all entities
    /// </summary>
    IQueryable<T> All();
    
    /// <summary>
    /// Gets all entities with no tracking (read-only)
    /// </summary>
    IQueryable<T> AllReadOnly();
    
    /// <summary>
    /// Gets entities matching the predicate
    /// </summary>
    IQueryable<T> Where(Expression<Func<T, bool>> predicate);
    
    /// <summary>
    /// Gets entities matching the predicate with no tracking (read-only)
    /// </summary>
    IQueryable<T> WhereReadOnly(Expression<Func<T, bool>> predicate);
    
    /// <summary>
    /// Gets a paged set of entities matching the predicate
    /// </summary>
    IQueryable<T> WherePaged(Expression<Func<T, bool>> predicate, out int total, int index = 0, int size = 50);
    
    /// <summary>
    /// Gets a single entity matching the predicate
    /// </summary>
    T Get(Expression<Func<T, bool>> predicate);
    
    /// <summary>
    /// Gets an entity by its key
    /// </summary>
    T Get(object key);
    
    /// <summary>
    /// Loads an entity by its key
    /// </summary>
    T Load(object key);

    #endregion

    #region Synchronous Command Methods

    /// <summary>
    /// Deletes an entity
    /// </summary>
    void Delete(T entity);
    
    /// <summary>
    /// Deletes entities matching the predicate
    /// </summary>
    void Delete(Expression<Func<T, bool>> predicate);
    
    /// <summary>
    /// Saves a new entity
    /// </summary>
    T Save(T entity);
    
    /// <summary>
    /// Saves a new entity or updates an existing one
    /// </summary>
    T SaveOrUpdate(T entity);
    
    /// <summary>
    /// Updates an existing entity
    /// </summary>
    void Update(T entity);
    
    /// <summary>
    /// Merges an entity with the current context
    /// </summary>
    T Merge(T entity);
    
    /// <summary>
    /// Refreshes an entity from the data store
    /// </summary>
    T Refresh(T entity);

    #endregion

    #region Asynchronous Query Methods

    /// <summary>
    /// Gets a single entity matching the predicate asynchronously
    /// </summary>
    Task<T> GetAsync(Expression<Func<T, bool>> predicate);
    
    /// <summary>
    /// Gets a single entity matching the predicate asynchronously with cancellation support
    /// </summary>
    Task<T> GetAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken);
    
    /// <summary>
    /// Gets an entity by its key asynchronously
    /// </summary>
    Task<T> GetAsync(object key);
    
    /// <summary>
    /// Gets an entity by its key asynchronously with cancellation support
    /// </summary>
    Task<T> GetAsync(object key, CancellationToken cancellationToken);
    
    /// <summary>
    /// Loads an entity by its key asynchronously
    /// </summary>
    Task<T> LoadAsync(object key);
    
    /// <summary>
    /// Loads an entity by its key asynchronously with cancellation support
    /// </summary>
    Task<T> LoadAsync(object key, CancellationToken cancellationToken);
    
    /// <summary>
    /// Gets the first entity matching the predicate or null asynchronously
    /// </summary>
    Task<T> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Determines whether any entities match the predicate asynchronously
    /// </summary>
    Task<bool> AnyAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Counts entities matching the predicate asynchronously
    /// </summary>
    Task<int> CountAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets all entities matching the predicate as a list asynchronously
    /// </summary>
    Task<List<T>> ToListAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);

    #endregion

    #region Asynchronous Command Methods

    /// <summary>
    /// Deletes an entity asynchronously
    /// </summary>
    Task DeleteAsync(T entity);
    
    /// <summary>
    /// Deletes an entity asynchronously with cancellation support
    /// </summary>
    Task DeleteAsync(T entity, CancellationToken cancellationToken);
    
    /// <summary>
    /// Deletes entities matching the predicate asynchronously
    /// </summary>
    Task DeleteAsync(Expression<Func<T, bool>> predicate);
    
    /// <summary>
    /// Deletes entities matching the predicate asynchronously with cancellation support
    /// </summary>
    Task DeleteAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken);
    
    /// <summary>
    /// Saves a new entity asynchronously
    /// </summary>
    Task<T> SaveAsync(T entity);
    
    /// <summary>
    /// Saves a new entity asynchronously with cancellation support
    /// </summary>
    Task<T> SaveAsync(T entity, CancellationToken cancellationToken);
    
    /// <summary>
    /// Saves a new entity or updates an existing one asynchronously
    /// </summary>
    Task<T> SaveOrUpdateAsync(T entity);
    
    /// <summary>
    /// Saves a new entity or updates an existing one asynchronously with cancellation support
    /// </summary>
    Task<T> SaveOrUpdateAsync(T entity, CancellationToken cancellationToken);
    
    /// <summary>
    /// Updates an existing entity asynchronously
    /// </summary>
    Task UpdateAsync(T entity);
    
    /// <summary>
    /// Updates an existing entity asynchronously with cancellation support
    /// </summary>
    Task UpdateAsync(T entity, CancellationToken cancellationToken);
    
    /// <summary>
    /// Merges an entity with the current context asynchronously
    /// </summary>
    Task<T> MergeAsync(T entity);
    
    /// <summary>
    /// Merges an entity with the current context asynchronously with cancellation support
    /// </summary>
    Task<T> MergeAsync(T entity, CancellationToken cancellationToken);
    
    /// <summary>
    /// Refreshes an entity from the data store asynchronously
    /// </summary>
    Task<T> RefreshAsync(T entity);
    
    /// <summary>
    /// Refreshes an entity from the data store asynchronously with cancellation support
    /// </summary>
    Task<T> RefreshAsync(T entity, CancellationToken cancellationToken);

    #endregion

    #region Include Methods

    /// <summary>
    /// Includes related entities in the query
    /// </summary>
    IQueryable<T> IncludeMany(params Expression<Func<T, object>>[] includes);

    #endregion
}