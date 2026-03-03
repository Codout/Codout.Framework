using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Codout.Framework.Data.Entity;

namespace Codout.Framework.Data.Specifications;

/// <summary>
/// Specification Pattern interface for encapsulating query logic
/// </summary>
/// <typeparam name="T">The entity type</typeparam>
public interface ISpecification<T> where T : class, IEntity
{
    /// <summary>
    /// Gets the filter expression
    /// </summary>
    Expression<Func<T, bool>>? Criteria { get; }
    
    /// <summary>
    /// Gets the ordering function
    /// </summary>
    Func<IQueryable<T>, IOrderedQueryable<T>>? OrderBy { get; }
    
    /// <summary>
    /// Gets the list of include expressions for eager loading
    /// </summary>
    List<Expression<Func<T, object>>> Includes { get; }
    
    /// <summary>
    /// Gets the list of string-based include paths for eager loading
    /// </summary>
    List<string> IncludeStrings { get; }
    
    /// <summary>
    /// Gets the number of entities to take
    /// </summary>
    int Take { get; }
    
    /// <summary>
    /// Gets the number of entities to skip
    /// </summary>
    int Skip { get; }
    
    /// <summary>
    /// Gets a value indicating whether paging is enabled
    /// </summary>
    bool IsPagingEnabled { get; }
    
    /// <summary>
    /// Gets a value indicating whether the query should not track changes
    /// </summary>
    bool AsNoTracking { get; }
}
