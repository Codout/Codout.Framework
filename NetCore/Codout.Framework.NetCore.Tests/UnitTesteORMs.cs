using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using Codout.Framework.Mongo;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Codout.Framework.NetCore.Tests
{
    [TestClass]
    public class UnitTesteORMs
    {
        [TestMethod]
        public void TestaInclusaoELeituraDBSQLEF()
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json");
            var config = configuration.Build();

            var builder = new DbContextOptionsBuilder<UnitTesteContextEF>();
            builder.UseSqlServer(config.GetConnectionString("EF"));

            IUnitOfWorkTest unitOfWorkTest = new UnitOfWorkTestEF(new UnitTesteContextEF(builder.Options));

            var guid = Guid.Parse("97A7513F-7D57-4171-96B5-AE02E2A9C6CE");

            var cliente = unitOfWorkTest.Customers.Get(guid);

            if (cliente == null)
            {
                cliente = new Customer { Nome = "José da Silva" };
                cliente.SetId((guid));
                unitOfWorkTest.Customers.Save(cliente);
            }
            else
            {
                cliente.Nome = $"José da Silva + {DateTime.Now.ToShortDateString()} + {DateTime.Now.ToLongTimeString()}";
                unitOfWorkTest.Customers.Update(cliente);
            }

            unitOfWorkTest.SaveChanges();

            var obj = unitOfWorkTest.Customers.Get(guid);

            Assert.AreEqual(guid, obj.Id);
        }

        [TestMethod]
        public void TestaInclusaoELeituraMongoDB()
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json");
            var config = configuration.Build();

            var mongoDBOptions = new MongoDBOptions
            {
                ConnectionString = config.GetConnectionString("MongoDB"),
                DatabaseName = config["MongoDBDatabaseName"]
            };

            //**** CHECAR A STRING DE CONEXÃO NO ARQUIVO appsettings.json
            IUnitOfWorkTest unitOfWorkTest = new UnitOfWorkTestMongo(new MongoDbContext(mongoDBOptions));

            Guid? guid = Guid.Parse("97A7513F-7D57-4171-96B5-AE02E2A9C6CE");

            Customer cliente = unitOfWorkTest.Customers.Get(guid);
            if (cliente == null)
            {
                cliente = new Customer { Nome = "José da Silva" };
                cliente.SetId((guid));
                unitOfWorkTest.Customers.Save(cliente);
            }
            else
            {
                cliente.Nome = $"José da Silva + {DateTime.Now.ToShortDateString()} + {DateTime.Now.ToLongTimeString()}";
                unitOfWorkTest.Customers.Update(cliente);
            }
            unitOfWorkTest.SaveChanges();

            var obj = unitOfWorkTest.Customers.Get(guid);

            Assert.AreEqual(guid, obj.Id);
        }
    }
}
