﻿using System.Collections.Generic;

namespace Codout.Framework.Domain.Base;

/// <summary>
///     Provides a comparer for supporting LINQ methods such as Intersect, Union and Distinct.
/// </summary>
/// <remarks>
///     <para>
///         This may be used for comparing objects of type <see cref="BaseObject" /> and anything
///         that derives from it, such as <see cref="BaseObject" /> and <see cref="ValueObject" />.
///     </para>
///     <para>
///         NOTE: Microsoft decided that set operators such as Intersect, Union and Distinct should
///         not use the IEqualityComparer's Equals() method when comparing objects, but should instead
///         use IEqualityComparer's GetHashCode() method.
///     </para>
/// </remarks>
internal class BaseObjectEqualityComparer<T> : IEqualityComparer<T>
    where T : BaseObject
{
    /// <summary>
    ///     Compares the specified objects for equality.
    /// </summary>
    /// <param name="firstObject">The first object.</param>
    /// <param name="secondObject">The second object.</param>
    /// <returns><c>true</c> if the objects are equal, <c>false</c> otherwise.</returns>
    public bool Equals(T firstObject, T secondObject)
    {
        // While SQL would return false for the following condition, returning true when 
        // comparing two null values is consistent with the C# language
        if (firstObject == null && secondObject == null) return true;

        if (firstObject == null ^ secondObject == null) return false;

        return firstObject.Equals(secondObject);
    }

    /// <summary>
    ///     Returns a hash code for the specified object.
    /// </summary>
    /// <param name="obj">The object.</param>
    /// <returns>A hash code for the object, suitable for use in hashing algorithms and data structures like a hash table.</returns>
    public int GetHashCode(T obj)
    {
        return obj.GetHashCode();
    }
}