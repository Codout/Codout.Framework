using System.Threading.Tasks;
using Codout.DynamicLinq;
using Codout.Framework.Api.Client;
using Codout.Framework.Domain.Entities;

namespace Codout.Framework.Application.Interfaces;

public interface ICrudAppService<TEntity, TDto, in TId> : IAppService<TEntity>
    where TEntity : Entity<TId>
    where TDto : EntityDto<TId>
{
    Task<DataSourceResult> GetAllAsync(DataSourceRequest dataSourceRequest);

    Task<TDto> GetAsync(TId id);

    Task<TDto> SaveAsync(TDto input);

    Task<TDto> UpdateAsync(TDto input);

    Task DeleteAsync(TId id);
}