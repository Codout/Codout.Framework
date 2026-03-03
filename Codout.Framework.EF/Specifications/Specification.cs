using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Codout.Framework.Data.Entity;
using Codout.Framework.Data.Specifications;
using Microsoft.EntityFrameworkCore;

namespace Codout.Framework.EF.Specifications;

/// <summary>
/// Implementação base do Specification Pattern para Entity Framework Core
/// </summary>
public abstract class Specification<T> : ISpecification<T> where T : class, IEntity
{
    protected Specification()
    {
        Includes = new List<Expression<Func<T, object>>>();
        IncludeStrings = new List<string>();
    }

    public Expression<Func<T, bool>>? Criteria { get; private set; }
    public Func<IQueryable<T>, IOrderedQueryable<T>>? OrderBy { get; private set; }
    public List<Expression<Func<T, object>>> Includes { get; }
    public List<string> IncludeStrings { get; }
    public int Take { get; private set; }
    public int Skip { get; private set; }
    public bool IsPagingEnabled { get; private set; }
    public bool AsNoTracking { get; private set; }

    protected void AddCriteria(Expression<Func<T, bool>> criteria)
    {
        Criteria = criteria;
    }

    protected void ApplyOrderBy(Func<IQueryable<T>, IOrderedQueryable<T>> orderByExpression)
    {
        OrderBy = orderByExpression;
    }

    protected void AddInclude(Expression<Func<T, object>> includeExpression)
    {
        Includes.Add(includeExpression);
    }

    protected void AddInclude(string includeString)
    {
        IncludeStrings.Add(includeString);
    }

    protected void ApplyPaging(int skip, int take)
    {
        Skip = skip;
        Take = take;
        IsPagingEnabled = true;
    }

    protected void ApplyNoTracking()
    {
        AsNoTracking = true;
    }
}

/// <summary>
/// Specification Evaluator para aplicar specifications em queries do EF Core
/// </summary>
public static class SpecificationEvaluator
{
    public static IQueryable<T> GetQuery<T>(IQueryable<T> inputQuery, ISpecification<T> specification)
        where T : class, IEntity
    {
        var query = inputQuery;

        if (specification.Criteria != null)
            query = query.Where(specification.Criteria);

        query = specification.Includes.Aggregate(query, (current, include) => current.Include(include));
        query = specification.IncludeStrings.Aggregate(query, (current, include) => current.Include(include));

        if (specification.OrderBy != null)
            query = specification.OrderBy(query);

        if (specification.AsNoTracking)
            query = query.AsNoTracking();

        if (specification.IsPagingEnabled)
            query = query.Skip(specification.Skip).Take(specification.Take);

        return query;
    }
}
