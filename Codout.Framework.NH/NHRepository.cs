using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Codout.Framework.DAL;
using Codout.Framework.DAL.Entity;
using Codout.Framework.DAL.Repository;
using NHibernate;
using NHibernate.Linq;

namespace Codout.Framework.NH
{
    public class NHRepository<T> : IRepository<T> where T : class, IEntity
    {
        public NHUnitOfWork UnitOfWork { get; }

        public ISession Session => UnitOfWork.Session;

        public NHRepository(IUnitOfWork unitOfWork)
        {
            UnitOfWork = unitOfWork as NHUnitOfWork;
        }

        public IQueryable<T> All()
        {
            return Session.Query<T>();
        }

        public IQueryable<T> Find(Expression<Func<T, bool>> predicate)
        {
            return All().Where(predicate);
        }

        public IQueryable<T> Find(Expression<Func<T, bool>> filter, out int total, int index = 0, int size = 50)
        {
            var skipCount = index * size;

            var resetSet = filter != null
                ? All().Where(filter).AsQueryable()
                : All().AsQueryable();

            resetSet = skipCount == 0
                ? resetSet.Take(size)
                : resetSet.Skip(skipCount).Take(size);

            total = resetSet.Count();

            return resetSet.AsQueryable();
        }

        public T Get(Expression<Func<T, bool>> predicate)
        {
            return All().SingleOrDefault(predicate);
        }

        public T Get(object key)
        {
            return Session.Get<T>(key);
        }

        public T Load(object key)
        {
            return Session.Load<T>(key);
        }

        public void Delete(T entity)
        {
            Session.Delete(entity);
        }

        public void Delete(Expression<Func<T, bool>> predicate)
        {
            var entities = Find(predicate);
            foreach (var entity in entities)
                Delete(entity);
        }

        public T Save(T entity)
        {
            return Session.Save(entity) as T;
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

        public async Task<T> LoadAsync(object key)
        {
            return await Session.LoadAsync<T>(key);
        }

        public async Task DeleteAsync(T entity)
        {
            await Session.DeleteAsync(entity);
        }

        public async Task DeleteAsync(Expression<Func<T, bool>> predicate)
        {
            var entities = Find(predicate);
            foreach (var entity in entities)
                await DeleteAsync(entity);
        }

        public async Task<T> SaveAsync(T entity)
        {
            return await Session.SaveAsync(entity) as T;
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

        private bool _disposed;

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    //Release
                }
            }
            _disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
