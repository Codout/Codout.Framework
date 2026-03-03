using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Codout.Framework.Data.Entity;
using Codout.Framework.Data.Repository;
using NHibernate;
using NHibernate.Linq;

namespace Codout.Framework.NH;

/// <summary>
/// Repositório genérico de dados para NHibernate
/// </summary>
/// <typeparam name="T">Classe que define o tipo do repositório</typeparam>
public class NHRepository<T>(ISession session) : IRepository<T> where T : class, IEntity
{
    private bool _disposed;

    public ISession Session { get; } = session ?? throw new ArgumentNullException(nameof(session));

    #region Synchronous Query Methods

    public IQueryable<T> All() => Session.Query<T>();

    public IQueryable<T> AllReadOnly()
    {
        Session.DefaultReadOnly = true;
        return Session.Query<T>();
    }

    public IQueryable<T> Where(Expression<Func<T, bool>> predicate)
    {
        return All().Where(predicate);
    }

    public IQueryable<T> WhereReadOnly(Expression<Func<T, bool>> predicate)
    {
        Session.DefaultReadOnly = true;
        return Session.Query<T>().Where(predicate);
    }

    public IQueryable<T> WherePaged(Expression<Func<T, bool>> predicate, out int total, int index = 0, int size = 50)
    {
        var query = Session.Query<T>().Where(predicate);

        total = query.Count();
        return query.Skip(index * size).Take(size);
    }

    public T Get(Expression<Func<T, bool>> predicate) =>
        All().SingleOrDefault(predicate);

    public T Get(object key) =>
        Session.Get<T>(key);

    public T Load(object key) =>
        Session.Load<T>(key);

    #endregion

    #region Synchronous Command Methods

    public void Delete(T entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        Session.Delete(entity);
    }

    public void Delete(Expression<Func<T, bool>> predicate)
    {
        var entities = Where(predicate).ToList();
        foreach (var entity in entities)
            Delete(entity);
    }

    public T Save(T entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        Session.Save(entity);
        return entity;
    }

    public T SaveOrUpdate(T entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        Session.SaveOrUpdate(entity);
        return entity;
    }

    public void Update(T entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        Session.Update(entity);
    }

    public T Merge(T entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        return Session.Merge(entity);
    }

    public T Refresh(T entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        Session.Refresh(entity);
        return entity;
    }

    #endregion

    #region Asynchronous Query Methods

    public async Task<T> GetAsync(Expression<Func<T, bool>> predicate)
    {
        return await GetAsync(predicate, CancellationToken.None);
    }

    public async Task<T> GetAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken)
    {
        return await All().SingleOrDefaultAsync(predicate, cancellationToken);
    }

    public async Task<T> GetAsync(object key)
    {
        return await GetAsync(key, CancellationToken.None);
    }

    public async Task<T> GetAsync(object key, CancellationToken cancellationToken)
    {
        return await Session.GetAsync<T>(key, cancellationToken);
    }

    public Task<T> LoadAsync(object key)
    {
        return LoadAsync(key, CancellationToken.None);
    }

    public Task<T> LoadAsync(object key, CancellationToken cancellationToken)
    {
        // NHibernate Load retorna proxy, não é async
        return Task.FromResult(Session.Load<T>(key));
    }

    public async Task<T> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await All().FirstOrDefaultAsync(predicate, cancellationToken);
    }

    public async Task<bool> AnyAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await All().AnyAsync(predicate, cancellationToken);
    }

    public async Task<int> CountAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await All().CountAsync(predicate, cancellationToken);
    }

    public async Task<List<T>> ToListAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await All().Where(predicate).ToListAsync(cancellationToken);
    }

    #endregion

    #region Asynchronous Command Methods

    public async Task DeleteAsync(T entity)
    {
        await DeleteAsync(entity, CancellationToken.None);
    }

    public async Task DeleteAsync(T entity, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(entity);
        await Session.DeleteAsync(entity, cancellationToken);
    }

    public async Task DeleteAsync(Expression<Func<T, bool>> predicate)
    {
        await DeleteAsync(predicate, CancellationToken.None);
    }

    public async Task DeleteAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken)
    {
        var entities = await All().Where(predicate).ToListAsync(cancellationToken);
        foreach (var entity in entities)
            await DeleteAsync(entity, cancellationToken);
    }

    public async Task<T> SaveAsync(T entity)
    {
        return await SaveAsync(entity, CancellationToken.None);
    }

    public async Task<T> SaveAsync(T entity, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(entity);
        await Session.SaveAsync(entity, cancellationToken);
        return entity;
    }

    public async Task<T> SaveOrUpdateAsync(T entity)
    {
        return await SaveOrUpdateAsync(entity, CancellationToken.None);
    }

    public async Task<T> SaveOrUpdateAsync(T entity, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(entity);
        await Session.SaveOrUpdateAsync(entity, cancellationToken);
        return entity;
    }

    public async Task UpdateAsync(T entity)
    {
        await UpdateAsync(entity, CancellationToken.None);
    }

    public async Task UpdateAsync(T entity, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(entity);
        await Session.UpdateAsync(entity, cancellationToken);
    }

    public async Task<T> MergeAsync(T entity)
    {
        return await MergeAsync(entity, CancellationToken.None);
    }

    public async Task<T> MergeAsync(T entity, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(entity);
        return await Session.MergeAsync(entity, cancellationToken);
    }

    public async Task<T> RefreshAsync(T entity)
    {
        return await RefreshAsync(entity, CancellationToken.None);
    }

    public async Task<T> RefreshAsync(T entity, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(entity);
        await Session.RefreshAsync(entity, cancellationToken);
        return entity;
    }

    #endregion

    #region Include Methods

    /// <summary>
    /// NHibernate não suporta Include como EF Core.
    /// Use Fetch/FetchMany no mapeamento ou queries específicas.
    /// </summary>
    public IQueryable<T> IncludeMany(params Expression<Func<T, object>>[] includes)
    {
        // NHibernate não tem suporte direto a Include
        // Use .Fetch() no LINQ ou configure eager loading no mapeamento
        return All();
    }

    #endregion

    #region IDisposable

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                // NHibernate Session é gerenciado pelo UnitOfWork
            }

            _disposed = true;
        }
    }

    #endregion
}
