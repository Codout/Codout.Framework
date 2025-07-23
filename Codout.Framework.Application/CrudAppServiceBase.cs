using System;
using System.Threading.Tasks;
using AutoMapper;
using Codout.DynamicLinq;
using Codout.Framework.Api.Client;
using Codout.Framework.Application.Interfaces;
using Codout.Framework.DAL;
using Codout.Framework.DAL.Repository;
using Codout.Framework.Domain.Entities;

namespace Codout.Framework.Application;

public class CrudAppServiceBase<TEntity, TDto, TId>(
    IUnitOfWork unitOfWork,
    IRepository<TEntity> repository,
    IMapper mapper)
    : AppServiceBase<TEntity>(unitOfWork, repository, mapper), ICrudAppService<TEntity, TDto, TId>
    where TEntity : Entity<TId>
    where TDto : EntityDto<TId>
{
    public virtual async Task<DataSourceResult> GetAllAsync(DataSourceRequest dataSourceRequest)
    {
        return await Task.Run(() => Repository.All().ToDataSourceResult(dataSourceRequest));
    }

    public virtual async Task<TDto> GetAsync(TId id)
    {
        var entity = await Repository.GetAsync(id);

        if (entity == null)
            return null;

        var output = Mapper.Map<TDto>(entity);

        return output;
    }

    public virtual async Task<TDto> SaveAsync(TDto input)
    {
        if (input == null)
            throw new NullReferenceException($"O objeto {nameof(TDto)} não pode ser nulo");

        var entity = Mapper.Map<TEntity>(input);

        await Repository.SaveAsync(entity);

        UnitOfWork.Commit();

        var output = Mapper.Map<TDto>(entity);

        return output;
    }

    public virtual async Task<TDto> UpdateAsync(TDto input)
    {
        var entity = await Repository.GetAsync(input.Id);

        if (entity == null)
            return null;

        entity = Mapper.Map(input, entity, typeof(TDto), typeof(TEntity)) as TEntity;

        UnitOfWork.Commit();

        return input;
    }

    public virtual async Task DeleteAsync(TId id)
    {
        var entity = await Repository.LoadAsync(id);

        if (entity == null)
            return;

        await Repository.DeleteAsync(entity);

        UnitOfWork.Commit();
    }
}