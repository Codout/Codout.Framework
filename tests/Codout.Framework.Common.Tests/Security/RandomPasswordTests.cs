using Codout.Framework.Common.Security;
using FluentAssertions;
using Xunit;

namespace Codout.Framework.Common.Tests.Security;

public class RandomPasswordTests
{
    private const string AllowedChars =
        "abcdefgijkmnopqrstwxyz" +
        "ABCDEFGHJKLMNPQRSTWXYZ" +
        "23456789" +
        "*$-+?_&=!%{}/";

    [Fact]
    public void Generate_SemParametros_RespeitaTamanhoPadrao()
    {
        for (var i = 0; i < 20; i++)
        {
            var password = RandomPassword.Generate();
            password.Length.Should().BeInRange(8, 10);
        }
    }

    [Fact]
    public void Generate_ComTamanhoExato_RetornaTamanhoSolicitado()
    {
        RandomPassword.Generate(12).Should().HaveLength(12);
        RandomPassword.Generate(4).Should().HaveLength(4);
    }

    [Fact]
    public void Generate_ComIntervalo_RespeitaLimites()
    {
        for (var i = 0; i < 20; i++)
        {
            var password = RandomPassword.Generate(6, 9);
            password.Length.Should().BeInRange(6, 9);
        }
    }

    [Fact]
    public void Generate_UsaSomenteCaracteresPermitidos()
    {
        var password = RandomPassword.Generate(50);
        password.Should().NotBeNull();
        foreach (var c in password!)
            AllowedChars.Should().Contain(c.ToString());
    }

    [Theory]
    [InlineData(0, 5)]
    [InlineData(5, 0)]
    [InlineData(-1, 5)]
    [InlineData(9, 5)] // min > max
    public void Generate_ParametrosInvalidos_RetornaNull(int min, int max)
    {
        RandomPassword.Generate(min, max).Should().BeNull();
    }

    [Fact]
    public void Generate_GeraSenhasDiferentes()
    {
        var p1 = RandomPassword.Generate(20);
        var p2 = RandomPassword.Generate(20);
        p1.Should().NotBe(p2);
    }
}
