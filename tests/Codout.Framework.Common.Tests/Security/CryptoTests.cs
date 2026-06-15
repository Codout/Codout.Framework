using Codout.Framework.Common.Security;
using FluentAssertions;
using Xunit;

namespace Codout.Framework.Common.Tests.Security;

public class CryptoTests
{
#pragma warning disable CS0618 // Métodos legados marcados como Obsolete
    [Fact]
    public void Md5Encrypt_RetornaHashConhecido()
    {
        // Vetor de teste padrão do MD5 para "abc"
        Crypto.Md5Encrypt("abc").Should().Be("900150983CD24FB0D6963F7D28E17F72");
    }

    [Fact]
    public void Sha1Encrypt_RetornaHashConhecido()
    {
        Crypto.Sha1Encrypt("abc").Should().Be("A9993E364706816ABA3E25717850C26C9CD0D89D");
    }

    [Fact]
    public void Sha256Encrypt_RetornaHashConhecido()
    {
        Crypto.Sha256Encrypt("abc")
            .Should().Be("BA7816BF8F01CFEA414140DE5DAE2223B00361A396177A9CB410FF61F20015AD");
    }

    [Fact]
    public void Sha512Encrypt_RetornaHashCom128CaracteresHex()
    {
        var hash = Crypto.Sha512Encrypt("abc");
        hash.Should().HaveLength(128);
        hash.Should().MatchRegex("^[0-9A-F]+$");
    }
#pragma warning restore CS0618

    [Fact]
    public void ByteArrayToString_ConverteParaHexMaiusculo()
    {
        Crypto.ByteArrayToString(new byte[] { 0x00, 0xAB, 0xFF }).Should().Be("00ABFF");
    }
}
