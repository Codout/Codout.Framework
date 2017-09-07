namespace Codout.Framework.NetStandard.Api.Dto
{

    /// <summary>
    /// Classe DTO base para transporte com WebAPI
    /// </summary>
    /// <typeparam name="TId">Tipo do Id</typeparam>
    public abstract class DtoBase<TId>
    {

        /// <summary>
        /// Id do objeto
        /// </summary>
        public TId Id { get; set; }
    }
}
