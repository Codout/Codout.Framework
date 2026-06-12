using System;

namespace Codout.Framework.Domain.Interfaces;

/// <summary>
///     Contrato para entidades com exclusão lógica (soft delete): em vez de remover
///     o registro fisicamente, a infraestrutura de persistência marca a data de
///     exclusão e passa a filtrar a entidade das consultas.
/// </summary>
public interface ISoftDeletable
{
    /// <summary>
    ///     Data e hora da exclusão lógica; <c>null</c> enquanto a entidade está ativa
    ///     (não excluída).
    /// </summary>
    DateTime? DeletedAt { get; set; }
}
