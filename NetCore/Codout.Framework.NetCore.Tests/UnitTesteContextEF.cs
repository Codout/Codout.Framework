using Microsoft.EntityFrameworkCore;

namespace Codout.Framework.NetCore.Tests
{
    public class UnitTesteContextEF : DbContext
    {
        public UnitTesteContextEF(DbContextOptions options)
            : base(options)
        {
        }

        public DbSet<Customer> Customers { get; set; }
    }
}
