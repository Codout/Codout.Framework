using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Codout.Framework.NetStandard.Domain.Entity;
using Codout.Framework.NetStandard.Repository;

namespace Codout.Framework.NetCore.Repository.DocumentDB
{
    /// <inheritdoc />
    public class DocumentDBRepository<T> : IRepository<T> where T : class, IEntity
    {
        public void Dispose()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public IQueryable<T> All()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public async Task<IQueryable<T>> AllAsync()
        {
            throw new NotImplementedException();
        }

        public IQueryable<T> Find(Expression<Func<T, bool>> predicate)
        {
            throw new NotImplementedException();
        }

        public async Task<IQueryable<T>> FindAsync(Expression<Func<T, bool>> predicate)
        {
            throw new NotImplementedException();
        }

        public IQueryable<T> Find(Expression<Func<T, bool>> filter, out int total, int index = 0, int size = 50)
        {
            throw new NotImplementedException();
        }

        public async Task<IQueryable<T>> FindAsync(Expression<Func<T, bool>> filter, out int total, int index = 0, int size = 50)
        {
            throw new NotImplementedException();
        }

        public T Get(Expression<Func<T, bool>> predicate)
        {
            throw new NotImplementedException();
        }

        public async Task<T> GetAsync(Expression<Func<T, bool>> predicate)
        {
            throw new NotImplementedException();
        }

        public T Get(object key)
        {
            throw new NotImplementedException();
        }

        public async Task<T> GetAsync(object key)
        {
            throw new NotImplementedException();
        }

        public T Load(object key)
        {
            throw new NotImplementedException();
        }

        public async Task<T> LoadAsync(object key)
        {
            throw new NotImplementedException();
        }

        public void Delete(T entity)
        {
            throw new NotImplementedException();
        }

        public async Task DeleteAsync(T entity)
        {
            throw new NotImplementedException();
        }

        public void Delete(Expression<Func<T, bool>> predicate)
        {
            throw new NotImplementedException();
        }

        public async Task DeleteAsync(Expression<Func<T, bool>> predicate)
        {
            throw new NotImplementedException();
        }

        public T Save(T entity)
        {
            throw new NotImplementedException();
        }

        public async Task<T> SaveAsync(T entity)
        {
            throw new NotImplementedException();
        }

        public T SaveOrUpdate(T entity)
        {
            throw new NotImplementedException();
        }

        public async Task<T> SaveOrUpdateAsync(T entity)
        {
            throw new NotImplementedException();
        }

        public void Update(T entity)
        {
            throw new NotImplementedException();
        }

        public async Task UpdateAsync(T entity)
        {
            throw new NotImplementedException();
        }

        public T Merge(T entity)
        {
            throw new NotImplementedException();
        }

        public async Task<T> MergeAsync(T entity)
        {
            throw new NotImplementedException();
        }
    }
}
