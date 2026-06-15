using Codout.Security.Core;
using Codout.Security.Scrypt;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;

namespace Codout.Security.Tests;

public class ScryptTests
{
    // Strength.Interactive (16 MiB) para manter os testes rápidos.
    private static ScryptPasswordHash CreateInteractiveHasher() =>
        new(Options.Create(new ImprovedPasswordHasherOptions
        {
            Strength = PasswordHasherStrength.Interactive
        }));

    [Fact]
    public void HashPassword_GeraFormatoScrypt()
    {
        var hash = CreateInteractiveHasher().HashPassword("minha-senha");
        hash.Should().StartWith("$7$");
    }

    [Fact]
    public void HashPassword_MesmaSenha_GeraHashesDiferentes()
    {
        var hasher = CreateInteractiveHasher();
        hasher.HashPassword("senha").Should().NotBe(hasher.HashPassword("senha"));
    }

    [Fact]
    public void VerifyHashedPassword_SenhaCorreta_RetornaSuccess()
    {
        var hasher = CreateInteractiveHasher();
        var hash = hasher.HashPassword("senha-correta");

        hasher.VerifyHashedPassword(hash, "senha-correta")
            .Should().Be(PasswordVerificationResult.Success);
    }

    [Fact]
    public void VerifyHashedPassword_SenhaErrada_RetornaFailed()
    {
        var hasher = CreateInteractiveHasher();
        var hash = hasher.HashPassword("senha-correta");

        hasher.VerifyHashedPassword(hash, "senha-errada")
            .Should().Be(PasswordVerificationResult.Failed);
    }

    [Fact]
    public void VerifyHashedPassword_HashFraco_RetornaSuccessRehashNeeded()
    {
        // Hash Interactive (N=2^14); verificador Sensitive espera N=2^20.
        var weakHash = CreateInteractiveHasher().HashPassword("senha");

        var strongHasher = new ScryptPasswordHash(Options.Create(new ImprovedPasswordHasherOptions
        {
            Strength = PasswordHasherStrength.Sensitive
        }));

        strongHasher.VerifyHashedPassword(weakHash, "senha")
            .Should().Be(PasswordVerificationResult.SuccessRehashNeeded);
    }

    [Fact]
    public void HashPassword_SenhaNulaOuVazia_LancaArgumentNullException()
    {
        var hasher = CreateInteractiveHasher();
        var actNull = () => hasher.HashPassword(null!);
        var actEmpty = () => hasher.HashPassword("");
        actNull.Should().Throw<ArgumentNullException>();
        actEmpty.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void VerifyHashedPassword_ArgumentosNulos_LancamArgumentNullException()
    {
        var hasher = CreateInteractiveHasher();
        var actHash = () => hasher.VerifyHashedPassword(null!, "x");
        var actPassword = () => hasher.VerifyHashedPassword("hash", null!);
        actHash.Should().Throw<ArgumentNullException>();
        actPassword.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void UseScrypt_RegistraIPasswordHasherNoContainer()
    {
        var services = new ServiceCollection();
        services.UpgradePasswordSecurity()
            .WithStrength(PasswordHasherStrength.Interactive)
            .UseScrypt();

        using var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();

        var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
        hasher.Should().BeOfType<ScryptPasswordHash>();

        var hash = hasher.HashPassword("senha-via-di");
        hasher.VerifyHashedPassword(hash, "senha-via-di")
            .Should().Be(PasswordVerificationResult.Success);
    }
}
