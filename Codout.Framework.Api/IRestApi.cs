using System.Threading.Tasks;
using Codout.DynamicLinq;
using Microsoft.AspNetCore.Mvc;

namespace Codout.Framework.Api;

/// <summary>
///     WebAPI Genérica Padrão de comandos CRUD
/// </summary>
public interface IRestApi<in TDto, in TId>
{
    /// <summary>
    ///     Retorna um objeto de acordo com o id
    /// </summary>
    /// <param name="id">Id do objeto</param>
    /// <returns>Objeto</returns>
    Task<IActionResult> Get(TId id);

    /// <summary>
    ///     Recebe e retorna o objeto tipado
    /// </summary>
    /// <param name="obj">Objeto a ser salvo</param>
    /// <returns>Objeto</returns>
    Task<IActionResult> Post(TDto obj);

    /// <summary>
    ///     Recebe e retorna o objeto tipado
    /// </summary>
    /// <param name="id">Id do Objeto</param>
    /// <param name="value">Objeto a ser atualizado</param>
    /// <returns>Objeto</returns>
    Task<IActionResult> Put(TId id, TDto value);

    /// <summary>
    ///     Deleta o objeto identitificado pelo id
    /// </summary>
    /// <param name="id">Id do objeto a ser excluído</param>
    Task<IActionResult> Delete(TId id);

    /// <summary>
    ///     Retorna a lista de itens paginada
    /// </summary>
    /// <param name="dataSourceRequest">Parâmetros de filtros da lista</param>
    /// <returns>lista paginada dos objetos</returns>
    Task<IActionResult> GetAll(DataSourceRequest dataSourceRequest);
}