using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Codout.Framework.Data.Entity;
using Codout.Framework.Data.Repository;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Codout.Framework.Mongo;

/// <summary>
/// Repositório genérico de dados para MongoDB
/// </summary>
/// <typeparam name="T">Classe que define o tipo do repositório</typeparam>
public class MongoRepository<T>(IMongoDatabase database) : IRepository<T> where T : class, IEntity
{
    private bool _disposed;
    private readonly IMongoCollection<T> _collection = database.GetCollection<T>(typeof(T).Name.ToLowerInvariant());

    #region Synchronous Query Methods

    /// <summary>
    /// Retorna todos os objetos do repositório (pode ser lento)
    /// </summary>
    public virtual IQueryable<T> All()
    {
        return _collection.AsQueryable();
    }

    public virtual IQueryable<T> AllReadOnly()
    {
        return _collection.AsQueryable();
    }

    /// <summary>
    /// Retorna uma lista de objetos do repositório de acordo com o filtro apresentado
    /// </summary>
    public virtual IQueryable<T> Where(Expression<Func<T, bool>> predicate)
    {
        return All().Where(predicate);
    }

    public virtual IQueryable<T> WhereReadOnly(Expression<Func<T, bool>> predicate)
    {
        return _collection.AsQueryable().Where(predicate);
    }

    /// <summary>
    /// Retorna uma lista de objetos do repositório de acordo com o filtro e com opção de paginação
    /// </summary>
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
    /// Retorna um objeto do repositório de acordo com o filtro
    /// </summary>
    public virtual T Get(Expression<Func<T, bool>> predicate)
    {
        return Where(predicate).SingleOrDefault();
    }

    /// <summary>
    /// Retorna um objeto de acordo com a Key
    /// </summary>
    public virtual T Get(object key)
    {
        if (!ObjectId.TryParse(key?.ToString(), out var id))
            return null;

        return _collection.Find(Builders<T>.Filter.Eq("_id", BsonValue.Create(id))).FirstOrDefault();
    }

    /// <summary>
    /// Efetua a carga do objeto conforme a key
    /// </summary>
    public virtual T Load(object key)
    {
        return Get(key);
    }

    #endregion

    #region Synchronous Command Methods

    /// <summary>
    /// Delete o objeto indicado do repositório de dados
    /// </summary>
    public virtual void Delete(T entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        _collection.DeleteOne(Builders<T>.Filter.Eq("_id", BsonValue.Create(GetIdValue(entity))));
    }

    /// <summary>
    /// Exclui uma lista de objetos conforme o filtro
    /// </summary>
    public virtual void Delete(Expression<Func<T, bool>> predicate)
    {
        _collection.DeleteMany(Builders<T>.Filter.Where(predicate));
    }

    /// <summary>
    /// Salva o objeto no repositório
    /// </summary>
    public virtual T Save(T entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        _collection.InsertOne(entity);
        return entity;
    }

    /// <summary>
    /// Salva ou atualiza o objeto em questão
    /// </summary>
    public virtual T SaveOrUpdate(T entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        
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
    /// Atualiza o objeto no repositório
    /// </summary>
    public virtual void Update(T entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        _collection.ReplaceOne(Builders<T>.Filter.Eq("_id", BsonValue.Create(GetIdValue(entity))), entity);
    }

    /// <summary>
    /// Efetua o Merge do objeto no repositório (MongoDB usa ReplaceOne)
    /// </summary>
    public virtual T Merge(T entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        Update(entity);
        return entity;
    }

    /// <summary>
    /// Refresh do objeto (re-carrega do banco)
    /// </summary>
    public virtual T Refresh(T entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        return Get(GetIdValue(entity));
    }

    #endregion

    #region Asynchronous Query Methods

    public virtual async Task<T> GetAsync(Expression<Func<T, bool>> predicate)
    {
        return await GetAsync(predicate, CancellationToken.None);
    }

    public virtual async Task<T> GetAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken)
    {
        return await (await _collection.FindAsync(predicate, cancellationToken: cancellationToken))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public virtual async Task<T> GetAsync(object key)
    {
        return await GetAsync(key, CancellationToken.None);
    }

    public virtual async Task<T> GetAsync(object key, CancellationToken cancellationToken)
    {
        if (!ObjectId.TryParse(key?.ToString(), out var id))
            return null;

        return await (await _collection.FindAsync(
                Builders<T>.Filter.Eq("_id", BsonValue.Create(id)), 
                cancellationToken: cancellationToken))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public virtual Task<T> LoadAsync(object key)
    {
        return GetAsync(key);
    }

    public virtual Task<T> LoadAsync(object key, CancellationToken cancellationToken)
    {
        return GetAsync(key, cancellationToken);
    }

    public virtual async Task<T> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await (await _collection.FindAsync(predicate, cancellationToken: cancellationToken))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public virtual async Task<bool> AnyAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
    {
        var count = await _collection.CountDocumentsAsync(predicate, cancellationToken: cancellationToken);
        return count > 0;
    }

    public virtual async Task<int> CountAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return (int)await _collection.CountDocumentsAsync(predicate, cancellationToken: cancellationToken);
    }

    public virtual async Task<List<T>> ToListAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await (await _collection.FindAsync(predicate, cancellationToken: cancellationToken))
            .ToListAsync(cancellationToken);
    }

