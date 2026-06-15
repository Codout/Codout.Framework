using Codout.DynamicLinq;
using FluentAssertions;
using Moq;
using Xunit;

namespace Codout.Framework.Application.Tests;

public class CrudAppServiceBaseTests
{
    private readonly CrudServiceFixture _fixture = new();

    [Fact]
    public void Construtor_DeveExporDependencias()
    {
        _fixture.Service.UnitOfWork.Should().BeSameAs(_fixture.UnitOfWork.Object);
        _fixture.Service.Repository.Should().BeSameAs(_fixture.Repository.Object);
        _fixture.Service.Mapper.Should().BeSameAs(_fixture.Mapper);
    }

    [Fact]
    public async Task GetAsync_QuandoEntidadeExiste_RetornaDtoMapeado()
    {
        var id = Guid.NewGuid();
        var entity = CrudServiceFixture.NewCustomer(id, "Ana", 30);
        _fixture.Repository.Setup(r => r.GetAsync(It.IsAny<object>())).ReturnsAsync(entity);

        var dto = await _fixture.Service.GetAsync(id);

        dto.Should().NotBeNull();
        dto.Id.Should().Be(id);
        dto.Name.Should().Be("Ana");
        dto.Age.Should().Be(30);
    }

    [Fact]
    public async Task GetAsync_QuandoEntidadeNaoExiste_RetornaNulo()
    {
        _fixture.Repository.Setup(r => r.GetAsync(It.IsAny<object>())).ReturnsAsync((Customer?)null);

        var dto = await _fixture.Service.GetAsync(Guid.NewGuid());

        dto.Should().BeNull();
    }

    [Fact]
    public async Task SaveAsync_DeveSalvarComitarERetornarDto()
    {
        var input = new CustomerDto { Id = Guid.NewGuid(), Name = "Bruno", Age = 25 };
        Customer? saved = null;
        _fixture.Repository
            .Setup(r => r.SaveAsync(It.IsAny<Customer>()))
            .Callback<Customer>(c => saved = c)
            .ReturnsAsync((Customer c) => c);

        var output = await _fixture.Service.SaveAsync(input);

        saved.Should().NotBeNull();
        saved!.Name.Should().Be("Bruno");
        saved.Age.Should().Be(25);
        _fixture.UnitOfWork.Verify(u => u.Commit(), Times.Once);
        output.Should().BeEquivalentTo(input);
    }

