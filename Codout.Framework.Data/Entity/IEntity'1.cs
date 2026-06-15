namespace Codout.Framework.Data.Entity;

/// <summary>
///     Entidade com identificador tipado: estende <see cref="IEntity" /> expondo o
///     Id de forma covariante (<c>out TId</c>).
/// </summary>
/// <typeparam name="TId">Tipo do identificador da entidade.</typeparam>
public interface IEntity<out TId> : IEntity
{
    /// <summary>
    ///     Gets the ID which uniquely identifies the entity instance within its type's bounds.
    /// </summary>
    TId Id { get; }
}
