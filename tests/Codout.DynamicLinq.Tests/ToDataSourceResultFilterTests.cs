using FluentAssertions;
using Xunit;

namespace Codout.DynamicLinq.Tests;

public class ToDataSourceResultFilterTests
{
    private static readonly Sort[] NoSort = [];
    private static readonly Aggregator[] NoAggregates = [];
    private static readonly Group[] NoGroups = [];

    private static DataSourceResult Run(Filter? filter, int take = 10, int skip = 0)
    {
        return TestData.People().ToDataSourceResult(take, skip, NoSort, filter!, NoAggregates, NoGroups);
    }

    [Fact]
    public void Filtro_Eq_DeveRetornarApenasRegistroIgual()
    {
        var result = Run(TestData.Single("Name", "eq", "Ana"));

        result.Total.Should().Be(1);
        TestData.DataOf(result).Should().ContainSingle(p => p.Name == "Ana");
    }

    [Fact]
    public void Filtro_Neq_DeveExcluirRegistro()
    {
        var result = Run(TestData.Single("Name", "neq", "Ana"));

        result.Total.Should().Be(4);
        TestData.DataOf(result).Should().NotContain(p => p.Name == "Ana");
    }

    [Theory]
    [InlineData("gt", 30, new[] { 3, 4 })]
    [InlineData("gte", 30, new[] { 1, 3, 4 })]
    [InlineData("lt", 28, new[] { 2 })]
    [InlineData("lte", 28, new[] { 2, 5 })]
    public void Filtros_DeComparacao_DevemFiltrarPorIdade(string op, int value, int[] expectedIds)
    {
        var result = Run(TestData.Single("Age", op, value));

        TestData.DataOf(result).Select(p => p.Id).Should().BeEquivalentTo(expectedIds);
    }

    [Fact]
    public void Filtro_StartsWith_DeveFiltrarPorPrefixo()
    {
        var result = Run(TestData.Single("Name", "startswith", "Br"));

        TestData.DataOf(result).Should().ContainSingle(p => p.Name == "Bruno");
    }

    [Fact]
    public void Filtro_EndsWith_DeveFiltrarPorSufixo()
    {
        var result = Run(TestData.Single("Name", "endswith", "la"));

        TestData.DataOf(result).Should().ContainSingle(p => p.Name == "Carla");
    }

    [Fact]
    public void Filtro_Contains_DeveFiltrarPorTrecho()
    {
        var result = Run(TestData.Single("Name", "contains", "an"));

        // "Daniel" contém "an"; a comparação é case-sensitive, então "Ana" não entra.
        TestData.DataOf(result).Select(p => p.Name).Should().BeEquivalentTo("Daniel");
    }

    [Fact]
    public void Filtro_Contains_EmCampoNulo_NaoDeveLancarExcecao()
    {
        // O predicado gerado inclui o null-check: "Nickname != null && Nickname.Contains(@0)"
        var result = Run(TestData.Single("Nickname", "contains", "D"));

        TestData.DataOf(result).Select(p => p.Name).Should().BeEquivalentTo("Daniel", "Eduarda");
    }

    [Fact]
    public void Filtro_DoesNotContain_DeveExcluirTrechoENulos()
    {
        var result = Run(TestData.Single("Nickname", "doesnotcontain", "D"));

        // Bruno (Nickname null) também fica de fora, pois o predicado exige Nickname != null.
        TestData.DataOf(result).Select(p => p.Name).Should().BeEquivalentTo("Ana", "Carla");
    }

    [Fact]
    public void Filtro_IsNull_DeveRetornarApenasNulos()
    {
        var result = Run(TestData.Single("Nickname", "isnull", null));

        TestData.DataOf(result).Should().ContainSingle(p => p.Name == "Bruno");
    }

    [Fact]
    public void Filtro_IsNotNull_DeveExcluirNulos()
    {
        var result = Run(TestData.Single("Nickname", "isnotnull", null));

        TestData.DataOf(result).Select(p => p.Name).Should().BeEquivalentTo("Ana", "Carla", "Daniel", "Eduarda");
    }

    [Fact]
    public void Filtro_IsEmpty_DeveRetornarApenasStringVazia()
    {
        var result = Run(TestData.Single("Nickname", "isempty", null));

        TestData.DataOf(result).Should().ContainSingle(p => p.Name == "Carla");
    }

    [Fact]
    public void Filtro_IsNotEmpty_DeveExcluirStringVazia()
    {
        var result = Run(TestData.Single("Nickname", "isnotempty", null));

        // null != "" é verdadeiro, então Bruno (null) também é retornado.
        TestData.DataOf(result).Select(p => p.Name).Should().BeEquivalentTo("Ana", "Bruno", "Daniel", "Eduarda");
    }

    [Fact]
    public void Filtro_IsNullOrEmpty_DeveRetornarNulosEVazios()
    {
        var result = Run(TestData.Single("Nickname", "isnullorempty", null));

        TestData.DataOf(result).Select(p => p.Name).Should().BeEquivalentTo("Bruno", "Carla");
    }

