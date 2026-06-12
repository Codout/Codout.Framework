using Codout.Framework.Common.Helpers;
using FluentAssertions;
using Xunit;

namespace Codout.Framework.Common.Tests.Helpers;

public class InflectorTests
{
    [Theory]
    [InlineData("casa", "casas")]
    [InlineData("cão", "cães")]
    [InlineData("alemão", "alemães")]
    [InlineData("irmão", "irmãos")]
    [InlineData("papel", "papéis")]
    [InlineData("animal", "animais")]
    public void MakePlural_PluralizaPortugues(string singular, string plural)
    {
        singular.MakePlural().Should().Be(plural);
    }

    [Theory]
    [InlineData("casas", "casa")]
    [InlineData("cães", "cão")]
    [InlineData("animais", "animal")]
    public void MakeSingular_SingularizaPortugues(string plural, string singular)
    {
        plural.MakeSingular().Should().Be(singular);
    }

    [Fact]
    public void MakePlural_PalavraIncontavel_NaoAltera()
    {
        "fénix".MakePlural().Should().Be("fénix");
    }

    [Theory]
    [InlineData("MyTestString", "my_test_string")]
    [InlineData("my test", "my_test")]
    public void AddUnderscores_ConverteParaSnakeCase(string input, string expected)
    {
        input.AddUnderscores().Should().Be(expected);
    }

    [Fact]
    public void ToPascalCase_ConverteUnderscores()
    {
        "minha_propriedade_teste".ToPascalCase().Should().Be("MinhaPropriedadeTeste");
    }

    [Fact]
    public void ToCamelCase_ConverteUnderscores()
    {
        "minha_propriedade".ToCamelCase().Should().Be("minhaPropriedade");
    }

    [Fact]
    public void ToHumanCase_ConverteParaTextoLegivel()
    {
        "meu_campo_teste".ToHumanCase().Should().Be("Meu campo teste");
    }

    [Fact]
    public void ToTitleCase_CapitalizaCadaPalavra()
    {
        "meu_campo_teste".ToTitleCase().Should().Be("Meu Campo Teste");
    }

    [Fact]
    public void MakeInitialCapsELowerCase_AlteramPrimeiraLetra()
    {
        "teste ABC".MakeInitialCaps().Should().Be("Teste abc");
        "Teste".MakeInitialLowerCase().Should().Be("teste");
    }

    [Theory]
    [InlineData("1", "1st")]
    [InlineData("2", "2nd")]
    [InlineData("3", "3rd")]
    [InlineData("4", "4th")]
    [InlineData("11", "11th")]
    [InlineData("12", "12th")]
    [InlineData("13", "13th")]
    [InlineData("21", "21st")]
    [InlineData("abc", "abc")]
    public void AddOrdinalSuffix_AdicionaSufixoIngles(string input, string expected)
    {
        input.AddOrdinalSuffix().Should().Be(expected);
    }

    [Fact]
    public void ConvertUnderscoresToDashes_SubstituiUnderscores()
    {
        "a_b_c".ConvertUnderscoresToDashes().Should().Be("a-b-c");
    }
}
