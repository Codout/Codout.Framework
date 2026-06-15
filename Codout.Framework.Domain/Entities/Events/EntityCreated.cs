using Codout.Framework.Data.Entity;

namespace Codout.Framework.Domain.Entities.Events;

/// <summary>
///     Evento de domínio que sinaliza que uma nova entidade foi criada.
///     Publique/assine este evento para reagir a criações sem acoplar o código
///     de negócio à infraestrutura de persistência.
/// </summary>
/// <typeparam name="T">Tipo da entidade criada.</typeparam>
/// <param name="entity">Entidade que foi criada.</param>
public class EntityCreated<T>(T entity)
    where T : IEntity
{
    /// <summary>
    ///     Entidade criada, associada ao evento.
    /// </summary>
    public T Entity { get; set; } = entity;
}
