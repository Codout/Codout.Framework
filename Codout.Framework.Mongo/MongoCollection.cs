using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using Codout.Framework.DAL.Entity;
using Codout.Framework.DAL.Repository;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Codout.Framework.Mongo;

/// <summary>
/// Repositório genérico de dados para MongoDB
/// </summary>
/// <typeparam name="T">Classe que define o tipo do repositório</typeparam>
public class MongoCollection<T>(IMongoDatabase mongoDatabase) : IRepository<T>
    where T : class, IEntity
{
    
    public readonly IMongoDatabase MongoDatabase = mongoDatabase;
    
    public IMongoCollection<TEntity> GetCollection<TEntity>()
    {
        return MongoDatabase.GetCollection<TEntity>(typeof(TEntity).Name.ToLower());
    }
    
    /// <summary>
    /// Retorna todos os objetos do repositório (pode ser lento)
    /// </summary>
    /// <returns>Lista de objetos</returns>
    public IQueryable<T> All()
    {
        return GetCollection<T>().AsQueryable();
    }

    /// <summary>
    /// Delete o objeto indicado do repositório de dados
    /// </summary>
    /// <param name="entity">Objeto a ser deletado</param>
    public void Delete(T entity)
    {
        GetCollection<T>().DeleteOne(Builders<T>.Filter.Eq("_id", BsonValue.Create(GetIdValue(entity))));
    }

    /// <summary>
    /// Exclui uma lista de objetos conforme o filtro
    /// </summary>
    /// <param name="predicate">Filtro de objetos a serem deletados</param>
    public void Delete(Expression<Func<T, bool>> predicate)
    {
        GetCollection<T>().DeleteMany(Builders<T>.Filter.Where(predicate));
    }

    private bool _disposed;

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                // Libera os componentes
            }
        }
        _disposed = true;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Retorna uma lista de objetos do repositório de acordo com o filtro apresentado
    /// </summary>
    /// <param name="predicate">Lista de objetos</param>
    /// <returns></returns>
    public IQueryable<T> Find(Expression<Func<T, bool>> predicate)
    {
        return All().Where(predicate);
    }

    /// <summary>
    /// Retorna uma lista de objetos do repositório de acordo com o filtro e com opção de paginação
    /// </summary>
    /// <param name="filter">Filtro de bojetos</param>
    /// <param name="total">Retorna o todal de objetos</param>
    /// <param name="index">Indica o índice da paginação</param>
    /// <param name="size">Tamanho da página</param>
    /// <returns>Lista de objetos</returns>
    public IQueryable<T> Find(Expression<Func<T, bool>> filter, out int total, int index = 0, int size = 50)
    {
        var skipCount = index * size;

        var resetSet = filter != null
            ? Find(filter).AsQueryable()
            : GetCollection<T>().AsQueryable();

        resetSet = skipCount == 0
            ? resetSet.Take(size)
            : resetSet.Skip(skipCount).Take(size);

        total = resetSet.Count();

        return resetSet.AsQueryable();
    }

    /// <summary>
    /// Retorna um objeto do repositório de acordo com o filtro
    /// </summary>
    /// <param name="predicate">Filtro</param>
    /// <returns>objeto</returns>
    public T Get(Expression<Func<T, bool>> predicate)
    {
        return Find(predicate).SingleOrDefault();
    }

    /// <summary>
    /// Retorna um objeto de acordo com a Key
    /// </summary>
    /// <param name="key">Key do objeto</param>
    /// <returns>objeto</returns>
    public T Get(object key)
    {
        return GetCollection<T>().Find(Builders<T>.Filter.Eq("_id", BsonValue.Create(key))).FirstOrDefault();
    }

    /// <summary>
    /// Efetua a carga do objeto conforme a key
    /// </summary>
    /// <param name="key">Key do objeto</param>
    /// <returns>Objeto</returns>
    public T Load(object key)
    {
        return Get(key);
    }

    /// <summary>
    /// Efetua o Merge do objeto no repositório
    /// </summary>
    /// <param name="entity">Objeto a ser mesclado</param>
    /// <returns>Retorna o mesmo objeto, para o caso de retornar algum Id gerado</returns>
    public T Merge(T entity)
    {
        Update(entity);
        return entity;
    }

    public T Refresh(T entity)
    {
        return Get(GetIdValue(entity));
    }

    public async Task<T> GetAsync(Expression<Func<T, bool>> predicate)
    {
        return await (await GetCollection<T>().FindAsync(predicate)).FirstOrDefaultAsync();
    }

    public async Task<T> GetAsync(object key)
    {
        return await (await GetCollection<T>().FindAsync(Builders<T>.Filter.Eq("_id", BsonValue.Create(key)))).FirstOrDefaultAsync();
    }

    public async Task<T> LoadAsync(object key)
    {
        return await GetAsync(key);
    }

    public async Task DeleteAsync(T entity)
    {
        await GetCollection<T>().DeleteOneAsync(Builders<T>.Filter.Eq("_id", BsonValue.Create(GetIdValue(entity))));
    }

    public async Task DeleteAsync(Expression<Func<T, bool>> predicate)
    {
        await GetCollection<T>().DeleteManyAsync(Builders<T>.Filter.Where(predicate));
    }

    public async Task<T> SaveAsync(T entity)
    {
        await GetCollection<T>().InsertOneAsync(entity);
        return entity;
    }

    public async Task<T> SaveOrUpdateAsync(T entity)
    {
        if (entity.IsTransient())
            await GetCollection<T>().InsertOneAsync(entity);
        else
            await GetCollection<T>().ReplaceOneAsync(Builders<T>.Filter.Eq("_id", BsonValue.Create(GetIdValue(entity))), entity);

        return entity;
    }

    public async Task UpdateAsync(T entity)
    {
        await GetCollection<T>().ReplaceOneAsync(Builders<T>.Filter.Eq("_id", BsonValue.Create(GetIdValue(entity))), entity);
    }

    public async Task<T> MergeAsync(T entity)
    {
        await UpdateAsync(entity);
        return entity;
    }

    public Task<T> RefreshAsync(T entity)
    {
        return GetAsync(GetIdValue(entity));
    }

    /// <summary>
    /// Salva o objeto no repositório
    /// </summary>
    /// <param name="entity">Objeto a ser salvo</param>
    /// <returns>Retorna o mesmo objeto, para o caso de retornar algum Id gerado</returns>
    public T Save(T entity)
    {
        GetCollection<T>().InsertOne(entity);
        return entity;
    }


    /// <summary>
    /// Salva ou atualiza o objeto em questão (USAR SOMENTE SE O ID NÃO FOI SETADO)
    /// </summary>
    /// <param name="entity">Objeto a ser salvo/atualizado</param>
    /// <returns>Retorna o mesmo objeto, para o caso de retornar algum Id gerado</returns>
    public T SaveOrUpdate(T entity)
    {
        if (entity.IsTransient())
        {
            GetCollection<T>().InsertOne(entity);
        }
        else
        {
            var filter = Builders<T>.Filter.Eq("_id", BsonValue.Create(GetIdValue(entity)));
            GetCollection<T>().ReplaceOne(filter, entity);
        }

        return entity;
    }

    /// <summary>
    /// Atualiza o objeto no repositório
    /// </summary>
    /// <param name="entity">Objeto a ser atulizado</param>
    public void Update(T entity)
    {
        GetCollection<T>().ReplaceOne(Builders<T>.Filter.Eq("_id", BsonValue.Create(GetIdValue(entity))), entity);
    }

    public static object GetIdValue(T entity)
    {
        return entity.GetType().GetTypeInfo().GetProperty("Id").GetValue(entity);
    }
}
