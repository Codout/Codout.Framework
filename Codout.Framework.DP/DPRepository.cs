using System;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Codout.Framework.DAL.Entity;
using Codout.Framework.DAL.Repository;
using Dapper;
using Dapper.Contrib.Extensions;
using MicroOrm.Dapper.Repositories;
using MicroOrm.Dapper.Repositories.SqlGenerator;

namespace Codout.Framework.DP
{
    public class DPRepository<T> : DapperRepository<T>, IRepository<T> where T : class, IEntity
    {
        protected IDbConnection DbConnection { get; }

        public DPRepository(IDbConnection connection, ISqlGenerator<T> sqlGenerator)
            : base(connection, sqlGenerator)
        {

        }

        private IDbConnection CreateConnection()
        {
            DbConnection.Open();
            return DbConnection;
        }

        public void Dispose()
        {
            DbConnection.Dispose();
        }

        public IQueryable<T> All()
        {
            using var con = CreateConnection();

            return DbConnection.GetAll<T>().AsQueryable();
        }

        public IQueryable<T> Find(Expression<Func<T, bool>> predicate)
        {
            return DbConnection.Query() <T>().AsQueryable();
        }

        public IQueryable<T> Find(Expression<Func<T, bool>> filter, out int total, int index = 0, int size = 50)
        {
            throw new NotImplementedException();
        }

        public T Get(Expression<Func<T, bool>> predicate)
        {
            throw new NotImplementedException();
        }

        public T Get(object key)
        {
            using var con = CreateConnection();

            return DbConnection.Get<T>(key);
        }

        public T Load(object key)
        {
            return Get(key);
        }

        public void Delete(T entity)
        {
            using var con = CreateConnection();

            var result = DbConnection.Delete<T>(entity);
        }

        public void Delete(Expression<Func<T, bool>> predicate)
        {
            using var con = CreateConnection();

            var result = DbConnection.Delete<T>(entity);
        }

        public T Save(T entity)
        {
            using var con = CreateConnection();

            var result = DbConnection.Insert<T>(entity);

            return entity;
        }

        public T SaveOrUpdate(T entity)
        {
            using var con = CreateConnection();

            var result = DbConnection.Update<T>(entity);

            return entity;
        }

        public void Update(T entity)
        {
            throw new NotImplementedException();
        }

        public T Merge(T entity)
        {
            throw new NotImplementedException();
        }

        public Task<T> GetAsync(Expression<Func<T, bool>> predicate)
        {
            throw new NotImplementedException();
        }

        public Task<T> GetAsync(object key)
        {
            throw new NotImplementedException();
        }

        public Task<T> LoadAsync(object key)
        {
            throw new NotImplementedException();
        }

        public Task DeleteAsync(T entity)
        {
            throw new NotImplementedException();
        }

        public Task DeleteAsync(Expression<Func<T, bool>> predicate)
        {
            throw new NotImplementedException();
        }

        public Task<T> SaveAsync(T entity)
        {
            throw new NotImplementedException();
        }

        public Task<T> SaveOrUpdateAsync(T entity)
        {
            throw new NotImplementedException();
        }

        public Task UpdateAsync(T entity)
        {
            throw new NotImplementedException();
        }

        public Task<T> MergeAsync(T entity)
        {
            throw new NotImplementedException();
        }
    }
}
