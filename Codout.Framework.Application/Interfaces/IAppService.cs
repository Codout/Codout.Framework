using AutoMapper;
using Codout.Framework.DAL;
using Codout.Framework.DAL.Entity;
using Codout.Framework.DAL.Repository;

namespace Codout.Framework.Application.Interfaces;

public interface IAppService<TEntity> where TEntity : class, IEntity
{
    IUnitOfWork UnitOfWork { get; }

    IRepository<TEntity> Repository { get; }

    IMapper Mapper { get; }
}