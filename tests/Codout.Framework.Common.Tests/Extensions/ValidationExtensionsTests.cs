using Codout.Framework.Common.Extensions;
using FluentAssertions;
using Xunit;

namespace Codout.Framework.Common.Tests.Extensions;

public class ValidationExtensionsTests
{
    [Theory]
    [InlineData("usuario@dominio.com", true)]
    [InlineData("nome.sobrenome@empresa.com.br", true)]
    [InlineData("sem-arroba.com", false)]
    [InlineData("a@b", false)]
    public void IsEmail_ValidaFormato(string input, bool expected)
    {
        input.IsEmail().Should().Be(expected);
    }

    [Fact]
    public void IsEmail_StringVazia_RetornaTrue()
    {
        // BUG?: e-mail vazio/whitespace é considerado válido pela implementação
        // atual (provavelmente para campos opcionais). Documentando o comportamento.
        string.Empty.IsEmail().Should().BeTrue();
        "   ".IsEmail().Should().BeTrue();
    }

    [Theory]
    [InlineData("529.982.247-25", true)]  // CPF válido conhecido
    [InlineData("52998224725", true)]     // mesmo CPF sem máscara
    [InlineData("529.982.247-24", false)] // dígito verificador errado
    [InlineData("11111111111", false)]    // sequência repetida
    [InlineData("123", false)]
    [InlineData("", false)]
    [InlineData(null, false)]
    public void IsCpf_ValidaDigitosVerificadores(string? input, bool expected)
    {
        input!.IsCpf().Should().Be(expected);
    }

    [Theory]
    [InlineData("11.222.333/0001-81", true)]  // CNPJ válido conhecido
    [InlineData("11222333000181", true)]
    [InlineData("11.222.333/0001-80", false)] // dígito errado
    [InlineData("123", false)]
    public void IsCnpj_ValidaDigitosVerificadores(string input, bool expected)
    {
        input.IsCnpj().Should().Be(expected);
    }

    [Theory]
    [InlineData("01310-100", true)]
    [InlineData("01310100", true)]
    [InlineData("01.310-100", true)]
    [InlineData("1310-100", false)]
    [InlineData("abcde-fgh", false)]
    public void IsCep_ValidaFormato(string input, bool expected)
    {
        input.IsCep().Should().Be(expected);
    }

    [Fact]
    public void IsGuid_ValidaFormato()
    {
        Guid.NewGuid().ToString().IsGuid().Should().BeTrue();
        "não-é-um-guid".IsGuid().Should().BeFalse();
    }

    [Theory]
    [InlineData("192.168.0.1", true)]
    [InlineData("255.255.255.255", true)]
    [InlineData("256.1.1.1", false)]
    [InlineData("1.2.3", false)]
    public void IsIpAddress_ValidaFormato(string input, bool expected)
    {
        input.IsIpAddress().Should().Be(expected);
    }

    [Theory]
    [InlineData("https://www.exemplo.com", true)]
    [InlineData("http://exemplo.com/caminho?x=1", true)]
    [InlineData("ftp://arquivos.exemplo.com", true)]
    [InlineData("não é url", false)]
    public void IsUrl_ValidaFormato(string input, bool expected)
    {
        input.IsUrl().Should().Be(expected);
    }

    [Theory]
    [InlineData("Senha123!", true)]
    [InlineData("abcdefgh", false)] // sem número/maiúscula/símbolo
    [InlineData("Ab1!", false)]     // curta demais
    public void IsStrongPassword_ValidaComplexidade(string input, bool expected)
    {
        input.IsStrongPassword().Should().Be(expected);
    }

    [Fact]
    public void IsValidLuhn_ValidaAlgoritmo()
    {
        // 4111111111111111 é um número de teste Visa que passa no Luhn
        var valid = "4111111111111111".Select(c => c - '0').ToArray();
        valid.IsValidLuhn().Should().BeTrue();

        var invalid = "4111111111111112".Select(c => c - '0').ToArray();
        invalid.IsValidLuhn().Should().BeFalse();
    }

    [Theory]
    [InlineData("4111 1111 1111 1111", true)]  // Visa de teste
    [InlineData("5500-0000-0000-0004", true)]  // MasterCard de teste
    [InlineData("1234567890123456", false)]
    public void IsCreditCardAny_ValidaCartoes(string input, bool expected)
    {
        input.IsCreditCardAny().Should().Be(expected);
    }

    [Fact]
    public void IsCreditCardVisa_ValidaSomenteVisa()
    {
        "4111111111111111".IsCreditCardVisa().Should().BeTrue();
        "5500000000000004".IsCreditCardVisa().Should().BeFalse();
    }

    [Fact]
    public void CleanCreditCardNumber_RemoveNaoNumericos()
    {
        "4111-1111 1111x1111".CleanCreditCardNumber().Should().Be("4111111111111111");
    }

    [Theory]
    [InlineData("12345", true)]
    [InlineData("12345-6789", true)]
    [InlineData("1234", false)]
    public void IsZipCodeAny_ValidaFormatoAmericano(string input, bool expected)
    {
        input.IsZipCodeAny().Should().Be(expected);
    }

    [Fact]
    public void IsInscricaoEstadual_Isento_RetornaTrue()
    {
        ValidationExtensions.IsInscricaoEstadual("SP", "ISENTO").Should().BeTrue();
    }

    [Fact]
    public void IsInscricaoEstadual_NumeroInvalido_RetornaFalse()
    {
        ValidationExtensions.IsInscricaoEstadual("MT", "00000000001").Should().BeFalse();
    }
}
