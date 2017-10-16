using System;
using Codout.Framework.DAL.Entity;
using Codout.Framework.DAL.Repository;

namespace Codout.Framework.DAL
{
    public interface IUnitOfWork : IDisposable
    {
        void SaveChanges();

        IRepository<T> Repository<T>() where T : class, IEntity;
    }
}