    [Fact]
    public async Task SaveAsync_ComEntradaNula_LancaNullReferenceException()
    {
        // BUG?: contrato esperado seria ArgumentNullException; o código lança
        // NullReferenceException e a mensagem usa nameof(TDto), que vira o texto
        // literal "TDto" em vez do nome real do DTO.
        var act = () => _fixture.Service.SaveAsync(null!);

        (await act.Should().ThrowAsync<NullReferenceException>())
            .WithMessage("*TDto*");

        _fixture.UnitOfWork.Verify(u => u.Commit(), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_QuandoEntidadeExiste_MapeiaComitaERetornaInput()
    {
        var id = Guid.NewGuid();
        var entity = CrudServiceFixture.NewCustomer(id, "Velho", 20);
        _fixture.Repository.Setup(r => r.GetAsync(It.IsAny<object>())).ReturnsAsync(entity);

        var input = new CustomerDto { Id = id, Name = "Novo", Age = 21 };
        var output = await _fixture.Service.UpdateAsync(input);

        output.Should().BeSameAs(input);
        entity.Name.Should().Be("Novo", "o DTO deve ser mapeado sobre a entidade rastreada");
        entity.Age.Should().Be(21);
        _fixture.UnitOfWork.Verify(u => u.Commit(), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_QuandoEntidadeNaoExiste_RetornaNuloESemCommit()
    {
        _fixture.Repository.Setup(r => r.GetAsync(It.IsAny<object>())).ReturnsAsync((Customer?)null);

        var output = await _fixture.Service.UpdateAsync(new CustomerDto { Id = Guid.NewGuid() });

        output.Should().BeNull();
        _fixture.UnitOfWork.Verify(u => u.Commit(), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_NaoChamaUpdateDoRepositorio()
    {
        // Observação de caracterização: UpdateAsync depende de a entidade estar
        // rastreada pelo ORM — nenhum método Update/SaveOrUpdate do repositório é chamado.
        var id = Guid.NewGuid();
        _fixture.Repository.Setup(r => r.GetAsync(It.IsAny<object>()))
            .ReturnsAsync(CrudServiceFixture.NewCustomer(id));

        await _fixture.Service.UpdateAsync(new CustomerDto { Id = id, Name = "X" });

        _fixture.Repository.Verify(r => r.Update(It.IsAny<Customer>()), Times.Never);
        _fixture.Repository.Verify(r => r.UpdateAsync(It.IsAny<Customer>()), Times.Never);
        _fixture.Repository.Verify(r => r.SaveOrUpdateAsync(It.IsAny<Customer>()), Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_QuandoEntidadeExiste_DeletaEComita()
    {
        var id = Guid.NewGuid();
        var entity = CrudServiceFixture.NewCustomer(id);
        _fixture.Repository.Setup(r => r.LoadAsync(It.IsAny<object>())).ReturnsAsync(entity);

        await _fixture.Service.DeleteAsync(id);

        _fixture.Repository.Verify(r => r.DeleteAsync(entity), Times.Once);
        _fixture.UnitOfWork.Verify(u => u.Commit(), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_QuandoEntidadeNaoExiste_NaoDeletaNemComita()
    {
        _fixture.Repository.Setup(r => r.LoadAsync(It.IsAny<object>())).ReturnsAsync((Customer?)null);

        await _fixture.Service.DeleteAsync(Guid.NewGuid());

        _fixture.Repository.Verify(r => r.DeleteAsync(It.IsAny<Customer>()), Times.Never);
        _fixture.UnitOfWork.Verify(u => u.Commit(), Times.Never);
    }

    [Fact]
    public async Task GetAllAsync_ComRequestPreenchido_RetornaPaginaETotal()
    {
        var customers = Enumerable.Range(1, 7)
            .Select(i => CrudServiceFixture.NewCustomer(Guid.NewGuid(), $"Cliente {i}", 20 + i))
            .AsQueryable();
        _fixture.Repository.Setup(r => r.All()).Returns(customers);

        var request = new DataSourceRequest
        {
            Take = 3,
            Skip = 3,
            Sort = [new Sort { Field = "Age", Dir = "asc" }],
            Aggregate = [],
            Group = []
        };

        var result = await _fixture.Service.GetAllAsync(request);

        result.Total.Should().Be(7);
        ((IEnumerable<Customer>)result.Data!).Select(c => c.Age).Should().ContainInOrder(24, 25, 26);
    }

    [Fact]
    public async Task GetAllAsync_ComFiltro_AplicaOFiltro()
    {
        var customers = new[]
        {
            CrudServiceFixture.NewCustomer(Guid.NewGuid(), "Ana", 30),
            CrudServiceFixture.NewCustomer(Guid.NewGuid(), "Bruno", 25)
        }.AsQueryable();
        _fixture.Repository.Setup(r => r.All()).Returns(customers);

        var request = new DataSourceRequest
        {
            Take = 10,
            Skip = 0,
            Sort = [],
            Aggregate = [],
            Group = [],
            Filter = new Filter
            {
                Logic = "and",
                Filters = [new Filter { Field = "Name", Operator = "eq", Value = "Ana" }]
            }
        };

        var result = await _fixture.Service.GetAllAsync(request);

        result.Total.Should().Be(1);
        ((IEnumerable<Customer>)result.Data!).Single().Name.Should().Be("Ana");
    }

    [Fact]
    public async Task GetAllAsync_ComRequestPadrao_LancaArgumentNullException()
    {
        // BUG?: um DataSourceRequest recém-criado (Sort/Aggregate/Group nulos) derruba
        // ToDataSourceResult com ArgumentNullException (`aggregates.ToArray()` sobre nulo).
        // Qualquer consumidor da API que poste um request "vazio" recebe erro 500.
        _fixture.Repository.Setup(r => r.All()).Returns(Array.Empty<Customer>().AsQueryable());

        var act = () => _fixture.Service.GetAllAsync(new DataSourceRequest { Take = 10 });

        await act.Should().ThrowAsync<ArgumentNullException>();
    }
}
