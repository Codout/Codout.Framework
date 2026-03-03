using AutoMapper;
using Codout.Framework.Application.Interfaces;
using Codout.Framework.Data;
using Codout.Framework.Data.Entity;
using Codout.Framework.Data.Repository;

namespace Codout.Framework.Application;

public abstract class AppServiceBase<TEntity> : IAppService<TEntity> where TEntity : class, IEntity
{
    protected AppServiceBase(IUnitOfWork unitOfWork, IRepository<TEntity> repository, IMapper mapper)
    {
        UnitOfWork = unitOfWork;
        Repository = repository;
        Mapper = mapper;
    }

    public IUnitOfWork UnitOfWork { get; }

    public IRepository<TEntity> Repository { get; }

    public IMapper Mapper { get; }
}