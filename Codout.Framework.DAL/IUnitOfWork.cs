using System;
using System.Data;

namespace Codout.Framework.DAL;

public interface IUnitOfWork : IDisposable
{
    void BeginTransaction(IsolationLevel isolationLevel);
    void BeginTransaction();
    void Commit(IsolationLevel isolationLevel);
    void Commit();
    void Rollback();
}