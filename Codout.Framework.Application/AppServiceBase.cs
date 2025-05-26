using AutoMapper;
using Codout.Framework.Application.Interfaces;
using Codout.Framework.DAL;
using Codout.Framework.DAL.Entity;
using Codout.Framework.DAL.Repository;

namespace Codout.Framework.Application
{
    public abstract class AppServiceBase<TEntity> : IAppService<TEntity> where TEntity : class, IEntity
    {
        public IUnitOfWork UnitOfWork { get; }

        public IRepository<TEntity> Repository { get; }

        public IMapper Mapper { get; }

        protected AppServiceBase(IUnitOfWork unitOfWork, IRepository<TEntity> repository, IMapper mapper)
        {
            UnitOfWork = unitOfWork;
            Repository = repository;
            Mapper = mapper;
        }
    }
}

