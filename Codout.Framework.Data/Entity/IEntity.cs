using System.Collections.Generic;
using System.Reflection;

namespace Codout.Framework.Data.Entity;

/// <summary>
///     Interface base (não genérica) de toda entidade do framework: expõe os membros
///     de identidade que não dependem do tipo do Id, permitindo que repositórios e
///     infraestrutura tratem entidades de forma polimórfica. Para acesso tipado ao
///     Id, use <see cref="IEntity{TId}" />.
/// </summary>
public interface IEntity
{
    /// <summary>
    ///     Returns the properties of the current object that make up the object's signature.
    /// </summary>
    /// <returns>A collection of <see cref="PropertyInfo" /> instances.</returns>
    IEnumerable<PropertyInfo> GetSignatureProperties();

    /// <summary>
    ///     Returns a value indicating whether the current object is transient.
    /// </summary>
    /// <remarks>
    ///     Transient objects are not associated with an item already in storage. For instance,
    ///     a Customer is transient if its ID is 0.  It's virtual to allow NHibernate-backed
    ///     objects to be lazily loaded.
    /// </remarks>
    bool IsTransient();
}
