using MongoDB.Driver;

namespace Codout.Framework.Mongo
{
    /// <summary>
    /// DBContext para MongoDB (Deve conter o arquivo appsettings.json na app com as configurações de conexão)
    /// </summary>
    public class MongoDbContext
    {
        public readonly IMongoDatabase Database;

        public MongoDbContext(MongoDBOptions options)
        {
            IMongoClient client = new MongoClient(options.ConnectionString);
            Database = client.GetDatabase(options.DatabaseName);
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
