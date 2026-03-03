using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Codout.Framework.Data.Entity;
using Codout.Framework.Data.Repository;
using Microsoft.EntityFrameworkCore;

namespace Codout.Framework.EF;

public class EFRepository<T>(DbContext context) : IRepository<T> where T : class, IEntity
{
    public DbContext Context { get; } = context ?? throw new ArgumentNullException(nameof(context));

    public DbSet<T> DbSet => Context.Set<T>();

    public IQueryable<T> All() => DbSet;

    public IQueryable<T> AllReadOnly() => DbSet.AsNoTracking();

    public IQueryable<T> Where(Expression<Func<T, bool>> predicate) =>
        DbSet.Where(predicate);

    public IQueryable<T> WhereReadOnly(Expression<Func<T, bool>> predicate) =>
        DbSet.Where(predicate).AsNoTracking();

    public IQueryable<T> WherePaged(Expression<Func<T, bool>> predicate, out int total, int index = 0, int size = 50)
    {
        var query = DbSet.Where(predicate);
        total = query.Count();

        return query.Skip(index * size).Take(size);
    }

    public T Get(Expression<Func<T, bool>> predicate) =>
        DbSet.SingleOrDefault(predicate);

    public T Get(object key) =>
        DbSet.Find(key);

    public T Load(object key) => Get(key);

    public void Delete(T entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        DbSet.Remove(entity);
    }

    public void Delete(Expression<Func<T, bool>> predicate)
    {
        var entities = DbSet.Where(predicate).ToList();
        DbSet.RemoveRange(entities);
    }

    public T Save(T entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        DbSet.Add(entity);
        return entity;
    }

    public T SaveOrUpdate(T entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        if (entity.IsTransient())
            DbSet.Add(entity);
        else
            Update(entity);
        return entity;
    }

    public void Update(T entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        Context.Entry(entity).State = EntityState.Modified;
    }

    public T Merge(T entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        DbSet.Attach(entity);
        return entity;
    }

    public T Refresh(T entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        Context.Entry(entity).Reload();
        return entity;
    }

    public async Task<T> RefreshAsync(T entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        await Context.Entry(entity).ReloadAsync();
        return entity;
    }

    public Task<T> RefreshAsync(T entity, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(entity);
        return Task.FromResult(Refresh(entity));
    }

    public async Task<T> GetAsync(Expression<Func<T, bool>> predicate) =>
        await DbSet.SingleOrDefaultAsync(predicate);

    public async Task<T> GetAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken) =>
        await DbSet.SingleOrDefaultAsync(predicate, cancellationToken);

    public async Task<T> GetAsync(object key) =>
        await DbSet.FindAsync(key);

    public async Task<T> GetAsync(object key, CancellationToken cancellationToken) =>
        await DbSet.FindAsync([key], cancellationToken);

    public Task<T> LoadAsync(object key) => GetAsync(key);

    public Task<T> LoadAsync(object key, CancellationToken cancellationToken) => GetAsync(key, cancellationToken);

    public Task DeleteAsync(T entity)
    {
        Delete(entity);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(T entity, CancellationToken cancellationToken)
    {
        Delete(entity);
        return Task.CompletedTask;
    }

    public async Task DeleteAsync(Expression<Func<T, bool>> predicate)
    {
        var entities = await DbSet.Where(predicate).ToListAsync();
        DbSet.RemoveRange(entities);
    }

    public async Task DeleteAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken)
    {
        var entities = await DbSet.Where(predicate).ToListAsync(cancellationToken);
        DbSet.RemoveRange(entities);
    }

    public Task<T> SaveAsync(T entity)
    {
        Save(entity);
        return Task.FromResult(entity);
    }

    public Task<T> SaveAsync(T entity, CancellationToken cancellationToken)
    {
        Save(entity);
        return Task.FromResult(entity);
    }

    public Task<T> SaveOrUpdateAsync(T entity)
    {
        SaveOrUpdate(entity);
        return Task.FromResult(entity);
    }

    public Task<T> SaveOrUpdateAsync(T entity, CancellationToken cancellationToken)
    {
        SaveOrUpdate(entity);
        return Task.FromResult(entity);
    }

    public Task UpdateAsync(T entity)
    {
        Update(entity);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(T entity, CancellationToken cancellationToken)
    {
        Update(entity);
        return Task.CompletedTask;
    }

    public Task<T> MergeAsync(T entity)
    {
        Merge(entity);
        return Task.FromResult(entity);
    }

    public Task<T> MergeAsync(T entity, CancellationToken cancellationToken)
    {
        Merge(entity);
        return Task.FromResult(entity);
    }

    public async Task<T> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default) =>
        await DbSet.FirstOrDefaultAsync(predicate, cancellationToken);

    public async Task<bool> AnyAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default) =>
        await DbSet.AnyAsync(predicate, cancellationToken);

    public async Task<int> CountAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default) =>
        await DbSet.CountAsync(predicate, cancellationToken);

    public async Task<List<T>> ToListAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default) =>
        await DbSet.Where(predicate).ToListAsync(cancellationToken);

    public IQueryable<T> IncludeMany(params Expression<Func<T, object>>[] includes)
    {
        IQueryable<T> query = DbSet;

        foreach (var include in includes)
            query = query.Include(include);

        return query;
    }

    public IQueryable<T> IncludeMany(params string[] includes)
    {
        IQueryable<T> query = DbSet;

        foreach (var include in includes)
            query = query.Include(include);

        return query;
    }

    // Repository não deve dispor o DbContext - isso é responsabilidade do container DI ou UnitOfWork
    public void Dispose()
    {
        // Intencionalmente vazio - o DbContext é gerenciado externamente
        GC.SuppressFinalize(this);
    }
}
