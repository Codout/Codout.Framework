using FluentAssertions;
using Xunit;

namespace Codout.DynamicLinq.Tests;

public class ToDataSourceResultSortPageTests
{
    private static readonly Aggregator[] NoAggregates = [];
    private static readonly Group[] NoGroups = [];

    private static DataSourceResult Run(int take, int skip, IEnumerable<Sort> sort)
    {
        return TestData.People().ToDataSourceResult(take, skip, sort, null!, NoAggregates, NoGroups);
    }

    [Fact]
    public void Sort_Asc_DeveOrdenarCrescente()
    {
        var result = Run(10, 0, [new Sort { Field = "Age", Dir = "asc" }]);

        TestData.DataOf(result).Select(p => p.Age).Should().BeInAscendingOrder();
    }

    [Fact]
    public void Sort_Desc_DeveOrdenarDecrescente()
    {
        var result = Run(10, 0, [new Sort { Field = "Age", Dir = "desc" }]);

        TestData.DataOf(result).Select(p => p.Age).Should().BeInDescendingOrder();
    }

    [Fact]
    public void Sort_Multiplo_DeveOrdenarPorCampoSecundario()
    {
        var result = Run(10, 0,
        [
            new Sort { Field = "Category", Dir = "asc" },
            new Sort { Field = "Age", Dir = "desc" }
        ]);

        TestData.DataOf(result).Select(p => p.Id).Should().ContainInOrder(3, 1, 4, 2, 5);
    }

    [Fact]
    public void Sort_PorPropriedadeAninhada_DeveSerSuportado()
    {
        var result = Run(10, 0, [new Sort { Field = "Address.City", Dir = "asc" }]);

        TestData.DataOf(result).First().Address.City.Should().Be("Belo Horizonte");
    }

    [Fact]
    public void Paginacao_DeveAplicarSkipETake()
    {
        var result = Run(2, 1, [new Sort { Field = "Id", Dir = "asc" }]);

        result.Total.Should().Be(5, "o total reflete a contagem antes da paginação");
        TestData.DataOf(result).Select(p => p.Id).Should().ContainInOrder(2, 3);
    }

    [Fact]
    public void Paginacao_ComTakeZero_RetornaTodosOsRegistros()
    {
        var result = Run(0, 0, [new Sort { Field = "Id", Dir = "asc" }]);

        TestData.DataOf(result).Should().HaveCount(5);
    }

    [Fact]
    public void Paginacao_AlemDoFim_RetornaListaVazia()
    {
        var result = Run(10, 10, [new Sort { Field = "Id", Dir = "asc" }]);

        result.Total.Should().Be(5);
        TestData.DataOf(result).Should().BeEmpty();
    }

    [Fact]
    public void Total_DeveRefletirAContagemFiltrada()
    {
        var filter = TestData.Single("Category", "eq", "A");

        var result = TestData.People().ToDataSourceResult(1, 0, [], filter, NoAggregates, NoGroups);

        result.Total.Should().Be(2);
        TestData.DataOf(result).Should().HaveCount(1);
    }

    [Fact]
    public void SortNulo_SemGrupos_LancaArgumentNullException()
    {
        // BUG?: Sort() faz `sort as Sort[] ?? sort.ToArray()` — com sort nulo (e sem grupos
        // que o substituam), ToArray() lança ArgumentNullException em vez de ignorar a ordenação.
        var act = () => TestData.People().ToDataSourceResult(10, 0, null!, null!, NoAggregates, NoGroups);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void OverloadSemAggregatesEGroups_LancaArgumentNullException()
    {
        // BUG?: o overload de 4 parâmetros repassa aggregates/group nulos, e
        // Aggregates() faz `aggregates as Aggregator[] ?? aggregates.ToArray()`,
        // que lança ArgumentNullException. Ou seja, o overload "simples" documentado
        // é inutilizável como está.
        var act = () => TestData.People()
            .ToDataSourceResult(10, 0, [new Sort { Field = "Id", Dir = "asc" }], null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void OverloadComDataSourceRequestPadrao_LancaArgumentNullException()
    {
        // BUG?: mesma causa raiz do overload de 4 parâmetros — um DataSourceRequest
        // recém-criado (Sort/Aggregate/Group nulos) derruba a chamada.
        var act = () => TestData.People().ToDataSourceResult(new DataSourceRequest { Take = 10 });

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void OverloadComDataSourceRequestPreenchido_DeveFuncionar()
    {
        var request = new DataSourceRequest
        {
            Take = 2,
            Skip = 0,
            Sort = [new Sort { Field = "Age", Dir = "desc" }],
            Filter = null,
            Aggregate = [],
            Group = []
        };

        var result = TestData.People().ToDataSourceResult(request);

        result.Total.Should().Be(5);
        TestData.DataOf(result).Select(p => p.Age).Should().ContainInOrder(40, 35);
    }
}
