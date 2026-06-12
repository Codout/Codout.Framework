using Codout.Framework.Data.Entity;

namespace Codout.Framework.Domain.Entities.Events;

/// <summary>
///     Evento de domínio que sinaliza que uma entidade existente foi alterada
///     (atualizada). Publique/assine este evento para reagir a atualizações sem
///     acoplar o código de negócio à infraestrutura de persistência.
/// </summary>
/// <typeparam name="T">Tipo da entidade alterada.</typeparam>
/// <param name="entity">Entidade que foi alterada.</param>
public class EntityChanged<T>(T entity)
    where T : IEntity
{
    /// <summary>
    ///     Entidade alterada, associada ao evento.
    /// </summary>
    public T Entity { get; set; } = entity;
}
