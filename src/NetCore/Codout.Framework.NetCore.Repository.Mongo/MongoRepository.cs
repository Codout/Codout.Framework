using MongoDB.Bson;
using MongoDB.Driver;
using Codout.Framework.NetStandard.Domain.Entity;
using Codout.Framework.NetStandard.Repository;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

namespace Codout.Framework.NetCore.Repository.Mongo
{

    /// <summary>
    /// Repositório genérico de dados para MongoDB
    /// </summary>
    /// <typeparam name="T">Classe que define o tipo do repositório</typeparam>
    public class MongoRepository<T> : IRepository<T> where T : class, IEntity
    {

        public MongoDbContext MongoDbContext;

        public MongoRepository(MongoDbContext mongoDbContext)
        {
            MongoDbContext = mongoDbContext;
        }

        public IMongoCollection<TEntity> GetCollection<TEntity>()
        {
            return MongoDbContext.Database.GetCollection<TEntity>(typeof(TEntity).Name.ToLower() + "s");
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
        /// Retorna todos os objetos do repositório (pode ser lento)
        /// </summary>
        /// <returns>Lista de objetos</returns>
        public Task<IQueryable<T>> AllAsync()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Delete o objeto indicado do repositório de dados
        /// </summary>
        /// <param name="entity">Objeto a ser deletado</param>
        public void Delete(T entity)
        {
            var filter = Builders<T>.Filter.Eq("_id", BsonValue.Create(GetIdValue(entity)));
            GetCollection<T>().DeleteOne(filter);
        }

        /// <summary>
        /// Deletra uma lista de objetos confrome o filtro
        /// </summary>
        /// <param name="predicate">Filtro de objetos a serem deletados</param>
        public void Delete(Expression<Func<T, bool>> predicate)
        {
            GetCollection<T>().DeleteMany(Builders<T>.Filter.Where(predicate));
        }

        /// <summary>
        /// Delete o objeto indicado do repositório de dados
        /// </summary>
        /// <param name="entity">Objeto a ser deletado</param>
        public Task<bool> DeleteAsync(T entity)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Deletra uma lista de objetos confrome o filtro
        /// </summary>
        /// <param name="predicate">Filtro de objetos a serem deletados</param>
        public Task<bool> DeleteAsync(Expression<Func<T, bool>> predicate)
        {
            throw new NotImplementedException();
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
        /// Retorna uma lista de objetos do repositório de acordo com o filtro apresentado
        /// </summary>
        /// <param name="predicate">Lista de objetos</param>
        /// <returns></returns>
        public Task<IQueryable<T>> FindAsync(Expression<Func<T, bool>> predicate)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Retorna uma lista de objetos do repositório de acordo com o filtro e com opção de paginação
        /// </summary>
        /// <param name="filter">Filtro de bojetos</param>
        /// <param name="total">Retorna o todal de objetos</param>
        /// <param name="index">Indica o índice da paginação</param>
        /// <param name="size">Tamanho da página</param>
        /// <returns>Lista de objetos</returns>
        public Task<IQueryable<T>> FindAsync(Expression<Func<T, bool>> filter, out int total, int index = 0, int size = 50)
        {
            throw new NotImplementedException();
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
            var filter = Builders<T>.Filter.Eq("_id", BsonValue.Create(key));
            var entity = GetCollection<T>().Find(filter).FirstOrDefault();
            return entity;
        }

        /// <summary>
        /// Retorna um objeto do repositório de acordo com o filtro
        /// </summary>
        /// <param name="predicate">Filtro</param>
        /// <returns>objeto</returns>
        public Task<T> GetAsync(Expression<Func<T, bool>> predicate)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Retorna um objeto de acordo com a Key
        /// </summary>
        /// <param name="key">Key do objeto</param>
        /// <returns>objeto</returns>
        public Task<T> GetAsync(object key)
        {
            throw new NotImplementedException();
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
        /// Efetua a carga do objeto conforme a key
        /// </summary>
        /// <param name="key">Key do objeto</param>
        /// <returns>Objeto</returns>
        public Task<T> LoadAsync(object key)
        {
            throw new NotImplementedException();
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

        /// <summary>
        /// Efetua o Merge do objeto no repositório
        /// </summary>
        /// <param name="entity">Objeto a ser mesclado</param>
        /// <returns>Retorna o mesmo objeto, para o caso de retornar algum Id gerado</returns>
        public Task<T> MergeAsync(T entity)
        {
            throw new NotImplementedException();
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
        /// Salva o objeto no repositório
        /// </summary>
        /// <param name="entity">Objeto a ser salvo</param>
        /// <returns>Retorna o mesmo objeto, para o caso de retornar algum Id gerado</returns>
        public Task<T> SaveAsync(T entity)
        {
            throw new NotImplementedException();
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
        /// Salva ou atualiza o objeto em questão (USAR SOMENTE SE O ID NÃO FOI SETADO)
        /// </summary>
        /// <param name="entity">Objeto a ser salvo/atualizado</param>
        /// <returns>Retorna o mesmo objeto, para o caso de retornar algum Id gerado</returns>
        public Task<T> SaveOrUpdateAsync(T entity)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Atualiza o objeto no repositório
        /// </summary>
        /// <param name="entity">Objeto a ser atulizado</param>
        public void Update(T entity)
        {
            var filter = Builders<T>.Filter.Eq("_id", BsonValue.Create(GetIdValue(entity)));
            GetCollection<T>().ReplaceOne(filter, entity);
        }

        /// <summary>
        /// Atualiza o objeto no repositório
        /// </summary>
        /// <param name="entity">Objeto a ser atulizado</param>
        public Task<bool> UpdateAsync(T entity)
        {
            throw new NotImplementedException();
        }

        public static object GetIdValue(T entity)
        {
            return entity.GetType().GetTypeInfo().GetProperty("Id").GetValue(entity);
        }
    }
}
