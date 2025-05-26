namespace Codout.Framework.Api.Client;

/// <summary>
///     Classe DTO base para transporte com WebAPI
/// </summary>
/// <typeparam name="TId">Tipo do Id</typeparam>
public abstract class EntityDtoBase<TId> : IEntityDto<TId>
{
    /// <summary>
    ///     Id do objeto
    /// </summary>
    public TId Id { get; set; }
}