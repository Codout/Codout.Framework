using FluentAssertions;
using Xunit;

namespace Codout.DynamicLinq.Tests;

public class AggregatesAndGroupTests
{
    private static object? GetAggregateValue(object aggregates, string field, string function)
    {
        var fieldObj = aggregates.GetType().GetProperty(field)?.GetValue(aggregates);
        fieldObj.Should().NotBeNull($"o objeto de agregados deveria ter a propriedade {field}");
        return fieldObj!.GetType().GetProperty(function)?.GetValue(fieldObj);
    }

    private static DataSourceResult RunAggregates(params Aggregator[] aggregators)
    {
        return TestData.People().ToDataSourceResult(10, 0, [], null!, aggregators, []);
    }

    [Fact]
    public void Aggregate_Sum_DeveSomarValores()
    {
        var result = RunAggregates(new Aggregator { Field = "Salary", Aggregate = "sum" });

        GetAggregateValue(result.Aggregates!, "Salary", "sum").Should().Be(10001.50m);
    }

    [Fact]
    public void Aggregate_Min_Max_DevemCalcularExtremos()
    {
        var result = RunAggregates(
            new Aggregator { Field = "Age", Aggregate = "min" },
            new Aggregator { Field = "Age", Aggregate = "max" });

        GetAggregateValue(result.Aggregates!, "Age", "min").Should().Be(25);
        GetAggregateValue(result.Aggregates!, "Age", "max").Should().Be(40);
    }

    [Fact]
    public void Aggregate_Average_DeveCalcularMedia()
    {
        var result = RunAggregates(new Aggregator { Field = "Salary", Aggregate = "average" });

        GetAggregateValue(result.Aggregates!, "Salary", "average").Should().Be(2000.30m);
    }

    [Fact]
    public void Aggregate_Count_DeveContarRegistros()
    {
        var result = RunAggregates(new Aggregator { Field = "Age", Aggregate = "count" });

        GetAggregateValue(result.Aggregates!, "Age", "count").Should().Be(5);
    }

    [Fact]
    public void Aggregate_Count_EmCampoNullable_ContaApenasNaoNulos()
    {
        var result = RunAggregates(new Aggregator { Field = "Score", Aggregate = "count" });

        GetAggregateValue(result.Aggregates!, "Score", "count").Should().Be(4);
    }

    [Fact]
    public void Aggregate_SobreConsultaFiltrada_ConsideraApenasOsFiltrados()
    {
        var filter = TestData.Single("Category", "eq", "A");

        var result = TestData.People().ToDataSourceResult(10, 0, [], filter,
            [new Aggregator { Field = "Salary", Aggregate = "sum" }], []);

        GetAggregateValue(result.Aggregates!, "Salary", "sum").Should().Be(4001.25m);
    }

    [Fact]
    public void Aggregate_Vazio_RetornaAggregatesNulo()
    {
        var result = RunAggregates();

        result.Aggregates.Should().BeNull();
    }

    [Fact]
    public void Aggregate_EmCampoInexistente_LancaExcecao()
    {
        var act = () => RunAggregates(new Aggregator { Field = "NaoExiste", Aggregate = "sum" });

        act.Should().Throw<Exception>();
    }

    [Fact]
    public void Group_SemAggregates_LancaAoEnumerarOsGrupos()
    {
        // BUG?: GroupByMany repassa Group.Aggregates (nulo por padrão) para
        // QueryableExtensions.Aggregates, que faz `aggregates.ToArray()` e lança
        // ArgumentNullException na enumeração. Todo Group precisa de Aggregates = []
        // para o agrupamento funcionar.
        var result = TestData.People()
            .ToDataSourceResult(10, 0, [], null!, [], [new Group { Field = "Category", Dir = "asc" }]);

        var act = () => ((IEnumerable<GroupResult>)result.Groups!).ToList();

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Group_DeveAgruparPorCampo()
    {
        var result = TestData.People()
            .ToDataSourceResult(10, 0, [], null!, [],
                [new Group { Field = "Category", Dir = "asc", Aggregates = [] }]);

        result.Data.Should().BeNull("quando há grupos o retorno vai em Groups");
        result.Groups.Should().NotBeNull();

        var groups = ((IEnumerable<GroupResult>)result.Groups!).ToList();

        groups.Select(g => (string)g.Value!).Should().ContainInOrder("A", "B", "C");
        groups.Select(g => g.Count).Should().ContainInOrder(2, 2, 1);
        groups.Should().OnlyContain(g => !g.HasSubgroups);
    }

    [Fact]
    public void Group_Aninhado_DeveMarcarHasSubgroups()
    {
        var result = TestData.People().ToDataSourceResult(10, 0, [], null!, [],
        [
            new Group { Field = "Category", Dir = "asc", Aggregates = [] },
            new Group { Field = "Address.City", Dir = "asc", Aggregates = [] }
        ]);

        var groups = ((IEnumerable<GroupResult>)result.Groups!).ToList();

        groups.Should().OnlyContain(g => g.HasSubgroups);

        var groupA = groups.Single(g => (string)g.Value! == "A");
        var subgroups = ((IEnumerable<GroupResult>)groupA.Items!).ToList();
        subgroups.Select(g => (string)g.Value!).Should().BeEquivalentTo("Vitoria", "Curitiba");
    }

    [Fact]
    public void Group_ComAggregates_DeveCalcularAgregadoPorGrupo()
    {
        var result = TestData.People().ToDataSourceResult(10, 0, [], null!, [],
        [
            new Group
            {
                Field = "Category",
                Dir = "asc",
                Aggregates = [new Aggregator { Field = "Salary", Aggregate = "sum" }]
            }
        ]);

        var groups = ((IEnumerable<GroupResult>)result.Groups!).ToList();
        var groupA = groups.Single(g => (string)g.Value! == "A");

        GetAggregateValue(groupA.Aggregates!, "Salary", "sum").Should().Be(4001.25m);
    }

    [Fact]
    public void Group_FieldExposeContagem()
    {
        var groupResult = new GroupResult { SelectorField = "Category", Count = 3 };

        groupResult.Field.Should().Be("Category (3)");
    }
}
