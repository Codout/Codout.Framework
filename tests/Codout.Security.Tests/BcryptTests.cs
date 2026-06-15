using Codout.Security.Bcrypt;
using Codout.Security.Core;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;

namespace Codout.Security.Tests;

public class BcryptTests
{
    // WorkFactor mínimo (4) para manter os testes rápidos.
    private static BcryptPasswordHash CreateFastHasher(
        int workFactor = 4,
        BcryptSaltRevision revision = BcryptSaltRevision.Revision2B) =>
        new(Options.Create(new BcryptOptions { WorkFactor = workFactor, SaltRevision = revision }));

    [Fact]
    public void HashPassword_GeraFormatoBcryptComWorkFactor()
    {
        var hash = CreateFastHasher().HashPassword("minha-senha");
        hash.Should().StartWith("$2b$04$");
    }

    [Fact]
    public void HashPassword_ComRevision2Y_GeraPrefixoCorrespondente()
    {
        var hash = CreateFastHasher(revision: BcryptSaltRevision.Revision2Y).HashPassword("senha");
        hash.Should().StartWith("$2y$04$");
    }

    [Fact]
    public void HashPassword_MesmaSenha_GeraHashesDiferentes()
    {
        var hasher = CreateFastHasher();
        hasher.HashPassword("senha").Should().NotBe(hasher.HashPassword("senha"));
    }

    [Fact]
    public void VerifyHashedPassword_SenhaCorreta_RetornaSuccess()
    {
        var hasher = CreateFastHasher();
        var hash = hasher.HashPassword("senha-correta");

        hasher.VerifyHashedPassword(hash, "senha-correta")
            .Should().Be(PasswordVerificationResult.Success);
    }

    [Fact]
    public void VerifyHashedPassword_SenhaErrada_RetornaFailed()
    {
        var hasher = CreateFastHasher();
        var hash = hasher.HashPassword("senha-correta");

        hasher.VerifyHashedPassword(hash, "senha-errada")
            .Should().Be(PasswordVerificationResult.Failed);
    }

    [Fact]
    public void VerifyHashedPassword_WorkFactorMenorQueConfigurado_RetornaSuccessRehashNeeded()
    {
        var weakHash = CreateFastHasher(workFactor: 4).HashPassword("senha");
        var strongHasher = CreateFastHasher(workFactor: 6);

        strongHasher.VerifyHashedPassword(weakHash, "senha")
            .Should().Be(PasswordVerificationResult.SuccessRehashNeeded);
    }

    [Fact]
    public void HashPassword_SenhaNulaOuVazia_LancaArgumentNullException()
    {
        var hasher = CreateFastHasher();
        var actNull = () => hasher.HashPassword(null!);
        var actEmpty = () => hasher.HashPassword("");
        actNull.Should().Throw<ArgumentNullException>();
        actEmpty.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void VerifyHashedPassword_ArgumentosNulos_LancamArgumentNullException()
    {
        var hasher = CreateFastHasher();
        var actHash = () => hasher.VerifyHashedPassword(null!, "x");
        var actPassword = () => hasher.VerifyHashedPassword("hash", null!);
        actHash.Should().Throw<ArgumentNullException>();
        actPassword.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Construtor_SemOptions_UsaPadroes()
    {
        // BcryptOptions padrão: WorkFactor 12, Revision2B
        var hash = new BcryptPasswordHash().HashPassword("senha");
        hash.Should().StartWith("$2b$12$");
    }

    [Fact]
    public void UseBcrypt_RegistraIPasswordHasherNoContainer()
    {
        var services = new ServiceCollection();
        services.UpgradePasswordSecurity()
            .UseBcrypt(o => o.WorkFactor = 4);

        using var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();

        var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
        hasher.Should().BeOfType<BcryptPasswordHash>();

        var hash = hasher.HashPassword("senha-via-di");
        hash.Should().StartWith("$2b$04$");
        hasher.VerifyHashedPassword(hash, "senha-via-di")
            .Should().Be(PasswordVerificationResult.Success);
    }
}
