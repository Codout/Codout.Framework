using System.Threading.Tasks;
using Codout.DynamicLinq;
using Codout.Framework.Api.Client.Extensions;
using Codout.Framework.Dto;

namespace Codout.Framework.Api.Client
{
    /// <summary>
    /// Classe genérica para operações CRUD em WebAPI
    /// </summary>
    /// <typeparam name="T">Tipo do objeto a ser tratado</typeparam>
    /// <typeparam name="TId">Tipo do Id do objeto</typeparam>
    public class RestApiClient<T, TId> : ApiClientBase, IRestApi<T, TId> where T : IEntityDto<TId>
    {
        public RestApiClient(string uriService, string baseUrl)
        : base(uriService, baseUrl)
        {
        }

        public RestApiClient(string uriService, string baseUrl, string apiKey)
            : base(uriService, baseUrl, apiKey)
        {
        }

        /// <inheritdoc />
        /// <summary>
        /// Retorna um objeto de acordo com o id
        /// </summary>
        /// <param name="id">Id do objeto</param>
        /// <returns>Objeto</returns>
        public async Task<T> GetAsync(TId id)
        {
            return await Client.GetAsync<T>($"{UriService}/{id}");
        }

        /// <inheritdoc />
        /// <summary>
        /// Recebe e retorna o objeto tipado
        /// </summary>
        /// <param name="obj">Objeto a ser tratado</param>
        /// <returns>Objeto</returns>
        public async Task<T> PostAsync(T obj)
        {
            return await Client.PostAsync<T, T>($"{UriService}", obj);
        }

        /// <summary>
        /// Recebe e retorna o objeto tipado
        /// </summary>
        /// <param name="obj">Objeto a ser tratado</param>
        /// <returns>Objeto</returns>
        public async Task<T> PutAsync(T obj)
        {
            return await Client.PutAsync<T, T>($"{UriService}/{obj.Id}", obj);
        }

        /// <summary>
        /// Deleta o objeto identitificado pelo id
        /// </summary>
        /// <param name="id">Id do objeto</param>
        /// <returns>StatusCode da operação</returns>
        public async Task DeleteAsync(TId id)
        {
            await Client.DeleteAsync($"{UriService}/{id}");
        }

        public async Task<DataSourceResult> GetAllAsync(DataSourceRequest obj)
        {
            return await Client.PostAsync<DataSourceResult, DataSourceRequest>($"{UriService}/get-all", obj);
        }
    }
}
