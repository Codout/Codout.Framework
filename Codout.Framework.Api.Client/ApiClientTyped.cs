using Codout.Framework.Api.Dto;

namespace Codout.Framework.Api.Client
{
    /// <inheritdoc />
    /// <summary>
    /// Classe genérica para operações CRUD em WebAPI tipada com Guid?
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TId"></typeparam>
    public class ApiClientTyped<T, TId> : ApiClient<T, TId> where T : IDto<TId>
    {
        public ApiClientTyped(string uriService, string baseUrl)
            : base(uriService, baseUrl)
        {
        }

        public ApiClientTyped(string uriService, string baseUrl, string apiKey)
            : base(uriService, baseUrl, apiKey)
        {
        }
    }
}
