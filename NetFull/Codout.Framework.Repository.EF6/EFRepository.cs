using System;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Codout.Framework.DAL.Entity;
using Codout.Framework.DAL.Repository;

namespace Codout.Framework.EF6
{

    /// <summary>
    /// Repositório genérico de dados para EntityFramework Full (6.1.3)
    /// </summary>
    /// <typeparam name="T">Classe que define o tipo do repositório</typeparam>
    public class EFRepository<T> : IRepository<T>
        where T : class, IEntity
    {
        protected DbContext Context = null;

        public EFRepository(DbContext context)
        {
            Context = context;
        }

        protected DbSet<T> DbSet => Context.Set<T>();

        /// <summary>
        /// Retorna todos os objetos do repositório (pode ser lento)
        /// </summary>
        /// <returns>Lista de objetos</returns>
        public IQueryable<T> All()
        {
            return DbSet.AsQueryable();
        }

        /// <summary>
        /// Retorna uma lista de objetos do repositório de acordo com o filtro apresentado
        /// </summary>
        /// <param name="predicate">Lista de objetos</param>
        /// <returns></returns>
        public IQueryable<T> Find(Expression<Func<T, bool>> predicate)
        {
            return DbSet.Where(predicate).AsQueryable();
        }

        /// <summary>
        /// Retorna uma lista de objetos do repositório de acordo com o filtro e com opção de paginação
        /// </summary>
        /// <param name="filter">Filtro de bojetos</param>
        /// <param name="total">Retorna o todal de objetos</param>
        /// <param name="index">Indica o índice da paginação</param>
        /// <param name="size">Tamanho da página</param>
        /// <returns>Lista de objetos</returns>
        public IQueryable<T> Find(Expression<Func<T, bool>> filter, out int total, int index = 0, int size = 50 )
        {
            var skipCount = index * size;

            var resetSet = filter != null
                ? DbSet.Where(filter).AsQueryable()
                : DbSet.AsQueryable();

            resetSet = skipCount == 0
                ? resetSet.Take(size)
                : resetSet.Skip(skipCount).Take(size);

            total = resetSet.Count();

            return resetSet.AsQueryable();
        }

        /// <summary>
        /// Retorna um objeto do repositório de acordo com o filtro (não usar para ID nullabe)
        /// </summary>
        /// <param name="predicate">Filtro</param>
        /// <returns>objeto</returns>
        public T Get(Expression<Func<T, bool>> predicate)
        {
            return DbSet.Find(predicate);
        }

        /// <summary>
        /// Retorna um objeto de acordo com a Key
        /// </summary>
        /// <param name="key">Key do objeto</param>
        /// <returns>objeto</returns>
        public T Get(object key)
        {
            return DbSet.Find(new[] { key });
        }

        /// <summary>
        /// Efetua a carga do objeto conforme a key
        /// </summary>
        /// <param name="key">Key do objeto</param>
        /// <returns>Objeto</returns>
        public T Load(object keys)
        {
            return Get(keys);
        }

        /// <summary>
        /// Delete o objeto indicado do repositório de dados
        /// </summary>
        /// <param name="entity">Objeto a ser deletado</param>
        public void Delete(T entity)
        {
            DbSet.Remove(entity);
        }

        /// <summary>
        /// Deletra uma lista de objetos confrome o filtro
        /// </summary>
        /// <param name="predicate">Filtro de objetos a serem deletados</param>
        public void Delete(Expression<Func<T, bool>> predicate)
        {
            DbSet.RemoveRange(DbSet.Where(predicate));
        }

        /// <summary>
        /// Salva o objeto no repositório
        /// </summary>
        /// <param name="entity">Objeto a ser salvo</param>
        /// <returns>Retorna o mesmo objeto, para o caso de retornar algum Id gerado</returns>
        public T Save(T entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException("entity");
            }
            return DbSet.Add(entity);
        }

        /// <summary>
        /// Salva ou atualiza o objeto em questão (USAR SOMENTE SE O ID NÃO FOI SETADO)
        /// </summary>
        /// <param name="entity">Objeto a ser salvo/atualizado</param>
        /// <returns>Retorna o mesmo objeto, para o caso de retornar algum Id gerado</returns>
        public T SaveOrUpdate(T entity)
        {
            if (entity.IsTransient())
                DbSet.Add(entity);
            else
                Context.Entry(entity).State = EntityState.Modified;

            return entity;
        }

        /// <summary>
        /// Atualiza o objeto no repositório
        /// </summary>
        /// <param name="entity">Objeto a ser atulizado</param>
        public void Update(T entity)
        {
            Context.Entry(entity).State = EntityState.Modified;
        }

        /// <summary>
        /// Efetua o Merge do objeto no repositório
        /// </summary>
        /// <param name="entity">Objeto a ser mesclado</param>
        /// <returns>Retorna o mesmo objeto, para o caso de retornar algum Id gerado</returns>
        public T Merge(T entity)
        {
            Context.Entry(entity).State = EntityState.Modified;
            return entity;
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

        public Task<IQueryable<T>> AllAsync()
        {
            throw new NotImplementedException();
        }

        public Task<IQueryable<T>> FindAsync(Expression<Func<T, bool>> predicate)
        {
            throw new NotImplementedException();
        }

        public Task<IQueryable<T>> FindAsync(Expression<Func<T, bool>> filter, out int total, int index = 0, int size = 50)
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

        public Task<T> LoadAsync(object key)
        {
            throw new NotImplementedException();
        }

        public Task<bool> DeleteAsync(T entity)
        {
            throw new NotImplementedException();
        }

        public Task<bool> DeleteAsync(Expression<Func<T, bool>> predicate)
        {
            throw new NotImplementedException();
        }

        public Task<T> SaveAsync(T entity)
        {
            throw new NotImplementedException();
        }

        public Task<T> SaveOrUpdateAsync(T entity)
        {
            throw new NotImplementedException();
        }

        public Task<bool> UpdateAsync(T entity)
        {
            throw new NotImplementedException();
        }

        public Task<T> MergeAsync(T entity)
        {
            throw new NotImplementedException();
        }
    }
}
