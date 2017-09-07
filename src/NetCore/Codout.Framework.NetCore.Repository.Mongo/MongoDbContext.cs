using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using System.IO;

namespace Codout.Framework.NetCore.Repository.Mongo
{

    /// <summary>
    /// DBContext para MongoDB (Deve conter o arquivo appsettings.json na app com as configurações de conexão)
    /// </summary>
    public class MongoDbContext
    {
        public readonly IMongoDatabase Database;

        public MongoDbContext()
        {
            var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json");
            var config = configuration.Build();
            IMongoClient client = new MongoClient(config.GetConnectionString("MongoDB"));
            Database = client.GetDatabase(config["MongoDBDatabaseName"]);
        }

        /// <summary>
        /// The private GetCollection method
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <returns></returns>
        public IMongoCollection<TEntity> GetCollection<TEntity>()
        {
            return Database.GetCollection<TEntity>(typeof(TEntity).Name.ToLower() + "s");
        }

    }
}
