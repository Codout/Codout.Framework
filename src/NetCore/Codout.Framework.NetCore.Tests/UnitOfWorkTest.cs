using Microsoft.EntityFrameworkCore;
using Codout.Framework.NetStandard.Repository;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace Codout.Framework.NetCore.Tests
{
    public interface IUnitOfWorkTest : IUnitOfWork
    {
        ICustomerRepository Customers { get; }
    }

    #region UnitOfWorkTestEF
    public class UnitOfWorkTestEF : Repository.EF.EFUnitOfWork<UnitTesteContextEF>, IUnitOfWorkTest
    {

        private ICustomerRepository _customers;

        public ICustomerRepository Customers => _customers ?? (_customers = new RepositoryCustomerEF(DbContext));

        public new void Dispose()
        {
            _customers?.Dispose();
            GC.SuppressFinalize(this);
        }
    }

    public class UnitTesteContextEF : DbContext
    {
        public UnitTesteContextEF(DbContextOptions<UnitTesteContextEF> options)
            : base(options)
        {
        }

        public UnitTesteContextEF()
            : this(GetBuilder())
        {
        }

        private static DbContextOptions<UnitTesteContextEF> GetBuilder()
        {
            var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json");
            var config = configuration.Build();

            var builder = new DbContextOptionsBuilder<UnitTesteContextEF>();
            builder.UseSqlServer(config.GetConnectionString("EF"));
            return builder.Options;
        }

        public DbSet<Customer> Customers { get; set; }
    }
    #endregion

    #region UnitOfWorkTestMongo
    public class UnitOfWorkTestMongo : Repository.Mongo.MongoUnitOfWork<Repository.Mongo.MongoDbContext>, IUnitOfWorkTest
    {

        private ICustomerRepository _customers;

        public ICustomerRepository Customers => _customers ?? (_customers = new RepositoryCustomerMongo(MongoDbContext));

        public new void Dispose()
        {
            _customers?.Dispose();
            GC.SuppressFinalize(this);
        }
    }
    #endregion

}

