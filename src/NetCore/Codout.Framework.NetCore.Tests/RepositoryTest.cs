using Microsoft.EntityFrameworkCore;
using Codout.Framework.NetStandard.Repository;
using System;
using System.Collections.Generic;
using System.Text;

namespace Codout.Framework.NetCore.Tests
{

    public interface ICustomerRepository : IRepository<Customer>
    {  }


    public class RepositoryCustomerEF : Repository.EF.EFRepository<Customer>, ICustomerRepository
    {
        public RepositoryCustomerEF(DbContext context) : base(context)
        {
        }
    }

    public class RepositoryCustomerMongo : Repository.Mongo.MongoRepository<Customer>, ICustomerRepository
    {
        public RepositoryCustomerMongo(Repository.Mongo.MongoDbContext context) : base(context)
        {
        }
    }
}
