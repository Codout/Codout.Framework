using System;

namespace Codout.Framework.Data.Auditing;

/// <summary>
/// Defines the contract for entities that support soft delete (logical delete)
/// </summary>
public interface ISoftDeletable
{
    /// <summary>
    /// Gets or sets a value indicating whether the entity has been deleted
    /// </summary>
    bool IsDeleted { get; set; }
    
    /// <summary>
    /// Gets or sets the date and time when the entity was deleted
    /// </summary>
    DateTime? DeletedAt { get; set; }
    
    /// <summary>
    /// Gets or sets the identifier of the user who deleted the entity
    /// </summary>
    string? DeletedBy { get; set; }
}
