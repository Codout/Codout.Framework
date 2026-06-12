using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Codout.Framework.Common.Helpers;
using FluentAssertions;
using Xunit;

namespace Codout.Framework.Common.Tests.Helpers;

public class EnumHelperTests
{
    private enum Status
    {
        [Description("Em andamento")]
        EmAndamento,

        [Description("Concluído")]
        Concluido,

        SemDescricao
    }

    private enum Idioma
    {
        // GetLocalizedName lê DisplayAttribute.GetDescription(), então é a
        // propriedade Description (e não Name) que precisa estar preenchida.
        [Display(Description = "Português")]
        Portugues,

        Ingles
    }

    [Fact]
    public void GetDescription_ComAtributo_RetornaDescricao()
    {
        Status.EmAndamento.GetDescription().Should().Be("Em andamento");
        Status.Concluido.GetDescription().Should().Be("Concluído");
    }

    [Fact]
    public void GetDescription_SemAtributo_RetornaNome()
    {
        Status.SemDescricao.GetDescription().Should().Be("SemDescricao");
    }

    [Fact]
    public void GetDescription_PorTipoENome_RetornaDescricao()
    {
        EnumHelper.GetDescription(typeof(Status), nameof(Status.Concluido)).Should().Be("Concluído");
    }

    [Fact]
    public void GetValueFromDescription_RetornaValorDoEnum()
    {
        EnumHelper.GetValueFromDescription<Status>("Em andamento").Should().Be(Status.EmAndamento);
        EnumHelper.GetValueFromDescription<Status>("SemDescricao").Should().Be(Status.SemDescricao);
    }

    [Fact]
    public void GetValueFromDescription_DescricaoInexistente_RetornaDefault()
    {
        EnumHelper.GetValueFromDescription<Status>("Não existe").Should().Be(default(Status));
    }

    [Fact]
    public void GetValueFromDescription_TipoNaoEnum_LancaInvalidOperationException()
    {
        var act = () => EnumHelper.GetValueFromDescription<int>("x");
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void GetLocalizedName_ComDisplayAttribute_RetornaNome()
    {
        Idioma.Portugues.GetLocalizedName().Should().Be("Português");
        Idioma.Ingles.GetLocalizedName().Should().Be("Ingles");
    }

    [Fact]
    public void GetDicionary_RetornaParesValorDescricao()
    {
        var entries = EnumHelper.GetDicionary(typeof(Status));
        entries.Should().HaveCount(3);
        entries.Select(e => e.Value).Should().Contain("Em andamento");
    }
}
