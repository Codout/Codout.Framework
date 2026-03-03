using System;

namespace Codout.Framework.Data.Auditing;

/// <summary>
/// Defines the contract for entities that support automatic auditing
/// </summary>
public interface IAuditable
{
    /// <summary>
    /// Gets or sets the date and time when the entity was created
    /// </summary>
    DateTime CreatedAt { get; set; }
    
    /// <summary>
    /// Gets or sets the identifier of the user who created the entity
    /// </summary>
    string? CreatedBy { get; set; }
    
    /// <summary>
    /// Gets or sets the date and time when the entity was last updated
    /// </summary>
    DateTime? UpdatedAt { get; set; }
    
    /// <summary>
    /// Gets or sets the identifier of the user who last updated the entity
    /// </summary>
    string? UpdatedBy { get; set; }
}
