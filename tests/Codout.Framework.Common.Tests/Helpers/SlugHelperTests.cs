using Codout.Framework.Common.Helpers;
using FluentAssertions;
using Xunit;

namespace Codout.Framework.Common.Tests.Helpers;

public class SlugHelperTests
{
    [Theory]
    [InlineData("Olá Mundo!", "ola-mundo")]
    [InlineData("Ação & Reação", "acao-reacao")]
    [InlineData("hello world", "hello-world")]
    [InlineData("UPPER Case", "upper-case")]
    public void ToSlug_GeraSlugLimpo(string input, string expected)
    {
        input.ToSlug().Should().Be(expected);
    }

    [Fact]
    public void ToSlug_ColapsaEspacosEHifens()
    {
        "  muitos    espaços  --- e hifens  ".ToSlug().Should().Be("muitos-espacos-e-hifens");
    }

    [Fact]
    public void ToSlug_EntradaVaziaOuWhitespace_LancaArgumentException()
    {
        var actEmpty = () => "".ToSlug();
        var actSpace = () => "   ".ToSlug();
        actEmpty.Should().Throw<ArgumentException>();
        actSpace.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void ToSlug_ResultadoVazio_UsaFallbackComHash()
    {
        // Apenas caracteres não permitidos resultam em slug vazio → fallback "item-{hash}"
        "!!!@@@###".ToSlug().Should().StartWith("item-");
    }

    [Fact]
    public void ToSlug_ComAllowEmptySlug_RetornaVazio()
    {
        var config = new SlugConfig { AllowEmptySlug = true };
        "!!!".ToSlug(config).Should().BeEmpty();
    }

    [Fact]
    public void ToSlug_ComMaxLength_TruncaSemHifenFinal()
    {
        var config = new SlugConfig { MaxLength = 10 };
        var slug = "uma frase bem longa para truncar".ToSlug(config);
        slug.Length.Should().BeLessThanOrEqualTo(10);
        slug.Should().NotEndWith("-");
    }

    [Fact]
    public void ToSlug_ConfigStrict_SubstituiPontosEUnderscores()
    {
        "arquivo.nome_teste".ToSlug(SlugConfig.Strict).Should().Be("arquivo-nome-teste");
    }

    [Fact]
    public void ToSlug_ConfigExtended_PreservaMaiusculas()
    {
        "Hello World".ToSlug(SlugConfig.Extended).Should().Be("Hello-World");
    }

    [Fact]
    public void SlugHelper_ConfigNula_LancaArgumentNullException()
    {
        var act = () => new SlugHelper(null!);
        act.Should().Throw<ArgumentNullException>();
    }
}
