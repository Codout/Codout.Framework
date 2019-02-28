using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Codout.Framework.Api.Dto;

namespace Codout.Framework.Api.Client
{
    /// <summary>
    /// WebAPI Genérica Padrão de comandos CRUD
    /// </summary>
    /// <typeparam name="T">Tipo do objeto a ser considerado</typeparam>
    /// <typeparam name="TId">Tipo do Id do objeto</typeparam>
    public interface IApiClient<T, in TId> where T : IDto<TId>
    {

        /// <summary>
        /// Retorma um IEnumerable do objeto tipado
        /// </summary>
        /// <returns>IEnumerable</returns>
        Task<IEnumerable<T>> Get();

        /// <summary>
        /// Retorna uma lista de resultados com paginação
        /// </summary>
        /// <param name="page">Página</param>
        /// <param name="size">Tamanho</param>
        /// <returns></returns>
        Task<IPagedResult<T>> Get(int page, int size);

        /// <summary>
        /// Retorna um objeto de acordo com o id
        /// </summary>
        /// <param name="id">Id do objeto</param>
        /// <returns>Objeto</returns>
        Task<T> Get(TId id);

        /// <summary>
        /// Recebe e retorna o objeto tipado
        /// </summary>
        /// <param name="obj">Objeto a ser tratado</param>
        /// <returns>Objeto</returns>
        Task<T> Post(T obj);

        /// <summary>
        /// Recebe e retorna o objeto tipado
        /// </summary>
        /// <param name="obj">Objeto a ser tratado</param>
        /// <returns>Objeto</returns>
        Task<T> Put(T obj);

        /// <summary>
        /// Deleta o objeto identitificado pelo id
        /// </summary>
        /// <param name="id">Id do objeto</param>
        /// <returns>StatusCode da operação</returns>
        Task Delete(TId id);
    }
}
