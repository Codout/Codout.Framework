using System;

namespace Codout.Framework.Domain.Interfaces;

/// <summary>
///     Contrato de auditoria básica para entidades de domínio: registra quando e por
///     quem a entidade foi criada e alterada pela última vez. Os campos normalmente
///     são preenchidos pela infraestrutura de persistência (interceptors/handlers de
///     auditoria), não pelo código de negócio.
/// </summary>
public interface IAudit
{
    /// <summary>
    ///     Data e hora em que a entidade foi criada; <c>null</c> enquanto a entidade
    ///     ainda não foi persistida (ou quando a auditoria não está habilitada).
    /// </summary>
    DateTime? CreatedAt { get; set; }

    /// <summary>
    ///     Data e hora da última alteração da entidade; <c>null</c> quando ela nunca
    ///     foi alterada após a criação.
    /// </summary>
    DateTime? UpdatedAt { get; set; }

    /// <summary>
    ///     Identificador do usuário que criou a entidade; <c>null</c> quando
    ///     desconhecido (ex.: processo sem usuário autenticado).
    /// </summary>
    string? CreatedBy { get; set; }

    /// <summary>
    ///     Identificador do usuário responsável pela última alteração; <c>null</c>
    ///     quando desconhecido.
    /// </summary>
    string? UpdatedBy { get; set; }
}
