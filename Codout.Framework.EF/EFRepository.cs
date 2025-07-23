using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Codout.Framework.DAL.Entity;
using Codout.Framework.DAL.Repository;
using Microsoft.EntityFrameworkCore;

public class EFRepository<T> : IRepository<T> where T : class, IEntity
{
    protected readonly DbContext Context;
    protected DbSet<T> DbSet => Context.Set<T>();
    private bool _disposed;

    public EFRepository(DbContext context)
    {
        Context = context ?? throw new ArgumentNullException(nameof(context));
    }

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
        Context.Entry(entity).Reload();
        return entity;
    }

    public async Task<T> RefreshAsync(T entity)
    {
        await Context.Entry(entity).ReloadAsync();
        return entity;
    }

    public async Task<T> GetAsync(Expression<Func<T, bool>> predicate) =>
        await DbSet.SingleOrDefaultAsync(predicate);

    public async Task<T> GetAsync(object key) =>
        await DbSet.FindAsync(key);

    public Task<T> LoadAsync(object key) => GetAsync(key);

    public Task DeleteAsync(T entity)
    {
        Delete(entity);
        return Task.CompletedTask;
    }

    public async Task DeleteAsync(Expression<Func<T, bool>> predicate)
    {
        var entities = await DbSet.Where(predicate).ToListAsync();
        DbSet.RemoveRange(entities);
    }

    public Task<T> SaveAsync(T entity)
    {
        Save(entity);
        return Task.FromResult(entity);
    }

    public Task<T> SaveOrUpdateAsync(T entity)
    {
        SaveOrUpdate(entity);
        return Task.FromResult(entity);
    }

    public Task UpdateAsync(T entity)
    {
        Update(entity);
        return Task.CompletedTask;
    }

    public Task<T> MergeAsync(T entity)
    {
        Merge(entity);
        return Task.FromResult(entity);
    }

    public IQueryable<T> IncludeMany(params Expression<Func<T, object>>[] includes)
    {
        IQueryable<T> query = DbSet;

        foreach (var include in includes)
            query = query.Include(include);

        return query;
    }

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
                Context.Dispose();

            _disposed = true;
        }
    }
}
