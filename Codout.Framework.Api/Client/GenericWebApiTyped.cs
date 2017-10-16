using System;
using Codout.Framework.Api.Dto;

namespace Codout.Framework.Api.Client
{
    /// <summary>
    /// Classe genérica para operações CRUD em WebAPI tipada com Guid?
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class GenericWebApiTyped<T> : GenericWebApi<T, Guid?> where T : DtoBase<Guid?>
    {
        public GenericWebApiTyped(string uriService, string baseUrl)
            : base(uriService, baseUrl)
        {
        }
    }
}
