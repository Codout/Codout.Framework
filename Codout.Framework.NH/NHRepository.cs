using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Codout.Framework.DAL;
using Codout.Framework.DAL.Entity;
using Codout.Framework.DAL.Repository;
using Codout.Framework.NH;
using NHibernate;
using NHibernate.Linq;

public class NHRepository<T>(IUnitOfWork unitOfWork) : IRepository<T>
    where T : class, IEntity
{
    private bool _disposed;
    public NHUnitOfWork UnitOfWork { get; } = unitOfWork as NHUnitOfWork ?? throw new ArgumentException("UnitOfWork must be of type NHUnitOfWork.");
    public ISession Session => UnitOfWork.Session;

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

    public void Delete(T entity)
    {
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
        Session.Save(entity);
        return entity;
    }

    public T SaveOrUpdate(T entity)
    {
        Session.SaveOrUpdate(entity);
        return entity;
    }

    public void Update(T entity)
    {
        Session.Update(entity);
    }

    public T Merge(T entity)
    {
        return Session.Merge(entity);
    }

    public T Refresh(T entity)
    {
        Session.Refresh(entity);
        return entity;
    }

    public async Task<T> RefreshAsync(T entity)
    {
        await Session.RefreshAsync(entity);
        return entity;
    }

    public async Task<T> GetAsync(Expression<Func<T, bool>> predicate)
    {
        return await All().SingleOrDefaultAsync(predicate);
    }

    public async Task<T> GetAsync(object key)
    {
        return await Session.GetAsync<T>(key);
    }

    public Task<T> LoadAsync(object key)
    {
        return Task.FromResult(Session.Load<T>(key));
    }

    public async Task DeleteAsync(T entity)
    {
        await Session.DeleteAsync(entity);
    }

    public async Task DeleteAsync(Expression<Func<T, bool>> predicate)
    {
        var entities = Where(predicate).ToList();
        foreach (var entity in entities)
            await DeleteAsync(entity);
    }

    public async Task<T> SaveAsync(T entity)
    {
        await Session.SaveAsync(entity);
        return entity;
    }

    public async Task<T> SaveOrUpdateAsync(T entity)
    {
        await Session.SaveOrUpdateAsync(entity);
        return entity;
    }

    public async Task UpdateAsync(T entity)
    {
        await Session.UpdateAsync(entity);
    }

    public async Task<T> MergeAsync(T entity)
    {
        return await Session.MergeAsync(entity);
    }

    public IQueryable<T> IncludeMany(params Expression<Func<T, object>>[] includes)
    {
        // NHibernate doesn't support Include — use only for interface compatibility
        return All(); // No-op
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed && disposing)
        {
            // Optional: Dispose or close session
        }

        _disposed = true;
    }
}
