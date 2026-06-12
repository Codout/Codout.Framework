using System;
using Codout.Framework.Domain.Interfaces;

namespace Codout.Framework.Domain.Entities;

/// <summary>
///     Entidade base com identificador tipado e campos de auditoria
///     (<see cref="IAudit" />). Herde desta classe quando a entidade precisa
///     registrar criação/alteração e o tipo do Id não é o padrão
///     <see cref="Guid" /> de <see cref="AuditEntityBase" />.
/// </summary>
/// <typeparam name="TId">Tipo do identificador da entidade (int, long, Guid, string, etc.).</typeparam>
[Serializable]
public abstract class AuditEntity<TId> : Entity<TId>, IAudit
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
