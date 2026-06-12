using System;
using Codout.Framework.Domain.Interfaces;

namespace Codout.Framework.Domain.Entities;

/// <summary>
///     Entidade base padrão do framework (Id <see cref="Guid" /> anulável, via
///     <see cref="EntityBase" />) com campos de auditoria (<see cref="IAudit" />).
///     Herde desta classe quando a entidade precisa registrar criação/alteração.
/// </summary>
[Serializable]
public abstract class AuditEntityBase : EntityBase, IAudit
{
    /// <inheritdoc />
    public virtual DateTime? CreatedAt { get; set; }

    /// <inheritdoc />
    public virtual DateTime? UpdatedAt { get; set; }

    /// <inheritdoc />
    public virtual string? CreatedBy { get; set; }

    /// <inheritdoc />
    public virtual string? UpdatedBy { get; set; }
}
