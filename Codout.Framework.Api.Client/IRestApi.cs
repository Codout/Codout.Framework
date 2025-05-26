using System.Threading.Tasks;
using Codout.DynamicLinq;

namespace Codout.Framework.Api.Client;

/// <summary>
///     WebAPI Genérica Padrão de comandos CRUD
/// </summary>
/// <typeparam name="TDto">Tipo do objeto DTO</typeparam>
/// <typeparam name="TId">Tipo do Id do objeto</typeparam>
public interface IRestApi<TDto, in TId>
{
    /// <summary>
    ///     Retorna um objeto de acordo com o id
    /// </summary>
    /// <param name="id">Id do objeto</param>
    /// <returns>Objeto</returns>
    Task<TDto> GetAsync(TId id);

    /// <summary>
    ///     Recebe e retorna o objeto tipado
    /// </summary>
    /// <param name="obj">Objeto a ser salvo</param>
    /// <returns>Objeto</returns>
    Task<TDto> PostAsync(TDto obj);

    /// <summary>
    ///     Recebe e retorna o objeto tipado
    /// </summary>
    /// <param name="obj">Objeto a ser atualizado</param>
    /// <returns>Objeto</returns>
    Task<TDto> PutAsync(TDto obj);

    /// <summary>
    ///     Deleta o objeto identitificado pelo id
    /// </summary>
    /// <param name="id">Id do objeto a ser excluído</param>
    Task DeleteAsync(TId id);

    /// <summary>
    ///     Retorna a lista de itens paginada
    /// </summary>
    /// <param name="dataSourceRequest">Parâmetros de filtros da lista</param>
    /// <returns>lista paginada dos objetos</returns>
    Task<DataSourceResult> GetAllAsync(DataSourceRequest dataSourceRequest);
}