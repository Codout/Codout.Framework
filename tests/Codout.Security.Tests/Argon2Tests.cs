using Codout.Security.Argon2;
using Codout.Security.Core;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;

namespace Codout.Security.Tests;

public class Argon2Tests
{
    // Strength.Interactive (64 MiB / t=2) para manter os testes rápidos.
    private static ArgonPasswordHash CreateInteractiveHasher() =>
        new(Options.Create(new ImprovedPasswordHasherOptions
        {
            Strength = PasswordHasherStrength.Interactive
        }));

    [Fact]
    public void HashPassword_GeraFormatoArgon2id()
    {
        var hash = CreateInteractiveHasher().HashPassword("minha-senha");
        hash.Should().StartWith("$argon2id$");
        hash.Should().NotContain("\0");
    }

    [Fact]
    public void HashPassword_MesmaSenha_GeraHashesDiferentes()
    {
        var hasher = CreateInteractiveHasher();
        hasher.HashPassword("senha").Should().NotBe(hasher.HashPassword("senha"));
    }

    [Fact]
    public void VerifyHashedPassword_SenhaCorreta_ComLimitesCustomizados_RetornaSuccess()
    {
        // Com OpsLimit/MemLimit explícitos os parâmetros gerados e os esperados
        // coincidem, então o roundtrip retorna Success.
        var hasher = new ArgonPasswordHash(
            argon2OptionsAccessor: Options.Create(new Argon2Options
            {
                OpsLimit = 3,
                MemLimit = 32 * 1024 * 1024
            }));

        var hash = hasher.HashPassword("senha-correta");

        hasher.VerifyHashedPassword(hash, "senha-correta")
            .Should().Be(PasswordVerificationResult.Success);
    }

    [Fact]
    public void VerifyHashedPassword_SenhaCorreta_ComStrength_ComportamentoAtual()
    {
        // BUG?: os parâmetros esperados em ArgonPasswordHash.GetExpectedParameters
        // (Interactive: t=2/m=65536KiB) não batem com os que o Sodium.Core realmente
        // usa (Interactive: t=4/m=32768KiB). Como m armazenado (32768) < m esperado
        // (65536), TODO hash gerado via Strength se auto-reporta como
        // SuccessRehashNeeded — nunca Success. O mesmo ocorre com Moderate
        // (real t=6/m=131072 vs esperado t=3/m=262144).
        var hasher = CreateInteractiveHasher();
        var hash = hasher.HashPassword("senha-correta");

        hash.Should().Contain("m=32768,t=4");
        hasher.VerifyHashedPassword(hash, "senha-correta")
            .Should().Be(PasswordVerificationResult.SuccessRehashNeeded);
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
        // Hash gerado com Interactive (t=2, m=64MiB); verificador configurado
        // com Moderate espera t=3 / m=256MiB → deve sinalizar rehash.
        var weakHash = CreateInteractiveHasher().HashPassword("senha");

        var strongHasher = new ArgonPasswordHash(Options.Create(new ImprovedPasswordHasherOptions
        {
            Strength = PasswordHasherStrength.Moderate
        }));

        strongHasher.VerifyHashedPassword(weakHash, "senha")
            .Should().Be(PasswordVerificationResult.SuccessRehashNeeded);
    }

    [Fact]
    public void HashPassword_ComOpsEMemLimitCustomizados_UsaParametrosInformados()
    {
        var hasher = new ArgonPasswordHash(
            argon2OptionsAccessor: Options.Create(new Argon2Options
            {
                OpsLimit = 3,
                MemLimit = 32 * 1024 * 1024 // 32 MiB
            }));

        var hash = hasher.HashPassword("senha");

        hash.Should().Contain("m=32768").And.Contain("t=3");
        hasher.VerifyHashedPassword(hash, "senha")
            .Should().Be(PasswordVerificationResult.Success);
    }

    [Fact]
    public void HashPassword_SenhaNula_LancaArgumentNullException()
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
    public void UseArgon2_RegistraIPasswordHasherNoContainer()
    {
        var services = new ServiceCollection();
        services.UpgradePasswordSecurity()
            .WithStrength(PasswordHasherStrength.Interactive)
            .UseArgon2(o =>
            {
                // Limites explícitos para um roundtrip Success (ver teste
                // VerifyHashedPassword_SenhaCorreta_ComStrength_ComportamentoAtual)
                o.OpsLimit = 3;
                o.MemLimit = 32 * 1024 * 1024;
            });

        using var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();

        var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
        hasher.Should().BeOfType<ArgonPasswordHash>();

        var options = scope.ServiceProvider
            .GetRequiredService<IOptions<ImprovedPasswordHasherOptions>>();
        options.Value.Strength.Should().Be(PasswordHasherStrength.Interactive);

        var hash = hasher.HashPassword("senha-via-di");
        hasher.VerifyHashedPassword(hash, "senha-via-di")
            .Should().Be(PasswordVerificationResult.Success);
    }

    [Fact]
    public void UseArgon2_ComConfigure_AplicaOpcoes()
    {
        var services = new ServiceCollection();
        services.UpgradePasswordSecurity()
            .UseArgon2(o =>
            {
                o.OpsLimit = 3;
                o.MemLimit = 32 * 1024 * 1024;
            });

        using var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();

        var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
        var hash = hasher.HashPassword("senha");
        hash.Should().Contain("m=32768").And.Contain("t=3");
    }
}
