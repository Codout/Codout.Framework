using Codout.Security.Core;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Codout.Security.Tests;

public class CoreTests
{
    [Fact]
    public void ImprovedPasswordHasherOptions_PadraoEhSensitive()
    {
        new ImprovedPasswordHasherOptions().Strength.Should().Be(PasswordHasherStrength.Sensitive);
    }

    [Fact]
    public void PasswordHasherBuilder_ServicesNulo_LancaArgumentNullException()
    {
        var act = () => new PasswordHasherBuilder(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void PasswordHasherBuilder_ExpoeServicesRecebido()
    {
        var services = new ServiceCollection();
        var builder = new PasswordHasherBuilder(services);
        builder.Services.Should().BeSameAs(services);
    }

    [Fact]
    public void WithStrength_AlteraOptionsERetornaMesmoBuilder()
    {
        var builder = new PasswordHasherBuilder(new ServiceCollection());

        var result = builder.WithStrength(PasswordHasherStrength.Interactive);

        result.Should().BeSameAs(builder);
        builder.Options.Strength.Should().Be(PasswordHasherStrength.Interactive);
    }

    [Fact]
    public void UseCustomHashPasswordBuilder_RetornaBuilderConfigurado()
    {
        var services = new ServiceCollection();
        var builder = services.UseCustomHashPasswordBuilder();

        builder.Should().BeOfType<PasswordHasherBuilder>();
        builder.Services.Should().BeSameAs(services);
    }

    [Fact]
    public void UpgradePasswordSecurity_RetornaBuilder()
    {
        var services = new ServiceCollection();
        services.UpgradePasswordSecurity().Should().NotBeNull();
    }

    [Fact]
    public void PasswordVerificationResult_PossuiOsTresEstados()
    {
        Enum.GetValues<PasswordVerificationResult>().Should().BeEquivalentTo(new[]
        {
            PasswordVerificationResult.Failed,
            PasswordVerificationResult.Success,
            PasswordVerificationResult.SuccessRehashNeeded
        });
    }
}
