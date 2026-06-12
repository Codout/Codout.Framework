using Codout.Mailer.Helpers;
using FluentAssertions;
using Xunit;

namespace Codout.Mailer.Tests;

public class HtmlUtilitiesTests
{
    #region ConvertToPlainText

    [Fact]
    public void ConvertToPlainText_DeveRemoverTagsHtml()
    {
        var resultado = HtmlUtilities.ConvertToPlainText("<div><span>Hello</span> <b>World</b></div>");

        resultado.Should().Contain("Hello");
        resultado.Should().Contain("World");
        resultado.Should().NotContain("<");
        resultado.Should().NotContain(">");
    }

    [Fact]
    public void ConvertToPlainText_DeveConverterParagrafoEmQuebraDeLinha()
    {
        var resultado = HtmlUtilities.ConvertToPlainText("<p>Linha1</p><p>Linha2</p>");

        resultado.Should().Be("\r\nLinha1\r\nLinha2");
    }

    [Fact]
    public void ConvertToPlainText_DeveConverterBrEmQuebraDeLinha()
    {
        var resultado = HtmlUtilities.ConvertToPlainText("Linha1<br>Linha2");

        resultado.Should().Be("Linha1\r\nLinha2");
    }

    [Fact]
    public void ConvertToPlainText_DeveIgnorarComentarios()
    {
        var resultado = HtmlUtilities.ConvertToPlainText("<!-- comentario -->texto");

        resultado.Should().Be("texto");
    }

    [Fact]
    public void ConvertToPlainText_DeveIgnorarScriptEStyle()
    {
        var resultado = HtmlUtilities.ConvertToPlainText(
            "<script>alert('x');</script><style>.a{color:red}</style>conteudo");

        resultado.Should().Be("conteudo");
    }

    [Fact]
    public void ConvertToPlainText_DeveDecodificarEntidadesHtml()
    {
        var resultado = HtmlUtilities.ConvertToPlainText("a &amp; b &lt;c&gt;");

        resultado.Should().Be("a & b <c>");
    }

    [Fact]
    public void ConvertToPlainText_DeveIgnorarTextoApenasComEspacos()
    {
        var resultado = HtmlUtilities.ConvertToPlainText("<div>   </div>ok");

        resultado.Should().Be("ok");
    }

    [Fact]
    public void ConvertToPlainText_ComStringVazia_DeveRetornarVazio()
    {
        HtmlUtilities.ConvertToPlainText(string.Empty).Should().BeEmpty();
    }

    #endregion

    #region CountWords

    [Fact]
    public void CountWords_ComTextoNuloOuVazio_DeveRetornarZero()
    {
        HtmlUtilities.CountWords(null!).Should().Be(0);
        HtmlUtilities.CountWords(string.Empty).Should().Be(0);
    }

    [Fact]
    public void CountWords_ComDuasPalavras_DeveRetornarDois()
    {
        HtmlUtilities.CountWords("uma duas").Should().Be(2);
    }

    [Fact]
    public void CountWords_ComQuebrasDeLinha_DeveContarComoSeparador()
    {
        HtmlUtilities.CountWords("uma\nduas\ntres").Should().Be(3);
    }

    [Fact]
    public void CountWords_ComEspacosDuplos_ContaEntradasVazias()
    {
        // BUG?: Split(' ', '\n') sem StringSplitOptions.RemoveEmptyEntries conta
        // entradas vazias — "uma  duas" (dois espaços) retorna 3 em vez de 2.
        // Teste de caracterização do comportamento atual.
        HtmlUtilities.CountWords("uma  duas").Should().Be(3);
    }

    #endregion

    #region Cut

    [Fact]
    public void Cut_ComTextoMenorQueLimite_DeveRetornarTextoOriginal()
    {
        HtmlUtilities.Cut("abc", 10).Should().Be("abc");
    }

    [Fact]
    public void Cut_ComTextoNulo_DeveRetornarNulo()
    {
        HtmlUtilities.Cut(null!, 10).Should().BeNull();
    }

    [Fact]
    public void Cut_ComTextoMaiorQueLimite_DeveTruncarComReticencias()
    {
        // text[..(length-4)] + " ..." => o resultado final tem exatamente 'length' chars.
        var resultado = HtmlUtilities.Cut("abcdefghij", 8);

        resultado.Should().Be("abcd ...");
        resultado.Should().HaveLength(8);
    }

    [Fact]
    public void Cut_ComLimiteMenorQueQuatro_LancaArgumentOutOfRange()
    {
        // BUG?: para length < 4 o slice text[..(length - 4)] usa índice negativo
        // e lança ArgumentOutOfRangeException. Teste de caracterização.
        var acao = () => HtmlUtilities.Cut("abcdefghij", 3);

        acao.Should().Throw<ArgumentOutOfRangeException>();
    }

    #endregion
}
