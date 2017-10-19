using Codout.Framework.NetStandard.Domain.Entity;
using Codout.Framework.NetStandard.Repository;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using System.Reflection;
using Microsoft.Azure.Documents;

namespace Codout.Framework.NetCore.Repository.Cosmos
{
    /// <summary>
    /// Repositório genérico de dados para Azure CosmosDB
    /// </summary>
    /// <typeparam name="T">Classe que define o tipo do repositório</typeparam>
    public class CosmosRepository<T> : IRepository<T> where T : class, IEntity
    {
        private static DocumentClient client;
        private static string DatabaseId;
        private static string CollectionId;

        public CosmosRepository(string endPoint, string key, string databaseId, string collectionId)
        {
            client = new DocumentClient(new Uri(endPoint), key, new ConnectionPolicy { EnableEndpointDiscovery = false });
            DatabaseId = databaseId;
            CollectionId = collectionId;
        }

        /// <inheritdoc />
        public IQueryable<T> All()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Retorna todos os objetos do repositório (pode ser lento)
        /// </summary>
        /// <returns>Lista de objetos</returns>
        public async Task<IQueryable<T>> AllAsync()
        {
            IDocumentQuery<T> query = client.CreateDocumentQuery<T>(
                UriFactory.CreateDocumentCollectionUri(DatabaseId, CollectionId),
                new FeedOptions { MaxItemCount = -1 })
                .AsDocumentQuery();

            List<T> results = new List<T>();
            while (query.HasMoreResults)
            {
                results.AddRange(await query.ExecuteNextAsync<T>());
            }

            return results.AsQueryable<T>();
        }

        public void Delete(T entity)
        {
        }

        public void Delete(Expression<Func<T, bool>> predicate)
        {
            throw new NotImplementedException();
        }

        public async Task DeleteAsync(T entity)
        {
            await client.DeleteDocumentAsync(UriFactory.CreateDocumentUri(DatabaseId, CollectionId, GetIdValue(entity).ToString()));                
        }

        public Task DeleteAsync(Expression<Func<T, bool>> predicate)
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

        public IQueryable<T> Find(Expression<Func<T, bool>> predicate)
        {
            throw new NotImplementedException();
        }

        public IQueryable<T> Find(Expression<Func<T, bool>> filter, out int total, int index = 0, int size = 50)
        {
            throw new NotImplementedException();
        }

        public Task<IQueryable<T>> FindAsync(Expression<Func<T, bool>> predicate)
        {
            return AllAsync().Result.Where(predicate);
        }

        public Task<IQueryable<T>> FindAsync(Expression<Func<T, bool>> filter, out int total, int index = 0, int size = 50)
        {
            throw new NotImplementedException();
        }

        public T Get(Expression<Func<T, bool>> predicate)
        {
            throw new NotImplementedException();
        }

        public T Get(object key)
        {
            throw new NotImplementedException();
        }

        public Task<T> GetAsync(Expression<Func<T, bool>> predicate)
        {
            throw new NotImplementedException();
        }

        public Task<T> GetAsync(object key)
        {
            throw new NotImplementedException();
        }

        public T Load(object key)
        {
            throw new NotImplementedException();
        }

        public Task<T> LoadAsync(object key)
        {
            throw new NotImplementedException();
        }

        public T Merge(T entity)
        {
            throw new NotImplementedException();
        }

        public Task<T> MergeAsync(T entity)
        {
            throw new NotImplementedException();
        }

        public T Save(T entity)
        {
            throw new NotImplementedException();
        }

        public Task<T> SaveAsync(T entity)
        {
            throw new NotImplementedException();
        }

        public T SaveOrUpdate(T entity)
        {
            throw new NotImplementedException();
        }

        public Task<T> SaveOrUpdateAsync(T entity)
        {
            throw new NotImplementedException();
        }

        public void Update(T entity)
        {
            throw new NotImplementedException();
        }

        public Task UpdateAsync(T entity)
        {
            throw new NotImplementedException();
        }

        public static object GetIdValue(T entity)
        {
            return entity.GetType().GetTypeInfo().GetProperty("Id").GetValue(entity);
        }
    }
}
