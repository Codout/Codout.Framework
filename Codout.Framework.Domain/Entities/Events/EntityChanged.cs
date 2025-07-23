using Codout.Framework.DAL.Entity;

namespace Codout.Framework.Domain.Entities.Events;

public class EntityChanged<T>(T entity)
    where T : IEntity
{
    public T Entity { get; set; } = entity;
}