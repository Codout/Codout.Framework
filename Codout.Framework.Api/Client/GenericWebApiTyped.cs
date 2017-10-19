using System;
using Codout.Framework.Api.Dto;

namespace Codout.Framework.Api.Client
{
    /// <inheritdoc />
    /// <summary>
    /// Classe genérica para operações CRUD em WebAPI tipada com Guid?
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TId"></typeparam>
    public class GenericWebApiTyped<T, TId> : GenericWebApi<T, TId> where T : DtoBase<TId>
    {
        public GenericWebApiTyped(string uriService, string baseUrl)
            : base(uriService, baseUrl)
        {
        }

        public GenericWebApiTyped(string uriService, string baseUrl, string apiKey)
            : base(uriService, baseUrl, apiKey)
        {
        }
    }
}
