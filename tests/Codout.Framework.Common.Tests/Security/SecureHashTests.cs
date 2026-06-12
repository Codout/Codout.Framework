using System.Security.Cryptography;
using System.Text;
using Codout.Framework.Common.Security;
using FluentAssertions;
using Xunit;

namespace Codout.Framework.Common.Tests.Security;

public class SecureHashTests
{
    [Theory]
    [InlineData(SecureHashAlgorithm.Sha256)]
    [InlineData(SecureHashAlgorithm.Sha384)]
    [InlineData(SecureHashAlgorithm.Sha512)]
    public void ComputeHash_VerifyHash_RoundtripValido(SecureHashAlgorithm algorithm)
    {
        const string texto = "senha-super-secreta";

        var hash = SecureHash.ComputeHash(texto, algorithm);

        SecureHash.VerifyHash(texto, hash, algorithm).Should().BeTrue();
        SecureHash.VerifyHash("senha-errada", hash, algorithm).Should().BeFalse();
    }

    [Fact]
    public void ComputeHash_MesmoTexto_GeraHashesDiferentes()
    {
        // Salt aleatório a cada chamada
        var h1 = SecureHash.ComputeHash("texto");
        var h2 = SecureHash.ComputeHash("texto");
        h1.Should().NotBe(h2);
    }

    [Fact]
    public void ComputeHash_RetornaBase64Valido()
    {
        var hash = SecureHash.ComputeHash("texto");
        var act = () => Convert.FromBase64String(hash);
        act.Should().NotThrow();
    }

    [Fact]
    public void ComputeHash_TextoVazio_LancaArgumentException()
    {
        var act = () => SecureHash.ComputeHash("");
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void ComputeHash_SaltMuitoPequeno_LancaArgumentException()
    {
        var salt = new byte[8]; // mínimo é 16
        var act = () => SecureHash.ComputeHash("texto", SecureHashAlgorithm.Sha256, salt);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void ComputeHash_ComSaltCustomizado_EhVerificavel()
    {
        var salt = new byte[32];
        RandomNumberGenerator.Fill(salt);

        var hash = SecureHash.ComputeHash("texto", SecureHashAlgorithm.Sha256, salt);
        SecureHash.VerifyHash("texto", hash).Should().BeTrue();
    }

    [Fact]
    public void ComputeHash_OpcoesInvalidas_LancaArgumentException()
    {
        var act = () => SecureHash.ComputeHash("texto", SecureHashAlgorithm.Sha256,
            new HashOptions { SaltSize = 8 });
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void VerifyHash_HashInvalido_RetornaFalse()
    {
        SecureHash.VerifyHash("texto", "isto não é base64!!!").Should().BeFalse();
        SecureHash.VerifyHash("texto", Convert.ToBase64String(new byte[4])).Should().BeFalse();
    }

    [Fact]
    public async Task ComputeStreamHashAsync_RetornaSha256Conhecido()
    {
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes("abc"));
        var hash = await SecureHash.ComputeStreamHashAsync(stream);
        hash.Should().Be("ba7816bf8f01cfea414140de5dae2223b00361a396177a9cb410ff61f20015ad");
    }

    [Fact]
    public async Task ComputeFileHashAsync_ArquivoInexistente_LancaFileNotFound()
    {
        var act = () => SecureHash.ComputeFileHashAsync("/caminho/inexistente.bin");
        await act.Should().ThrowAsync<FileNotFoundException>();
    }

    [Fact]
    public async Task VerifyFileIntegrityAsync_ArquivoIntegro_RetornaTrue()
    {
        var path = Path.Combine(Path.GetTempPath(), $"securehash-{Guid.NewGuid():N}.txt");
        try
        {
            await File.WriteAllTextAsync(path, "conteúdo de teste");
            var hash = await SecureHash.ComputeFileHashAsync(path);

            (await SecureHash.VerifyFileIntegrityAsync(path, hash)).Should().BeTrue();
            (await SecureHash.VerifyFileIntegrityAsync(path, new string('0', 64))).Should().BeFalse();
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void ExtensionMethods_ToSecureHashEVerifyAgainstHash_Funcionam()
    {
        const string texto = "minha-senha";
        var hash = texto.ToSecureHash();
        texto.VerifyAgainstHash(hash).Should().BeTrue();
        "outra".VerifyAgainstHash(hash).Should().BeFalse();
    }

    [Fact]
    public void GetSupportedAlgorithms_IncluiSha256()
    {
        SecureHash.GetSupportedAlgorithms().Should().Contain(SecureHashAlgorithm.Sha256);
    }
}
