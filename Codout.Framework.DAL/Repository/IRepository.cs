using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Codout.Framework.DAL.Entity;

namespace Codout.Framework.DAL.Repository;

public interface IRepository<T> : IDisposable where T : class, IEntity
{
    IQueryable<T> All();
    IQueryable<T> AllReadOnly();
    IQueryable<T> Where(Expression<Func<T, bool>> predicate);
    IQueryable<T> WhereReadOnly(Expression<Func<T, bool>> predicate);
    IQueryable<T> WherePaged(Expression<Func<T, bool>> predicate, out int total, int index = 0, int size = 50);
    T Get(Expression<Func<T, bool>> predicate);
    T Get(object key);
    T Load(object key);
    void Delete(T entity);
    void Delete(Expression<Func<T, bool>> predicate);
    T Save(T entity);
    T SaveOrUpdate(T entity);
    void Update(T entity);
    T Merge(T entity);
    T Refresh(T entity);

    Task<T> GetAsync(Expression<Func<T, bool>> predicate);
    Task<T> GetAsync(object key);
    Task<T> LoadAsync(object key);
    Task DeleteAsync(T entity);
    Task DeleteAsync(Expression<Func<T, bool>> predicate);
    Task<T> SaveAsync(T entity);
    Task<T> SaveOrUpdateAsync(T entity);
    Task UpdateAsync(T entity);
    Task<T> MergeAsync(T entity);
    Task<T> RefreshAsync(T entity);
    IQueryable<T> IncludeMany(params Expression<Func<T, object>>[] includes);
}