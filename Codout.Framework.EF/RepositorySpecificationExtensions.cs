using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Codout.Framework.Data.Entity;
using Codout.Framework.Data.Specifications;
using Microsoft.EntityFrameworkCore;

namespace Codout.Framework.EF;

/// <summary>
/// Extensões do Repository para suportar Specification Pattern
/// </summary>
public static class RepositorySpecificationExtensions
{
    public static IQueryable<T> GetBySpecification<T>(
        this EFRepository<T> repository,
        ISpecification<T> specification) where T : class, IEntity
    {
        return Specifications.SpecificationEvaluator.GetQuery(repository.DbSet, specification);
    }

    public static async Task<List<T>> ListAsync<T>(
        this EFRepository<T> repository,
        ISpecification<T> specification,
        CancellationToken cancellationToken = default) where T : class, IEntity
    {
        var query = Specifications.SpecificationEvaluator.GetQuery(repository.DbSet, specification);
        return await query.ToListAsync(cancellationToken);
    }

    public static async Task<T?> FirstOrDefaultAsync<T>(
        this EFRepository<T> repository,
        ISpecification<T> specification,
        CancellationToken cancellationToken = default) where T : class, IEntity
    {
        var query = Specifications.SpecificationEvaluator.GetQuery(repository.DbSet, specification);
        return await query.FirstOrDefaultAsync(cancellationToken);
    }

    public static async Task<int> CountAsync<T>(
        this EFRepository<T> repository,
        ISpecification<T> specification,
        CancellationToken cancellationToken = default) where T : class, IEntity
    {
        var query = Specifications.SpecificationEvaluator.GetQuery(repository.DbSet, specification);
        return await query.CountAsync(cancellationToken);
    }

    public static async Task<bool> AnyAsync<T>(
        this EFRepository<T> repository,
        ISpecification<T> specification,
        CancellationToken cancellationToken = default) where T : class, IEntity
    {
        var query = Specifications.SpecificationEvaluator.GetQuery(repository.DbSet, specification);
        return await query.AnyAsync(cancellationToken);
    }
}
