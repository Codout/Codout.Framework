using Codout.Framework.Data.Entity;

namespace Codout.Framework.Domain.Entities.Events;

public class EntityDeleted<T>(T entity)
    where T : IEntity
{
    public T Entity { get; set; } = entity;
}