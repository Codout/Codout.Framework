using System.Threading.Tasks;
using Codout.Framework.Api.Dto;
using Microsoft.AspNetCore.Mvc;

namespace Codout.Framework.Api
{
    /// <summary>
    /// WebAPI Genérica Padrão de comandos CRUD
    /// </summary>
    /// <typeparam name="TDto">Tipo do objeto DTO</typeparam>
    /// <typeparam name="TId">Tipo do Id do objeto</typeparam>
    public interface IApi<TDto, in TId> where TDto : IDto<TId>
    {
        /// <summary>
        /// Retorma um IEnumerable do objeto tipado
        /// </summary>
        /// <returns>Lista com todos os objetos</returns>
        Task<IActionResult> Get();

        /// <summary>
        /// Retorna uma lista de resultados com paginação
        /// </summary>
        /// <param name="page">Página</param>
        /// <param name="size">Tamanho</param>
        /// <returns>Lista paginada dos objetos</returns>
        Task<IActionResult> Get(int page, int size);

        /// <summary>
        /// Retorna um objeto de acordo com o id
        /// </summary>
        /// <param name="id">Id do objeto</param>
        /// <returns>Objeto</returns>
        Task<IActionResult> Get(TId id);

        /// <summary>
        /// Recebe e retorna o objeto tipado
        /// </summary>
        /// <param name="obj">Objeto a ser salvo</param>
        /// <returns>Objeto</returns>
        Task<IActionResult> Post(TDto obj);

        /// <summary>
        /// Recebe e retorna o objeto tipado
        /// </summary>
        /// <param name="id">Id do Objeto</param>
        /// <param name="obj">Objeto a ser atualizado</param>
        /// <returns>Objeto</returns>
        Task<IActionResult> Put(TId id, TDto obj);

        /// <summary>
        /// Deleta o objeto identitificado pelo id
        /// </summary>
        /// <param name="id">Id do objeto a ser excluído</param>
        Task<IActionResult> Delete(TId id);
    }
}
