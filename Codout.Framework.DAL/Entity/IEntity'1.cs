namespace Codout.Framework.DAL.Entity
{
    /// <summary>
    ///     This serves as a base interface for <see cref="IEntity{TId}" /> and
    /// </summary>
    public interface IEntity<out TId> : IEntity
    {
        /// <summary>
        ///     Gets the ID which uniquely identifies the entity instance within its type's bounds.
        /// </summary>
        TId Id { get; }
    }
}
