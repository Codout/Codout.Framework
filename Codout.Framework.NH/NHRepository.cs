using System;
using System.Linq;
using System.Linq.Expressions;
using Codout.Framework.DAL.Entity;
using Codout.Framework.DAL.Repository;
using NHibernate;
using NHibernate.Linq;

namespace Codout.Framework.NH
{
    public class NHRepository<T> : IRepository<T> where T : class, IEntity
    {
        public ISession Session { get; }

        public NHRepository(ISession session)
        {
            Session = session;
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

        private bool _disposed;

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    Session.Dispose();
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
