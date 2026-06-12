using Codout.Framework.Common.Extensions;
using FluentAssertions;
using Xunit;

namespace Codout.Framework.Common.Tests.Extensions;

public class ObjectExtensionsTests
{
    private class Pessoa
    {
        public string? Nome { get; set; }
        public int Idade { get; set; }
    }

    [Fact]
    public void ChangeTypeTo_ConverteTiposSimples()
    {
        "42".ChangeTypeTo<int>().Should().Be(42);
        "true".ChangeTypeTo<bool>().Should().Be(true);
    }

    [Fact]
    public void ChangeTypeTo_ConverteParaNullable()
    {
        "42".ChangeTypeTo<int?>().Should().Be(42);
        ((object?)null!).ChangeTypeTo<int?>().Should().BeNull();
    }

    [Fact]
    public void ChangeTypeTo_ConverteParaGuid()
    {
        var guid = Guid.NewGuid();
        guid.ToString().ChangeTypeTo<Guid>().Should().Be(guid);
    }

    [Fact]
    public void ChangeTypeTo_IntParaLong_LancaExcecao()
    {
        // Comportamento intencional documentado no código (caso SQLite/PK Int64)
        var act = () => 42.ChangeTypeTo<long>();
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void ChangeTypeTo_TipoNulo_LancaArgumentNullException()
    {
        var act = () => "x".ChangeTypeTo(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void ToDictionary_ConvertePropriedadesPublicas()
    {
        var dict = new Pessoa { Nome = "Ana", Idade = 30 }.ToDictionary();
        dict.Should().Contain("Nome", "Ana");
        dict.Should().Contain("Idade", 30);
    }

    [Fact]
    public void CopyTo_CopiaPropriedadesEntreObjetos()
    {
        var origem = new Pessoa { Nome = "Ana", Idade = 30 };
        var destino = new Pessoa();

        var resultado = origem.CopyTo(destino);

        resultado.Nome.Should().Be("Ana");
        resultado.Idade.Should().Be(30);
    }

    [Fact]
    public void FromDictionary_PreencheObjeto()
    {
        var dict = new Dictionary<string, object> { ["Nome"] = "Bia", ["Idade"] = 25 };
        var pessoa = dict.FromDictionary(new Pessoa());

        pessoa.Nome.Should().Be("Bia");
        pessoa.Idade.Should().Be(25);
    }
}
