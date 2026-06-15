using FluentAssertions;
using Xunit;

namespace Codout.DynamicLinq.Tests;

public class FilterAndSortExpressionTests
{
    [Fact]
    public void Sort_ToExpression_DeveConcatenarCampoEDirecao()
    {
        new Sort { Field = "Name", Dir = "desc" }.ToExpression().Should().Be("Name desc");
    }

    [Fact]
    public void Filter_All_DeveAchatarFiltrosAninhados()
    {
        var leaf1 = new Filter { Field = "A", Operator = "eq", Value = 1 };
        var leaf2 = new Filter { Field = "B", Operator = "eq", Value = 2 };
        var leaf3 = new Filter { Field = "C", Operator = "eq", Value = 3 };

        var root = new Filter
        {
            Logic = "and",
            Filters = new List<Filter>
            {
                leaf1,
                new() { Logic = "or", Filters = new List<Filter> { leaf2, leaf3 } }
            }
        };

        root.All().Should().ContainInOrder(leaf1, leaf2, leaf3);
    }

    [Fact]
    public void Filter_All_DeFiltroSimples_RetornaEleMesmo()
    {
        var filter = new Filter { Field = "Age", Operator = "gt", Value = 1 };

        filter.All().Should().ContainSingle().Which.Should().BeSameAs(filter);
    }

    [Theory]
    [InlineData("eq", "Age = @0")]
    [InlineData("neq", "Age != @0")]
    [InlineData("lt", "Age < @0")]
    [InlineData("lte", "Age <= @0")]
    [InlineData("gt", "Age > @0")]
    [InlineData("gte", "Age >= @0")]
    public void Filter_ToExpression_OperadoresDeComparacao(string op, string expected)
    {
        var filter = new Filter { Field = "Age", Operator = op, Value = 1 };

        filter.ToExpression(typeof(Person), filter.All()).Should().Be(expected);
    }

    [Theory]
    [InlineData("contains", "Name != null && Name.Contains(@0)")]
    [InlineData("startswith", "Name != null && Name.StartsWith(@0)")]
    [InlineData("endswith", "Name != null && Name.EndsWith(@0)")]
    [InlineData("doesnotcontain", "Name != null && !Name.Contains(@0)")]
    public void Filter_ToExpression_OperadoresDeString(string op, string expected)
    {
        var filter = new Filter { Field = "Name", Operator = op, Value = "x" };

        filter.ToExpression(typeof(Person), filter.All()).Should().Be(expected);
    }

    [Theory]
    [InlineData("isnull", "Nickname = null")]
    [InlineData("isnotnull", "Nickname != null")]
    [InlineData("isempty", "Nickname = String.Empty")]
    [InlineData("isnotempty", "Nickname != String.Empty")]
    [InlineData("isnullorempty", "String.IsNullOrEmpty(Nickname)")]
    [InlineData("isnotnullorempty", "!String.IsNullOrEmpty(Nickname)")]
    public void Filter_ToExpression_OperadoresDeNuloEVazio(string op, string expected)
    {
        var filter = new Filter { Field = "Nickname", Operator = op };

        filter.ToExpression(typeof(Person), filter.All()).Should().Be(expected);
    }

    [Fact]
    public void Filter_ToExpression_Composto_DeveUsarIndicesDaListaAchatada()
    {
        var root = new Filter
        {
            Logic = "or",
            Filters = new List<Filter>
            {
                new() { Field = "Age", Operator = "gt", Value = 30 },
                new() { Field = "Name", Operator = "eq", Value = "Ana" }
            }
        };

        root.ToExpression(typeof(Person), root.All()).Should().Be("(Age > @0 or Name = @1)");
    }

    [Fact]
    public void Filter_ToExpression_OperadorDeString_EmCampoNaoString_Lanca()
    {
        var filter = new Filter { Field = "Age", Operator = "contains", Value = "3" };

        var act = () => filter.ToExpression(typeof(Person), filter.All());

        act.Should().Throw<NotSupportedException>().WithMessage("*contains*");
    }

    [Fact]
    public void Aggregator_MethodInfo_ParaCampoInexistente_LancaArgumentException()
    {
        var aggregator = new Aggregator { Field = "NaoExiste", Aggregate = "sum" };

        var act = () => aggregator.MethodInfo(typeof(Person));

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Aggregator_MethodInfo_ComTipoNulo_LancaArgumentNullException()
    {
        var aggregator = new Aggregator { Field = "Age", Aggregate = "sum" };

        var act = () => aggregator.MethodInfo(null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Aggregator_MethodInfo_ParaAgregadoDesconhecido_RetornaNulo()
    {
        var aggregator = new Aggregator { Field = "Age", Aggregate = "median" };

        aggregator.MethodInfo(typeof(Person)).Should().BeNull();
    }

    [Fact]
    public void DataSourceResult_Padrao_TemValoresNulosETotalZero()
    {
        var result = new DataSourceResult();

        result.Data.Should().BeNull();
        result.Groups.Should().BeNull();
        result.Aggregates.Should().BeNull();
        result.Errors.Should().BeNull();
        result.Total.Should().Be(0);
    }
}