    #endregion

    #region Asynchronous Command Methods

    public virtual Task DeleteAsync(T entity)
    {
        return DeleteAsync(entity, CancellationToken.None);
    }

    public virtual async Task DeleteAsync(T entity, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(entity);
        await _collection.DeleteOneAsync(
            Builders<T>.Filter.Eq("_id", BsonValue.Create(GetIdValue(entity))), 
            cancellationToken);
    }

    public virtual Task DeleteAsync(Expression<Func<T, bool>> predicate)
    {
        return DeleteAsync(predicate, CancellationToken.None);
    }

    public virtual async Task DeleteAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken)
    {
        await _collection.DeleteManyAsync(Builders<T>.Filter.Where(predicate), cancellationToken);
    }

    public virtual Task<T> SaveAsync(T entity)
    {
        return SaveAsync(entity, CancellationToken.None);
    }

    public virtual async Task<T> SaveAsync(T entity, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(entity);
        await _collection.InsertOneAsync(entity, cancellationToken: cancellationToken);
        return entity;
    }

    public virtual Task<T> SaveOrUpdateAsync(T entity)
    {
        return SaveOrUpdateAsync(entity, CancellationToken.None);
    }

    public virtual async Task<T> SaveOrUpdateAsync(T entity, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(entity);
        
        if (entity.IsTransient())
        {
            await _collection.InsertOneAsync(entity, cancellationToken: cancellationToken);
        }
        else
        {
            await _collection.ReplaceOneAsync(
                Builders<T>.Filter.Eq("_id", BsonValue.Create(GetIdValue(entity))), 
                entity,
                cancellationToken: cancellationToken);
        }

        return entity;
    }

    public virtual Task UpdateAsync(T entity)
    {
        return UpdateAsync(entity, CancellationToken.None);
    }

    public virtual async Task UpdateAsync(T entity, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(entity);
        await _collection.ReplaceOneAsync(
            Builders<T>.Filter.Eq("_id", BsonValue.Create(GetIdValue(entity))), 
            entity,
            cancellationToken: cancellationToken);
    }

    public virtual Task<T> MergeAsync(T entity)
    {
        return MergeAsync(entity, CancellationToken.None);
    }

    public virtual async Task<T> MergeAsync(T entity, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(entity);
        await UpdateAsync(entity, cancellationToken);
        return entity;
    }

    public virtual Task<T> RefreshAsync(T entity)
    {
        return RefreshAsync(entity, CancellationToken.None);
    }

    public virtual async Task<T> RefreshAsync(T entity, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(entity);
        return await GetAsync(GetIdValue(entity), cancellationToken);
    }

    #endregion

    #region Include Methods

    /// <summary>
    /// MongoDB não suporta Include nativo como EF Core
    /// </summary>
    public virtual IQueryable<T> IncludeMany(params Expression<Func<T, object>>[] includes)
    {
        // MongoDB não tem suporte a includes como EF Core
        // Retorna o queryable normal - joins devem ser feitos via agregações ou lookups
        return _collection.AsQueryable();
    }

    #endregion

    #region IDisposable

    public virtual void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                // MongoDB não requer dispose de collections
            }

            _disposed = true;
        }
    }

    #endregion

    #region Helper Methods

    private static object GetIdValue(T entity)
    {
        var key = entity.GetType().GetTypeInfo().GetProperty("Id")?.GetValue(entity);
        
        if (key == null)
            return null;

        if (ObjectId.TryParse(key.ToString(), out var id))
            return id;

        return key;
    }

    #endregion
}