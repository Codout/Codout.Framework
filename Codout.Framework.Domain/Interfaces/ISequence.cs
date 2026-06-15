namespace Codout.Framework.Domain.Interfaces;

/// <summary>
///     Contrato para entidades que possuem um código sequencial numérico legível
///     (ex.: número de pedido ou de nota), geralmente gerado pela infraestrutura
///     de persistência (sequence/identity), independente da chave primária.
/// </summary>
public interface ISequence
{
    /// <summary>
    ///     Código sequencial da entidade.
    /// </summary>
    long Code { get; set; }
}
