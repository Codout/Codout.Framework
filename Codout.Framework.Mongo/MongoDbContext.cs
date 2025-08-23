using MongoDB.Driver;

namespace Codout.Framework.Mongo;

public class MongoDbContext(IMongoDatabase database)
{
    public IMongoCollection<T> GetCollection<T>(string? collectionName = null) where T : class
    {
        return database.GetCollection<T>(collectionName ?? typeof(T).Name.ToLowerInvariant());
    }
}