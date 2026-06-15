using System.Security.Cryptography;
using Codout.Framework.Common.Security;
using FluentAssertions;
using Xunit;

namespace Codout.Framework.Common.Tests.Security;

public class CryptoStringTests
{
    // Senha que atende aos critérios de "senha forte" (>= 12 chars, maiúscula,
    // minúscula, dígito e caractere especial)
    private const string StrongPassword = "S3nh@F0rte!2026";

    [Fact]
    public void EncryptDecrypt_Roundtrip_RecuperaTextoOriginal()
    {
        const string texto = "informação confidencial: çãé€";

        var encrypted = CryptoString.Encrypt(texto, StrongPassword);
        var decrypted = CryptoString.Decrypt(encrypted, StrongPassword);

        decrypted.Should().Be(texto);
    }

    [Fact]
    public void Encrypt_MesmoTexto_GeraCiphertextsDiferentes()
    {
        var c1 = CryptoString.Encrypt("texto", StrongPassword);
        var c2 = CryptoString.Encrypt("texto", StrongPassword);
        c1.Should().NotBe(c2); // salt e nonce aleatórios
    }

    [Fact]
    public void Decrypt_SenhaErrada_LancaCryptographicException()
    {
        var encrypted = CryptoString.Encrypt("texto", StrongPassword);
        var act = () => CryptoString.Decrypt(encrypted, "0utr@Senh4!Forte");
        act.Should().Throw<CryptographicException>();
    }

    [Fact]
    public void Decrypt_CiphertextCorrompido_LancaExcecao()
    {
        var encrypted = CryptoString.Encrypt("texto", StrongPassword);
        var bytes = Convert.FromBase64String(encrypted);
        bytes[^1] ^= 0xFF; // corrompe o último byte
        var corrompido = Convert.ToBase64String(bytes);

        var act = () => CryptoString.Decrypt(corrompido, StrongPassword);
        act.Should().Throw<CryptographicException>();
    }

    [Fact]
    public void Decrypt_Base64Invalido_LancaArgumentException()
    {
        var act = () => CryptoString.Decrypt("@@não-base64@@", StrongPassword);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Encrypt_SenhaFraca_ComOpcoesPadrao_LancaArgumentException()
    {
        var act = () => CryptoString.Encrypt("texto", "fraca");
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Encrypt_SenhaFraca_ComOpcoesLegacy_Funciona()
    {
        var encrypted = CryptoString.Encrypt("texto", "abc123", CryptoOptions.Legacy);
        CryptoString.Decrypt(encrypted, "abc123", CryptoOptions.Legacy).Should().Be("texto");
    }

    [Fact]
    public void Encrypt_TextoVazio_LancaArgumentException()
    {
        var act = () => CryptoString.Encrypt("", StrongPassword);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void ExtensionMethods_EncryptDecrypt_Funcionam()
    {
        var encrypted = "segredo".Encrypt(StrongPassword);
        encrypted.Decrypt(StrongPassword).Should().Be("segredo");
    }

    [Fact]
    public void Decrypt_ToleraEspacosNoLugarDeMais()
    {
        // O Decrypt normaliza ' ' para '+' (caso comum em querystrings)
        var encrypted = CryptoString.Encrypt("texto qualquer", StrongPassword);
        var comEspacos = encrypted.Replace('+', ' ');
        CryptoString.Decrypt(comEspacos, StrongPassword).Should().Be("texto qualquer");
    }

    [Fact]
    public void SecureBuffer_AposDispose_LancaObjectDisposedException()
    {
        var buffer = SecureMemory.Allocate<byte>(16);
        buffer.Dispose();
        var act = () => { _ = buffer.Span.Length; };
        act.Should().Throw<ObjectDisposedException>();
    }
}
