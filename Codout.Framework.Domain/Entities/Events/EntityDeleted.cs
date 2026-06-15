using Codout.Framework.Data.Entity;

namespace Codout.Framework.Domain.Entities.Events;

/// <summary>
///     Evento de domínio que sinaliza que uma entidade foi excluída.
///     Publique/assine este evento para reagir a exclusões sem acoplar o código
///     de negócio à infraestrutura de persistência.
/// </summary>
/// <typeparam name="T">Tipo da entidade excluída.</typeparam>
/// <param name="entity">Entidade que foi excluída.</param>
public class EntityDeleted<T>(T entity)
    where T : IEntity
{
    /// <summary>
    ///     Entidade excluída, associada ao evento.
    /// </summary>
    public T Entity { get; set; } = entity;
}
