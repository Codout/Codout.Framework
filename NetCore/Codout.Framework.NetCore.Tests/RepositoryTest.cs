using Microsoft.EntityFrameworkCore;
using Codout.Framework.DAL.Repository;
using Codout.Framework.EF;
using Codout.Framework.Mongo;
using Codout.Framework.NetCore.Repository.Mongo;

namespace Codout.Framework.NetCore.Tests
{

    public interface ICustomerRepository : IRepository<Customer>
    {  }


    public class RepositoryCustomerEF : EFRepository<Customer>, ICustomerRepository
    {
        public RepositoryCustomerEF(DbContext context) : base(context)
        {
        }
    }

    public class RepositoryCustomerMongo : MongoRepository<Customer>, ICustomerRepository
    {
        public RepositoryCustomerMongo(MongoDbContext context) : base(context)
        {
        }
    }
}
