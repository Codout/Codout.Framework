using AutoMapper;
using Codout.Framework.Api.Client;
using Codout.Framework.Application.Interfaces;
using Codout.Framework.Data;
using Codout.Framework.Data.Repository;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Codout.Framework.Application.Tests;

public class RegisterServicesTests
{
    private static ServiceProvider BuildProvider()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddScoped(_ => Mock.Of<IUnitOfWork>());
        services.AddScoped(_ => Mock.Of<IRepository<Customer>>());

        services.AddCrudAppServices();

        return services.BuildServiceProvider();
    }

    [Fact]
    public void AddCrudAppServices_RegistraICrudAppServiceGenerico()
    {
        using var provider = BuildProvider();
        using var scope = provider.CreateScope();

        var service = scope.ServiceProvider.GetService<ICrudAppService<Customer, CustomerDto, Guid>>();

        service.Should().NotBeNull();
        service.Should().BeOfType<CrudAppServiceBase<Customer, CustomerDto, Guid>>();
    }

    [Fact]
    public void AddCrudAppServices_RegistraIMapper()
    {
        using var provider = BuildProvider();
        using var scope = provider.CreateScope();

        scope.ServiceProvider.GetService<IMapper>().Should().NotBeNull();
    }

    [Fact]
    public void AddCrudAppServices_RegistroEhScoped()
    {
        using var provider = BuildProvider();

        using var scope1 = provider.CreateScope();
        var a = scope1.ServiceProvider.GetRequiredService<ICrudAppService<Customer, CustomerDto, Guid>>();
        var b = scope1.ServiceProvider.GetRequiredService<ICrudAppService<Customer, CustomerDto, Guid>>();

        using var scope2 = provider.CreateScope();
        var c = scope2.ServiceProvider.GetRequiredService<ICrudAppService<Customer, CustomerDto, Guid>>();

        a.Should().BeSameAs(b);
        a.Should().NotBeSameAs(c);
    }

    [Fact]
    public void AddCrudAppServices_RetornaAMesmaColecao()
    {
        var services = new ServiceCollection();

        services.AddCrudAppServices().Should().BeSameAs(services);
    }
}

public class MappingProfileTests
{
    [Fact]
    public void MappingProfile_MapeiaEntityParaEntityDto_ViaMapaGenericoAberto()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddAutoMapper(x => x.AddMaps(typeof(MappingProfile)));
        using var provider = services.BuildServiceProvider();

        var mapper = provider.GetRequiredService<IMapper>();
        var entity = CrudServiceFixture.NewCustomer(Guid.NewGuid());

        var dto = mapper.Map<EntityDto<Guid>>(entity);

        dto.Id.Should().Be(entity.Id);
    }

    [Fact]
    public void MappingProfile_MapaReverso_NaoConsegueCriarEntityConcreta()
    {
        // BUG?: o ReverseMap de CreateMap(typeof(Entity<>), typeof(EntityDto<>)) casa o
        // destino com o tipo aberto Entity<>, e o AutoMapper tenta instanciar a classe
        // abstrata Entity<Guid> em vez do tipo concreto pedido (Customer). Na prática,
        // Mapper.Map<TEntity>(dto) — usado por CrudAppServiceBase.SaveAsync — falha se o
        // consumidor não registrar um mapa próprio DTO→Entidade.
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddAutoMapper(x => x.AddMaps(typeof(MappingProfile)));
        using var provider = services.BuildServiceProvider();

        var mapper = provider.GetRequiredService<IMapper>();

        var act = () => mapper.Map<Customer>(new EntityDto<Guid> { Id = Guid.NewGuid() });

        act.Should().Throw<ArgumentException>()
            .WithMessage("*abstract type*Entity*");
    }
}
