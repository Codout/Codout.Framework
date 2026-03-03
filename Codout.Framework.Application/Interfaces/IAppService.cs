using AutoMapper;
using Codout.Framework.Data;
using Codout.Framework.Data.Entity;
using Codout.Framework.Data.Repository;

namespace Codout.Framework.Application.Interfaces;

public interface IAppService<TEntity> where TEntity : class, IEntity
{
    IUnitOfWork UnitOfWork { get; }

    IRepository<TEntity> Repository { get; }

    IMapper Mapper { get; }
}