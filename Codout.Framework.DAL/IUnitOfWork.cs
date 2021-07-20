using System;

namespace Codout.Framework.DAL
{
    public interface IUnitOfWork : IDisposable
    {
        void SaveChanges();

        void CancelChanges();
    }
}