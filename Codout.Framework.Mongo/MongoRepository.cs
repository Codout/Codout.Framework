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
///     Repositório genérico de dados para MongoDB
/// </summary>
/// <typeparam name="T">Classe que define o tipo do repositório</typeparam>
public class MongoRepository<T>(MongoDbContext context) : IRepository<T>
    where T : class, IEntity
{
    private readonly IMongoCollection<T> _collection = context.GetCollection<T>();

    private bool _disposed;

    /// <summary>
    ///     Retorna todos os objetos do repositório (pode ser lento)
    /// </summary>
    /// <returns>Lista de objetos</returns>
    public virtual IQueryable<T> All()
    {
        return _collection.AsQueryable();
    }

    public virtual IQueryable<T> AllReadOnly()
    {
        return _collection.AsQueryable();
    }

    /// <summary>
    ///     Delete o objeto indicado do repositório de dados
    /// </summary>
    /// <param name="entity">Objeto a ser deletado</param>
    public virtual void Delete(T entity)
    {
        _collection.DeleteOne(Builders<T>.Filter.Eq("_id", BsonValue.Create(GetIdValue(entity))));
    }

    /// <summary>
    ///     Exclui uma lista de objetos conforme o filtro
    /// </summary>
    /// <param name="predicate">Filtro de objetos a serem deletados</param>
    public virtual void Delete(Expression<Func<T, bool>> predicate)
    {
        _collection.DeleteMany(Builders<T>.Filter.Where(predicate));
    }

    public virtual void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    ///     Retorna uma lista de objetos do repositório de acordo com o filtro apresentado
    /// </summary>
    /// <param name="predicate">Lista de objetos</param>
    /// <returns></returns>
    public virtual IQueryable<T> Where(Expression<Func<T, bool>> predicate)
    {
        return All().Where(predicate);
    }

    public virtual IQueryable<T> WhereReadOnly(Expression<Func<T, bool>> predicate)
    {
        return _collection.AsQueryable().Where(predicate);
    }

    /// <summary>
    ///     Retorna uma lista de objetos do repositório de acordo com o filtro e com opção de paginação
    /// </summary>
    /// <param name="filter">Filtro de bojetos</param>
    /// <param name="total">Retorna o todal de objetos</param>
    /// <param name="index">Indica o índice da paginação</param>
    /// <param name="size">Tamanho da página</param>
    /// <returns>Lista de objetos</returns>
    public virtual IQueryable<T> WherePaged(Expression<Func<T, bool>> filter, out int total, int index = 0, int size = 50)
    {
        var skipCount = index * size;

        var resetSet = filter != null
            ? Where(filter).AsQueryable()
            : _collection.AsQueryable();

        resetSet = skipCount == 0
            ? resetSet.Take(size)
            : resetSet.Skip(skipCount).Take(size);

        total = resetSet.Count();

        return resetSet.AsQueryable();
    }

    /// <summary>
    ///     Retorna um objeto do repositório de acordo com o filtro
    /// </summary>
    /// <param name="predicate">Filtro</param>
    /// <returns>objeto</returns>
    public virtual T Get(Expression<Func<T, bool>> predicate)
    {
        return Where(predicate).SingleOrDefault();
    }

    /// <summary>
    ///     Retorna um objeto de acordo com a Key
    /// </summary>
    /// <param name="key">Key do objeto</param>
    /// <returns>objeto</returns>
    public virtual T Get(object key)
    {
        if (!ObjectId.TryParse(key.ToString(), out var id))
            return null;

        return _collection.Find(Builders<T>.Filter.Eq("_id", BsonValue.Create(id))).FirstOrDefault();
    }

    /// <summary>
    ///     Efetua a carga do objeto conforme a key
    /// </summary>
    /// <param name="key">Key do objeto</param>
    /// <returns>Objeto</returns>
    public virtual T Load(object key)
    {
        throw new NotSupportedException();
    }

    /// <summary>
    ///     Efetua o Merge do objeto no repositório
    /// </summary>
    /// <param name="entity">Objeto a ser mesclado</param>
    /// <returns>Retorna o mesmo objeto, para o caso de retornar algum Id gerado</returns>
    public virtual T Merge(T entity)
    {
        throw new NotSupportedException();
    }

    public virtual T Refresh(T entity)
    {
        throw new NotSupportedException();
    }

    public virtual async Task<T> GetAsync(Expression<Func<T, bool>> predicate)
    {
        return await (await _collection.FindAsync(predicate)).FirstOrDefaultAsync();
    }

    public virtual async Task<T> GetAsync(object key)
    {
        if (!ObjectId.TryParse(key.ToString(), out var id))
            return null;

        return await (await _collection.FindAsync(Builders<T>.Filter.Eq("_id", BsonValue.Create(id))))
            .FirstOrDefaultAsync();
    }

    public Task<T> LoadAsync(object key)
    {
        throw new NotSupportedException();
    }

    public virtual async Task DeleteAsync(T entity)
    {
        await _collection.DeleteOneAsync(Builders<T>.Filter.Eq("_id", BsonValue.Create(GetIdValue(entity))));
    }

    public virtual async Task DeleteAsync(Expression<Func<T, bool>> predicate)
    {
        await _collection.DeleteManyAsync(Builders<T>.Filter.Where(predicate));
    }

    public virtual async Task<T> SaveAsync(T entity)
    {
        await _collection.InsertOneAsync(entity);
        return entity;
    }

    public virtual async Task<T> SaveOrUpdateAsync(T entity)
    {
        if (entity.IsTransient())
            await _collection.InsertOneAsync(entity);
        else
            await _collection
                .ReplaceOneAsync(Builders<T>.Filter.Eq("_id", BsonValue.Create(GetIdValue(entity))), entity);

        return entity;
    }

    public virtual async Task UpdateAsync(T entity)
    {
        await _collection
            .ReplaceOneAsync(Builders<T>.Filter.Eq("_id", BsonValue.Create(GetIdValue(entity))), entity);
    }

    public virtual Task<T> MergeAsync(T entity)
    {
        throw new NotSupportedException();
    }

    public virtual Task<T> RefreshAsync(T entity)
    {
        throw new NotSupportedException();
    }

    public virtual IQueryable<T> IncludeMany(params Expression<Func<T, object>>[] includes)
    {
        throw new NotSupportedException();
    }

    /// <summary>
    ///     Salva o objeto no repositório
    /// </summary>
    /// <param name="entity">Objeto a ser salvo</param>
    /// <returns>Retorna o mesmo objeto, para o caso de retornar algum Id gerado</returns>
    public virtual T Save(T entity)
    {
        _collection.InsertOne(entity);
        return entity;
    }

    /// <summary>
    ///     Salva ou atualiza o objeto em questão (USAR SOMENTE SE O ID NÃO FOI SETADO)
    /// </summary>
    /// <param name="entity">Objeto a ser salvo/atualizado</param>
    /// <returns>Retorna o mesmo objeto, para o caso de retornar algum Id gerado</returns>
    public virtual T SaveOrUpdate(T entity)
    {
        if (entity.IsTransient())
        {
            _collection.InsertOne(entity);
        }
        else
        {
            var filter = Builders<T>.Filter.Eq("_id", BsonValue.Create(GetIdValue(entity)));
            _collection.ReplaceOne(filter, entity);
        }

        return entity;
    }

    /// <summary>
    ///     Atualiza o objeto no repositório
    /// </summary>
    /// <param name="entity">Objeto a ser atulizado</param>
    public virtual void Update(T entity)
    {
        _collection.ReplaceOne(Builders<T>.Filter.Eq("_id", BsonValue.Create(GetIdValue(entity))), entity);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
            if (disposing)
            {
                // Libera os componentes
            }

        _disposed = true;
    }

    private static object GetIdValue(T entity)
    {
        var key = entity.GetType().GetTypeInfo().GetProperty("Id").GetValue(entity);
        if (!ObjectId.TryParse(key.ToString(), out var id))
            return null;
        return id;
    }
}