using System;
using System.Data.Entity;

namespace Codout.Framework.NetFull.Tests
{
    public class UnitOfWorkTest : Repository.EF.EFUnitOfWork<UnitTesteContext>
    {

        private RepositoryCustomer _customers;

        public RepositoryCustomer Customers => _customers ?? (_customers = new RepositoryCustomer(DbContext));

        public new void Dispose()
        {
            _customers?.Dispose();
            GC.SuppressFinalize(this);
        }
    }

    public class UnitTesteContext : DbContext
    {
        public UnitTesteContext() : base("Server=.\\SQL2014;Database=FrameworkTeste;Integrated Security=true;MultipleActiveResultSets=true")
        {
        }

        public DbSet<Customer> Customers { get; set; }
    }
}
