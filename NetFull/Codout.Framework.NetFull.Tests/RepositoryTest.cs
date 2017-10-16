using System.Data.Entity;
using Codout.Framework.EF6;

namespace Codout.Framework.NetFull.Tests
{
    public class RepositoryCustomer : EFRepository<Customer>
    {
        public RepositoryCustomer(DbContext context) : base(context)
        {
        }
    }
}
