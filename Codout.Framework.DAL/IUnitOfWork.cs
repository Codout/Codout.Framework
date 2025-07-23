using System;
using System.Data;
using Codout.Framework.DAL.Entity;

namespace Codout.Framework.DAL;

public interface IUnitOfWork : IDisposable
{
    void Commit();
    void Commit(IsolationLevel isolationLevel);
    void Rollback();
    void BeginTransaction();
    void BeginTransaction(IsolationLevel isolationLevel);
    T InTransaction<T>(Func<T> work) where T : class, IEntity;
}