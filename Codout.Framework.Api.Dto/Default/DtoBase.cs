namespace Codout.Framework.Api.Dto.Default
{

    /// <summary>
    /// Classe DTO base para transporte com WebAPI
    /// </summary>
    /// <typeparam name="TId">Tipo do Id</typeparam>
    public abstract class DtoBase<TId> : IDto<TId>
    {
        /// <summary>
        /// Id do objeto
        /// </summary>
        public TId Id { get; set; }
    }
}
