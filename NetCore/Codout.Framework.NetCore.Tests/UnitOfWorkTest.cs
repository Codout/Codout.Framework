using System;
using Codout.Framework.DAL;
using Codout.Framework.EF;
using Codout.Framework.Mongo;

namespace Codout.Framework.NetCore.Tests
{
    public interface IUnitOfWorkTest : IUnitOfWork
    {
        ICustomerRepository Customers { get; }
    }

    #region UnitOfWorkTestEF
    public class UnitOfWorkTestEF : EFUnitOfWork<UnitTesteContextEF>, IUnitOfWorkTest
    {
        private ICustomerRepository _customers;

        public UnitOfWorkTestEF(UnitTesteContextEF instance)
            : base(instance)
        {
        }

        public ICustomerRepository Customers => _customers ?? (_customers = new RepositoryCustomerEF(DbContext));

        public new void Dispose()
        {
            _customers?.Dispose();
            GC.SuppressFinalize(this);
        }
    }
    #endregion

    #region UnitOfWorkTestMongo
    public class UnitOfWorkTestMongo : MongoUnitOfWork<MongoDbContext>, IUnitOfWorkTest
    {

        private ICustomerRepository _customers;

        public UnitOfWorkTestMongo(MongoDbContext instance)
            : base(instance)
        {
        }

        public ICustomerRepository Customers => _customers ?? (_customers = new RepositoryCustomerMongo(MongoDbContext));

        public new void Dispose()
        {
            _customers?.Dispose();
            GC.SuppressFinalize(this);
        }
    }
    #endregion

}

