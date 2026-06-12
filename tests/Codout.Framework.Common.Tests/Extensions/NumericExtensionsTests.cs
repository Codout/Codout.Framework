using Codout.Framework.Common.Extensions;
using FluentAssertions;
using Xunit;

namespace Codout.Framework.Common.Tests.Extensions;

public class NumericExtensionsTests
{
    [Theory]
    [InlineData("123", true)]
    [InlineData("007", true)]
    [InlineData("0", false)]
    [InlineData("-5", false)]
    [InlineData("12.3", false)]
    [InlineData("abc", false)]
    public void IsNaturalNumber_ValidaNumeroNatural(string input, bool expected)
    {
        input.IsNaturalNumber().Should().Be(expected);
    }

    [Theory]
    [InlineData("123", true)]
    [InlineData("0", true)]
    [InlineData("-5", false)]
    [InlineData("1.5", false)]
    public void IsWholeNumber_ValidaNumeroInteiroSemSinal(string input, bool expected)
    {
        input.IsWholeNumber().Should().Be(expected);
    }

    [Theory]
    [InlineData("123", true)]
    [InlineData("-123", true)]
    [InlineData("--123", false)]
    [InlineData("1.5", false)]
    [InlineData("abc", false)]
    public void IsInteger_ValidaInteiroComSinal(string input, bool expected)
    {
        input.IsInteger().Should().Be(expected);
    }

    [Theory]
    [InlineData(0, true)]
    [InlineData(2, true)]
    [InlineData(-4, true)]
    [InlineData(3, false)]
    [InlineData(-7, false)]
    public void IsEven_ValidaPar(int value, bool expected)
    {
        value.IsEven().Should().Be(expected);
        value.IsOdd().Should().Be(!expected);
    }

    [Theory]
    [InlineData(3.14159, 2, 3.14)]
    [InlineData(3.999, 0, 3.0)]
    [InlineData(-1.239, 2, -1.23)]
    public void Truncate_TruncaCasasDecimais(double value, int precision, double expected)
    {
        value.Truncate(precision).Should().Be(expected);
    }

    [Fact]
    public void Random_ComLimites_RetornaDentroDoIntervalo()
    {
        for (var i = 0; i < 50; i++)
            NumericExtensions.Random(5, 10).Should().BeInRange(5, 9);
    }

    [Fact]
    public void Random_SemParametros_RetornaEntreZeroEUm()
    {
        NumericExtensions.Random().Should().BeInRange(0.0, 1.0);
    }

    [Fact]
    public void Random_ComLimiteSuperior_RetornaMenorQueLimite()
    {
#pragma warning disable CS0618 // Random(int) está marcado como Obsolete
        for (var i = 0; i < 50; i++)
            NumericExtensions.Random(10).Should().BeInRange(0, 9);
#pragma warning restore CS0618
    }
}
