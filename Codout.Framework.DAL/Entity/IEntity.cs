using System.Collections.Generic;
using System.Reflection;

namespace Codout.Framework.DAL.Entity;

/// <summary>
///     This serves as a base interface for <see cref="IEntity{TId}" /> and
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