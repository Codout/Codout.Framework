using AutoMapper;
using Codout.Framework.Api.Client;
using Codout.Framework.Application;
using Codout.Framework.Data;
using Codout.Framework.Data.Repository;
using Codout.Framework.Domain.Entities;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace Codout.Framework.Application.Tests;

public class Customer : Entity<Guid>
{
    public string? Name { get; set; }

    public int Age { get; set; }
}

public class CustomerDto : EntityDto<Guid>
{
    public string? Name { get; set; }

    public int Age { get; set; }
}

public class CustomerAppService(
    IUnitOfWork unitOfWork,
    IRepository<Customer> repository,
    IMapper mapper)
    : CrudAppServiceBase<Customer, CustomerDto, Guid>(unitOfWork, repository, mapper);

public static class TestMapper
{
    public static IMapper Create()
    {
        var configuration = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Customer, CustomerDto>().ReverseMap();
        }, NullLoggerFactory.Instance);

        return configuration.CreateMapper();
    }
}

public class CrudServiceFixture
{
    public CrudServiceFixture()
    {
        Service = new CustomerAppService(UnitOfWork.Object, Repository.Object, Mapper);
    }

    public Mock<IUnitOfWork> UnitOfWork { get; } = new();

    public Mock<IRepository<Customer>> Repository { get; } = new();

    public IMapper Mapper { get; } = TestMapper.Create();

    public CustomerAppService Service { get; }

    public static Customer NewCustomer(Guid id, string name = "Ana", int age = 30)
    {
        var customer = new Customer { Name = name, Age = age };
        customer.SetId(id);
        return customer;
    }
}
