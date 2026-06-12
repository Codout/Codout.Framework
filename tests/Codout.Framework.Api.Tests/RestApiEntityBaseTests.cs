using Codout.DynamicLinq;
using Codout.Framework.Api.Client;
using Codout.Framework.Application.Interfaces;
using Codout.Framework.Domain.Entities;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace Codout.Framework.Api.Tests;

public class Product : Entity<Guid>
{
    public string? Name { get; set; }
}

public class ProductDto : EntityDto<Guid>
{
    public string? Name { get; set; }
}

public class ProductsController(ICrudAppService<Product, ProductDto, Guid> appService)
    : RestApiEntityBase<Product, ProductDto, Guid>(appService);

public class RestApiEntityBaseTests
{
    private readonly Mock<ICrudAppService<Product, ProductDto, Guid>> _appService = new();
    private readonly ProductsController _controller;

    public RestApiEntityBaseTests()
    {
        _controller = new ProductsController(_appService.Object);
    }

    [Fact]
    public void Construtor_DeveExporOAppService()
    {
        _controller.AppService.Should().BeSameAs(_appService.Object);
    }

    [Fact]
    public async Task Get_DeveRetornarOkComODto()
    {
        var id = Guid.NewGuid();
        var dto = new ProductDto { Id = id, Name = "Caneta" };
        _appService.Setup(s => s.GetAsync(id)).ReturnsAsync(dto);

        var result = await _controller.Get(id);

        result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().BeSameAs(dto);
    }

    [Fact]
    public async Task Get_QuandoNaoEncontrado_RetornaOkComCorpoNulo()
    {
        // BUG?: o controller sempre responde 200, mesmo quando o serviço retorna null —
        // o contrato documenta 404 (ProducesResponseType Status404NotFound), mas o
        // chamador recebe `200 OK` com corpo vazio.
        _appService.Setup(s => s.GetAsync(It.IsAny<Guid>())).ReturnsAsync((ProductDto)null!);

        var result = await _controller.Get(Guid.NewGuid());

        result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().BeNull();
    }

    [Fact]
    public async Task Post_DeveSalvarERetornarOk()
    {
        var input = new ProductDto { Name = "Lápis" };
        var saved = new ProductDto { Id = Guid.NewGuid(), Name = "Lápis" };
        _appService.Setup(s => s.SaveAsync(input)).ReturnsAsync(saved);

        var result = await _controller.Post(input);

        _appService.Verify(s => s.SaveAsync(input), Times.Once);
        result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().BeSameAs(saved);
    }

    [Fact]
    public async Task Put_DeveSobrescreverOIdDoDtoComODaRota()
    {
        var routeId = Guid.NewGuid();
        var input = new ProductDto { Id = Guid.NewGuid(), Name = "Borracha" };
        _appService.Setup(s => s.UpdateAsync(input)).ReturnsAsync(input);

        var result = await _controller.Put(routeId, input);

        input.Id.Should().Be(routeId, "o id da rota tem precedência sobre o id do corpo");
        _appService.Verify(s => s.UpdateAsync(input), Times.Once);
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Delete_DeveChamarOServicoERetornarOk()
    {
        var id = Guid.NewGuid();

        var result = await _controller.Delete(id);

        _appService.Verify(s => s.DeleteAsync(id), Times.Once);
        result.Should().BeOfType<OkResult>();
    }

    [Fact]
    public async Task GetAll_DeveRetornarOkComODataSourceResult()
    {
        var request = new DataSourceRequest { Take = 10 };
        var dataSourceResult = new DataSourceResult { Total = 3 };
        _appService.Setup(s => s.GetAllAsync(request)).ReturnsAsync(dataSourceResult);

        var result = await _controller.GetAll(request);

        result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().BeSameAs(dataSourceResult);
    }

    [Fact]
    public async Task ExcecaoDoServico_NaoEhTratadaNoController()
    {
        // O tratamento de erro fica a cargo do ApiExceptionMiddleware.
        _appService.Setup(s => s.GetAsync(It.IsAny<Guid>())).ThrowsAsync(new InvalidOperationException("boom"));

        var act = () => _controller.Get(Guid.NewGuid());

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("boom");
    }

    [Fact]
    public void RotasDosMetodos_SeguemOPadraoRest()
    {
        var type = typeof(RestApiEntityBase<Product, ProductDto, Guid>);

        type.GetMethod("Get")!.GetCustomAttributes(typeof(HttpGetAttribute), true)
            .Cast<HttpGetAttribute>().Single().Template.Should().Be("{id}");
        type.GetMethod("Post")!.GetCustomAttributes(typeof(HttpPostAttribute), true)
            .Cast<HttpPostAttribute>().Single().Template.Should().Be("");
        type.GetMethod("Put")!.GetCustomAttributes(typeof(HttpPutAttribute), true)
            .Cast<HttpPutAttribute>().Single().Template.Should().Be("{id}");
        type.GetMethod("Delete")!.GetCustomAttributes(typeof(HttpDeleteAttribute), true)
            .Cast<HttpDeleteAttribute>().Single().Template.Should().Be("{id}");
        type.GetMethod("GetAll")!.GetCustomAttributes(typeof(HttpPostAttribute), true)
            .Cast<HttpPostAttribute>().Single().Template.Should().Be("get-all");
    }
}
