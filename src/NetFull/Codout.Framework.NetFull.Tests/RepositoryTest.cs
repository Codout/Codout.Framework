using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Codout.Framework.NetFull.Tests
{
    public class RepositoryCustomer : Repository.EF.EFRepository<Customer>
    {
        public RepositoryCustomer(DbContext context) : base(context)
        {
        }
    }
}
