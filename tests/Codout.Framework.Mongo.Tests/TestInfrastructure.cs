using System.Reflection;
using Codout.Framework.Data.Entity;
using EphemeralMongo;
using MongoDB.Bson;
using MongoDB.Driver;
using Xunit;

namespace Codout.Framework.Mongo.Tests;

// ---------------------------------------------------------------------------
// Infraestrutura compartilhada pelos testes de MongoRepository / MongoUnitOfWork.
// Os testes de integração usam EphemeralMongo (baixa o binário do mongod em
// runtime) com replica set de nó único — necessário para transações. Se o
// mongod não subir neste ambiente, a fixture captura a falha e os testes de
// integração ([SkippableFact]) são marcados como SKIPPED com a razão.
// ---------------------------------------------------------------------------

/// <summary>
/// Entidade de teste com Id ObjectId — o MongoRepository só consegue resolver
/// chaves que parseiam como ObjectId (ver FINDINGS-D.md).
/// </summary>
public class Gadget : IEntity<ObjectId>
{
    public ObjectId Id { get; set; }
    public string Name { get; set; } = "";
    public int Price { get; set; }

    public IEnumerable<PropertyInfo> GetSignatureProperties() => [];

    public bool IsTransient() => Id == ObjectId.Empty;
}

/// <summary>
/// Fixture de coleção: sobe um único mongod efêmero (replica set de nó único)
/// para todos os testes de integração. Indisponibilidade vira SkipReason.
/// </summary>
public sealed class MongoFixture : IDisposable
{
    private readonly IMongoRunner? _runner;

    public IMongoClient? Client { get; }
    public IMongoDatabase? Database { get; }
    public string SkipReason { get; } = "";

    public bool IsAvailable => Database != null;

    public MongoFixture()
    {
        try
        {
            var options = new MongoRunnerOptions
            {
                UseSingleNodeReplicaSet = true, // transações exigem replica set
                StandardOutputLogger = _ => { },
                StandardErrorLogger = _ => { },
            };

            _runner = MongoRunner.Run(options);
            var client = new MongoClient(_runner.ConnectionString);
            var database = client.GetDatabase("codout_mongo_tests");

            // Garante que o servidor responde antes de liberar os testes.
            database.RunCommand<BsonDocument>(new BsonDocument("ping", 1));

            Client = client;
            Database = database;
        }
        catch (Exception ex)
        {
            SkipReason = "mongod efêmero (EphemeralMongo) indisponível neste ambiente: " +
                         $"{ex.GetType().Name}: {ex.Message}";
            _runner?.Dispose();
            _runner = null;
        }
    }

    /// <summary>Marca o teste como SKIPPED quando o mongod não subiu.</summary>
    public void EnsureAvailable() => Skip.IfNot(IsAvailable, SkipReason);

    /// <summary>Coleção usada pelo MongoRepository&lt;Gadget&gt; (nome em minúsculas).</summary>
    public IMongoCollection<Gadget> GadgetCollection =>
        Database!.GetCollection<Gadget>("gadget");

    public MongoRepository<Gadget> CreateRepository() => new(Database!);

    public void ResetCollection() =>
        GadgetCollection.DeleteMany(FilterDefinition<Gadget>.Empty);

    /// <summary>Insere um Gadget direto na coleção, por fora do repositório.</summary>
    public Gadget Seed(string name, int price = 0)
    {
        var gadget = new Gadget { Name = name, Price = price };
        GadgetCollection.InsertOne(gadget);
        return gadget;
    }

    public void Dispose() => _runner?.Dispose();
}

[CollectionDefinition("Mongo")]
public class MongoCollection : ICollectionFixture<MongoFixture>;
