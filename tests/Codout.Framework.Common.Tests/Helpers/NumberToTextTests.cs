using Codout.Framework.Common.Helpers;
using FluentAssertions;
using Xunit;

namespace Codout.Framework.Common.Tests.Helpers;

public class NumberToTextTests
{
    [Theory]
    [InlineData("1", "hum real")]
    [InlineData("2", "dois reais")]
    [InlineData("2.50", "dois reais e cinqüenta centavos")]
    [InlineData("0.01", "um centavo")]
    [InlineData("0.25", "vinte e cinco centavos")]
    [InlineData("100", "cem reais")]
    [InlineData("101", "cento e um reais")]
    [InlineData("1000", "hum mil reais")]
    public void ToString_ConverteValorPorExtenso(string valor, string expected)
    {
        var n = new NumberToText(decimal.Parse(valor, System.Globalization.CultureInfo.InvariantCulture));
        n.ToString().Should().Be(expected);
    }

    [Fact]
    public void ToString_Zero_RetornaVazio()
    {
        new NumberToText(0m).ToString().Should().BeEmpty();
    }

    [Fact]
    public void SetNumero_PermiteReuso()
    {
        var n = new NumberToText();
        n.SetNumero(2m);
        n.ToString().Should().Be("dois reais");

        n.SetNumero(3m);
        n.ToString().Should().Be("três reais");
    }

    [Fact]
    public void ToString_ArredondaParaDuasCasas()
    {
        new NumberToText(1.999m).ToString().Should().Be("dois reais");
    }
}
