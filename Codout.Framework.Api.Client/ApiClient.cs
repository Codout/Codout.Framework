using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Codout.Framework.Api.Client.Helpers;
using Codout.Framework.Api.Dto;
using Codout.Framework.Api.Dto.Default;

namespace Codout.Framework.Api.Client
{
    /// <inheritdoc />
    /// <summary>
    /// Classe genérica para operações CRUD em WebAPI
    /// </summary>
    /// <typeparam name="T">Tipo do objeto a ser tratado</typeparam>
    /// <typeparam name="TId">Tipo do Id do objeto</typeparam>
    public class ApiClient<T, TId> : IApiClient<T, TId> where T : IDto<TId>
    {
        public string UriService { get; }

        public ApiClient(string uriService, string baseUrl)
        {
            UriService = uriService;
            Client = new HttpClient { BaseAddress = new Uri(baseUrl) };
            Client.DefaultRequestHeaders.Accept.Clear();
            Client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            Client.Timeout = new TimeSpan(0, 0, 1, 0);
        }

        public ApiClient(string uriService, string baseUrl, string apiKey)
        {
            UriService = uriService;
            Client = new HttpClient { BaseAddress = new Uri(baseUrl) };
            Client.DefaultRequestHeaders.Accept.Clear();
            Client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            Client.DefaultRequestHeaders.Add("ApiKey", apiKey);
            Client.Timeout = new TimeSpan(0, 0, 1, 0);
        }

        public HttpClient Client { get; }

        /// <inheritdoc />
        /// <summary>
        /// Retorma um IEnumerable do objeto tipado
        /// </summary>
        /// <returns>IEnumerable</returns>
        public async Task<IEnumerable<T>> Get()
        {
            IEnumerable<T> itens = null;

            var response = await Client.GetAsync(UriService);

            if (response.IsSuccessStatusCode)
            {
                itens = await response.Content.ReadAsAsync<IEnumerable<T>>();
            }

            return itens;
        }

        /// <summary>
        /// Busca os resultados com paginação
        /// </summary>
        /// <param name="page"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public async Task<IPagedResult<T>> Get(int page, int size)
        {
            PagedResultDto<T> itens = null;

            page = page < 0 ? 0 : page;

            size = size < 1 ? 1 : size;

            var response = await Client.GetAsync($"{UriService}?page={page}&size={size}");

            if (response.IsSuccessStatusCode)
            {
                itens = await response.Content.ReadAsAsync<PagedResultDto<T>>();
            }

            return itens;
        }

        /// <inheritdoc />
        /// <summary>
        /// Retorna um objeto de acordo com o id
        /// </summary>
        /// <param name="id">Id do objeto</param>
        /// <returns>Objeto</returns>
        public async Task<T> Get(TId id)
        {
            T obj = default(T);

            var response = await Client.GetAsync($"{UriService}/{id}");

            if (response.IsSuccessStatusCode)
            {
                obj = await response.Content.ReadAsAsync<T>();
            }

            return obj;
        }

        /// <inheritdoc />
        /// <summary>
        /// Recebe e retorna o objeto tipado
        /// </summary>
        /// <param name="obj">Objeto a ser tratado</param>
        /// <returns>Objeto</returns>
        public async Task<T> Post(T obj)
        {
            var response = await Client.PostAsJsonAsync($"{UriService}", obj);

            response.EnsureSuccessStatusCode();

            obj = await response.Content.ReadAsAsync<T>();

            return obj;
        }

        /// <summary>
        /// Recebe e retorna o objeto tipado
        /// </summary>
        /// <param name="obj">Objeto a ser tratado</param>
        /// <returns>Objeto</returns>
        public async Task<HttpStatusCode> Put(T obj)
        {
            var response = await Client.PutAsJsonAsync($"{UriService}/{obj.Id}", obj);
            response.EnsureSuccessStatusCode();
            return response.StatusCode;
        }

        /// <summary>
        /// Deleta o objeto identitificado pelo id
        /// </summary>
        /// <param name="id">Id do objeto</param>
        /// <returns>StatusCode da operação</returns>
        public async Task<HttpStatusCode> Delete(TId id)
        {
            var response = await Client.DeleteAsync($"{UriService}/{id}");
            return response.StatusCode;
        }
    }
}
