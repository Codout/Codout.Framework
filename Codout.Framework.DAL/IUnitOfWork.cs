using System;
using System.Data;

namespace Codout.Framework.DAL;

public interface IUnitOfWork : IDisposable
{
    void BeginTransaction(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted);
    void Commit(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted);
    void Rollback();
}