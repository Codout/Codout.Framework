using System;
using System.Data.Entity;
using Codout.Framework.EF6;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Codout.Framework.NetFull.Tests
{
    public class UnitOfWorkTest : EFUnitOfWork<UnitTesteContext>
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
        // string conexão LocalDB   
        //data source=(LocalDb)\MSSQLLocalDB;initial catalog=CodoutFameworkTeste;integrated security=True;MultipleActiveResultSets=True;App=EntityFramework
        public UnitTesteContext() : base("data source=(LocalDb)\\MSSQLLocalDB;initial catalog=CodoutFameworkTeste;integrated security=True;MultipleActiveResultSets=True;App=EntityFramework")
        {
        }

        public DbSet<Customer> Customers { get; set; }
    }
}
