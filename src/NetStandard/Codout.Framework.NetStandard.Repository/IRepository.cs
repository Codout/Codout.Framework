using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Codout.Framework.NetStandard.Repository
{
    /// <summary>
    /// Interface de definição para um repositório genérico de dados
    /// </summary>
    /// <typeparam name="T">Classe que define o tipo do repositório</typeparam>
    public interface IRepository<T> : IDisposable where T : class
    {
        /// <summary>
        /// Retorna todos os objetos do repositório (pode ser lento)
        /// </summary>
        /// <returns>Lista de objetos</returns>
        IQueryable<T> All();

        /// <summary>
        /// Retorna todos os objetos do repositório (pode ser lento)
        /// </summary>
        /// <returns>Lista de objetos</returns>
        Task<IQueryable<T>> AllAsync();

        /// <summary>
        /// Retorna uma lista de objetos do repositório de acordo com o filtro apresentado
        /// </summary>
        /// <param name="predicate">Lista de objetos</param>
        /// <returns></returns>
        IQueryable<T> Find(Expression<Func<T, bool>> predicate);

        /// <summary>
        /// Retorna uma lista de objetos do repositório de acordo com o filtro apresentado
        /// </summary>
        /// <param name="predicate">Lista de objetos</param>
        /// <returns></returns>
        Task<IQueryable<T>> FindAsync(Expression<Func<T, bool>> predicate);

        /// <summary>
        /// Retorna uma lista de objetos do repositório de acordo com o filtro e com opção de paginação
        /// </summary>
        /// <param name="filter">Filtro de bojetos</param>
        /// <param name="total">Retorna o todal de objetos</param>
        /// <param name="index">Indica o índice da paginação</param>
        /// <param name="size">Tamanho da página</param>
        /// <returns>Lista de objetos</returns>
        IQueryable<T> Find(Expression<Func<T, bool>> filter, out int total, int index = 0, int size = 50);

        /// <summary>
        /// Retorna uma lista de objetos do repositório de acordo com o filtro e com opção de paginação
        /// </summary>
        /// <param name="filter">Filtro de bojetos</param>
        /// <param name="total">Retorna o todal de objetos</param>
        /// <param name="index">Indica o índice da paginação</param>
        /// <param name="size">Tamanho da página</param>
        /// <returns>Lista de objetos</returns>
        Task<IQueryable<T>> FindAsync(Expression<Func<T, bool>> filter, out int total, int index = 0, int size = 50);

        /// <summary>
        /// Retorna um objeto do repositório de acordo com o filtro
        /// </summary>
        /// <param name="predicate">Filtro</param>
        /// <returns>objeto</returns>
        T Get(Expression<Func<T, bool>> predicate);

        /// <summary>
        /// Retorna um objeto do repositório de acordo com o filtro
        /// </summary>
        /// <param name="predicate">Filtro</param>
        /// <returns>objeto</returns>
        Task<T> GetAsync(Expression<Func<T, bool>> predicate);

        /// <summary>
        /// Retorna um objeto de acordo com a Key
        /// </summary>
        /// <param name="key">Key do objeto</param>
        /// <returns>objeto</returns>
        T Get(object key);

        /// <summary>
        /// Retorna um objeto de acordo com a Key
        /// </summary>
        /// <param name="key">Key do objeto</param>
        /// <returns>objeto</returns>
        Task<T> GetAsync(object key);

        /// <summary>
        /// Efetua a carga do objeto conforme a key
        /// </summary>
        /// <param name="key">Key do objeto</param>
        /// <returns>Objeto</returns>
        T Load(object key);

        /// <summary>
        /// Efetua a carga do objeto conforme a key
        /// </summary>
        /// <param name="key">Key do objeto</param>
        /// <returns>Objeto</returns>
        Task<T> LoadAsync(object key);

        /// <summary>
        /// Delete o objeto indicado do repositório de dados
        /// </summary>
        /// <param name="entity">Objeto a ser deletado</param>
        void Delete(T entity);

        /// <summary>
        /// Delete o objeto indicado do repositório de dados
        /// </summary>
        /// <param name="entity">Objeto a ser deletado</param>
        Task<bool> DeleteAsync(T entity);

        /// <summary>
        /// Deleta uma lista de objetos confrome o filtro
        /// </summary>
        /// <param name="predicate">Filtro de objetos a serem deletados</param>
        void Delete(Expression<Func<T, bool>> predicate);

        /// <summary>
        /// Deletra uma lista de objetos confrome o filtro
        /// </summary>
        /// <param name="predicate">Filtro de objetos a serem deletados</param>
        Task<bool> DeleteAsync(Expression<Func<T, bool>> predicate);

        /// <summary>
        /// Salva o objeto no repositório
        /// </summary>
        /// <param name="entity">Objeto a ser salvo</param>
        /// <returns>Retorna o mesmo objeto, para o caso de retornar algum Id gerado</returns>
        T Save(T entity);

        /// <summary>
        /// Salva o objeto no repositório
        /// </summary>
        /// <param name="entity">Objeto a ser salvo</param>
        /// <returns>Retorna o mesmo objeto, para o caso de retornar algum Id gerado</returns>
        Task<T> SaveAsync(T entity);

        /// <summary>
        /// Salva ou atualiza o objeto em questão (USAR SOMENTE SE O ID NÃO FOI SETADO)
        /// </summary>
        /// <param name="entity">Objeto a ser salvo/atualizado</param>
        /// <returns>Retorna o mesmo objeto, para o caso de retornar algum Id gerado</returns>
        T SaveOrUpdate(T entity);

        /// <summary>
        /// Salva ou atualiza o objeto em questão (USAR SOMENTE SE O ID NÃO FOI SETADO)
        /// </summary>
        /// <param name="entity">Objeto a ser salvo/atualizado</param>
        /// <returns>Retorna o mesmo objeto, para o caso de retornar algum Id gerado</returns>
        Task<T> SaveOrUpdateAsync(T entity);

        /// <summary>
        /// Atualiza o objeto no repositório
        /// </summary>
        /// <param name="entity">Objeto a ser atulizado</param>
        void Update(T entity);

        /// <summary>
        /// Atualiza o objeto no repositório
        /// </summary>
        /// <param name="entity">Objeto a ser atulizado</param>
        Task<bool> UpdateAsync(T entity);

        /// <summary>
        /// Efetua o Merge do objeto no repositório
        /// </summary>
        /// <param name="entity">Objeto a ser mesclado</param>
        /// <returns>Retorna o mesmo objeto, para o caso de retornar algum Id gerado</returns>
        T Merge(T entity);

        /// <summary>
        /// Efetua o Merge do objeto no repositório
        /// </summary>
        /// <param name="entity">Objeto a ser mesclado</param>
        /// <returns>Retorna o mesmo objeto, para o caso de retornar algum Id gerado</returns>
        Task<T> MergeAsync(T entity);
    }
}
