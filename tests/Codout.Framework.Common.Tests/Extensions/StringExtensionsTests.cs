using Codout.Framework.Common.Extensions;
using Codout.Framework.Common.Helpers;
using FluentAssertions;
using Xunit;

namespace Codout.Framework.Common.Tests.Extensions;

public class StringExtensionsTests
{
    [Theory]
    [InlineData("abc", "ABC", true)]
    [InlineData("abc", "abc", true)]
    [InlineData("abc", "abd", false)]
    public void Matches_ComparaIgnorandoCase(string source, string compare, bool expected)
    {
        source.Matches(compare).Should().Be(expected);
    }

    [Fact]
    public void MatchesTrimmed_IgnoraEspacosNasBordas()
    {
        "  abc  ".MatchesTrimmed("ABC").Should().BeTrue();
        " abc ".MatchesTrimmed("ab c").Should().BeFalse();
    }

    [Fact]
    public void MatchesRegex_ValidaPadrao()
    {
        "abc123".MatchesRegex(@"^[a-z]+\d+$").Should().BeTrue();
        "123abc".MatchesRegex(@"^[a-z]+\d+$").Should().BeFalse();
    }

    [Fact]
    public void Chop_RemoveUltimosCaracteres()
    {
        "abcdef".Chop(2).Should().Be("abcd");
        "abcdef".Chop().Should().Be("abcde");
        "ab".Chop(0).Should().Be("ab");
    }

    [Fact]
    public void Chop_ComStringAlvo_RemoveAtePadrao()
    {
        "documento.txt".Chop(".txt").Should().Be("documento");
    }

    [Fact]
    public void Chop_ComStringAlvoInexistente_LancaExcecao()
    {
        // BUG?: quando o padrão não é encontrado, LastIndexOf retorna -1 e o
        // código chama Remove(-1, 0), que lança ArgumentOutOfRangeException
        // em vez de retornar a string original.
        var act = () => "abc".Chop("zzz");
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Clip_RemoveCaracteresDoInicio()
    {
        "abcdef".Clip(2).Should().Be("cdef");
        "abcdef".Clip().Should().Be("bcdef");
        "ab".Clip(5).Should().Be("ab");
    }

    [Fact]
    public void Clip_ComStringAlvo_RemoveAtePadrao()
    {
        "prefixo:valor".Clip(":").Should().Be(":valor");
    }

    [Fact]
    public void FastReplace_SubstituiIgnorandoCase()
    {
        "Hello World, hello!".FastReplace("hello", "bye").Should().Be("bye World, bye!");
    }

    [Fact]
    public void FastReplace_OriginalNula_RetornaNull()
    {
        ((string?)null!).FastReplace("a", "b").Should().BeNull();
    }

    [Fact]
    public void Crop_RetornaTextoEntreDelimitadores()
    {
        "<b>negrito</b>".Crop("<b>", "</b>").Should().Be("negrito");
        "sem delimitador".Crop("<b>", "</b>").Should().Be(string.Empty);
    }

    [Fact]
    public void Squeeze_RemoveEspacosExcedentes()
    {
        "  a   b  c ".Squeeze().Should().Be("a b c");
    }

    [Fact]
    public void ToAlphaNumericOnly_RemoveCaracteresNaoAlfanumericos()
    {
        "ab-12!cd #34".ToAlphaNumericOnly().Should().Be("ab12cd34");
    }

    [Theory]
    [InlineData("(11) 98765-4321", "11987654321")]
    [InlineData("abc", "")]
    [InlineData("", "")]
    public void OnlyNumbers_RetornaSomenteDigitos(string input, string expected)
    {
        input.OnlyNumbers().Should().Be(expected);
    }

    [Fact]
    public void ToWords_SeparaPalavras()
    {
        " uma  frase de teste ".ToWords().Should().Equal("uma", "frase", "de", "teste");
    }

    [Fact]
    public void StripHtml_RemoveTags()
    {
        "<p>Texto <b>importante</b>&nbsp;&amp; outro</p>".StripHtml().Should().Be("Texto importante& outro");
    }

    [Fact]
    public void FindMatches_RetornaOcorrencias()
    {
        "a1 b2 c3".FindMatches(@"[a-z]\d").Should().Equal("a1", "b2", "c3");
    }

    [Fact]
    public void ToDelimitedList_ConcatenaComDelimitador()
    {
        new[] { "a", "b", "c" }.ToDelimitedList().Should().Be("a,b,c");
        new[] { "a", "b" }.ToDelimitedList(";").Should().Be("a;b");
    }

    [Fact]
    public void Strip_RemovePadroesSeparadosPorVirgula()
    {
        "abc123def".Strip(@"\d").Should().Be("abcdef");
    }

    [Fact]
    public void ToFormattedString_FormataComArgumentos()
    {
        "{0}-{1}".ToFormattedString(1, "a").Should().Be("1-a");
    }

    [Fact]
    public void ToEnum_ConvertePorNomeIgnorandoCase()
    {
        "friday".ToEnum<DayOfWeek>().Should().Be(DayOfWeek.Friday);
        "inexistente".ToEnum<DayOfWeek>().Should().Be(default(DayOfWeek));
    }

    [Theory]
    [InlineData("abcdef", 3, "abc")]
    [InlineData("ab", 5, "ab")]
    [InlineData("", 3, "")]
    public void Truncate_LimitaTamanho(string input, int max, string expected)
    {
        input.Truncate(max).Should().Be(expected);
    }

    [Fact]
    public void HtmlEncode_HtmlDecode_SaoInversos()
    {
        // BUG?: HtmlEncode substitui '&' por "&amp;" por ÚLTIMO, depois de já ter
        // gerado as entidades; com isso "é" vira "&amp;eacute;" (duplo escape) em
        // vez de "&eacute;". O roundtrip com HtmlDecode ainda funciona porque o
        // decode desfaz na ordem inversa.
        const string texto = "café & ação <tag>";
        var encoded = texto.HtmlEncode();

        encoded.Should().Contain("&amp;");
        encoded.Should().Contain("eacute;");
        encoded.Should().NotContain("é");
        encoded.Should().NotContain("<");

        encoded.HtmlDecode().Should().Be(texto);
    }

    [Fact]
    public void Pluralize_UsaSingularOuPluralConformeQuantidade()
    {
        1.Pluralize("casa").Should().Be("1 casa");
        2.Pluralize("casa").Should().Be("2 casas");
    }

    [Fact]
    public void RemoveAccents_ComportamentoAtual()
    {
        // BUG?: RemoveAccents usa Encoding.GetEncoding("iso-8859-8") (hebraico),
        // que não está registrado por padrão no .NET (Core); o método lança
        // ArgumentException em runtime moderno em vez de remover acentos.
        var act = () => "ação".RemoveAccents();
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void RemoveCharactersSpecial_SemReplaceAccents_RemoveSimbolos()
    {
        // replaceAccents = false para evitar o caminho do RemoveAccents (ver teste acima)
        "olá! @mundo#".RemoveCharactersSpecial(allowWhiteSpace: true, replaceAccents: false)
            .Should().Be("olá mundo");
    }
}