    [Fact]
    public void Filtro_IsNotNullOrEmpty_DeveRetornarPreenchidos()
    {
        var result = Run(TestData.Single("Nickname", "isnotnullorempty", null));

        TestData.DataOf(result).Select(p => p.Name).Should().BeEquivalentTo("Ana", "Daniel", "Eduarda");
    }

    [Fact]
    public void Filtro_Composto_ComLogicAnd_DeveAplicarTodasAsCondicoes()
    {
        var filter = new Filter
        {
            Logic = "and",
            Filters = new List<Filter>
            {
                new() { Field = "Category", Operator = "eq", Value = "A" },
                new() { Field = "Age", Operator = "gt", Value = 30 }
            }
        };

        var result = Run(filter);

        TestData.DataOf(result).Should().ContainSingle(p => p.Name == "Carla");
    }

    [Fact]
    public void Filtro_Composto_ComLogicOr_DeveAplicarQualquerCondicao()
    {
        var filter = new Filter
        {
            Logic = "or",
            Filters = new List<Filter>
            {
                new() { Field = "Name", Operator = "eq", Value = "Ana" },
                new() { Field = "Name", Operator = "eq", Value = "Bruno" }
            }
        };

        var result = Run(filter);

        TestData.DataOf(result).Select(p => p.Name).Should().BeEquivalentTo("Ana", "Bruno");
    }

    [Fact]
    public void Filtro_Aninhado_DeveCombinarLogicasDiferentes()
    {
        // Category == "B" and (Age < 30 or Age > 34)
        var filter = new Filter
        {
            Logic = "and",
            Filters = new List<Filter>
            {
                new() { Field = "Category", Operator = "eq", Value = "B" },
                new()
                {
                    Logic = "or",
                    Filters = new List<Filter>
                    {
                        new() { Field = "Age", Operator = "lt", Value = 30 },
                        new() { Field = "Age", Operator = "gt", Value = 34 }
                    }
                }
            }
        };

        var result = Run(filter);

        TestData.DataOf(result).Select(p => p.Name).Should().BeEquivalentTo("Bruno", "Daniel");
    }

    [Fact]
    public void Filtro_EmPropriedadeAninhada_DeveSerSuportado()
    {
        var result = Run(TestData.Single("Address.City", "eq", "Vitoria"));

        TestData.DataOf(result).Select(p => p.Id).Should().BeEquivalentTo(new[] { 1, 4 });
    }

    [Fact]
    public void Filtro_Decimal_DeveConverterValorParaDecimal()
    {
        // PreliminaryWork converte o valor (double) para decimal antes do Where.
        var result = Run(TestData.Single("Salary", "gt", 2000.0));

        TestData.DataOf(result).Select(p => p.Name).Should().BeEquivalentTo("Carla", "Eduarda");
    }

    [Fact]
    public void Filtro_DateTime_Eq_MeiaNoite_DeveCasarComODiaInteiro()
    {
        // PreliminaryWork expande "eq meia-noite" para o intervalo [00:00:00, 23:59:59]
        // do dia (em horário local; o ambiente de teste roda em UTC).
        var result = Run(TestData.Single("BirthDate", "eq", "1994-05-10T00:00:00"));

        TestData.DataOf(result).Should().ContainSingle(p => p.Name == "Ana");
    }

    [Fact]
    public void Filtro_DateTime_Gt_DeveCompararDatas()
    {
        var result = Run(TestData.Single("BirthDate", "gt", new DateTime(1994, 1, 1)));

        TestData.DataOf(result).Select(p => p.Name).Should().BeEquivalentTo("Ana", "Bruno", "Eduarda");
    }

    [Fact]
    public void Filtro_SemLogic_EhIgnoradoSilenciosamente()
    {
        // BUG?: um Filter simples (sem Logic) não é aplicado — Filters() exige Logic != null,
        // então o chamador recebe todos os registros sem nenhum aviso.
        var filter = new Filter { Field = "Name", Operator = "eq", Value = "Ana" };

        var result = Run(filter);

        result.Total.Should().Be(5);
        result.Errors.Should().BeNull();
    }

    [Fact]
    public void Filtro_ComOperadorDesconhecido_DeveRegistrarErroERetornarTudo()
    {
        var result = Run(TestData.Single("Name", "like", "Ana"));

        result.Total.Should().Be(5);
        result.Errors.Should().NotBeNull();
        ((IEnumerable<object>)result.Errors!).Should().NotBeEmpty();
    }

    [Fact]
    public void Filtro_DeString_EmCampoNumerico_DeveRegistrarErroERetornarTudo()
    {
        // ToExpression lança NotSupportedException, que é capturada e vai para Errors.
        var result = Run(TestData.Single("Age", "contains", "3"));

        result.Total.Should().Be(5);
        result.Errors.Should().NotBeNull();
    }

    [Fact]
    public void Filtro_Nulo_NaoFiltraNada()
    {
        var result = Run(null);

        result.Total.Should().Be(5);
        TestData.DataOf(result).Should().HaveCount(5);
    }
}
