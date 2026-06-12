using System.Text.Json;
using Codout.Framework.Api.Client;
using FluentAssertions;
using Xunit;

namespace Codout.Framework.Api.Dto.Tests;

public class DtoContractTests
{
    [Fact]
    public void Dto_DeveImplementarIDto()
    {
        new Client.Dto().Should().BeAssignableTo<IDto>();
    }

    [Fact]
    public void EntityDto_DeveImplementarIEntityDto()
    {
        new EntityDto<int>().Should().BeAssignableTo<IEntityDto<int>>();
    }

    [Fact]
    public void EntityDto_Int_IdPadraoEhZero()
    {
        new EntityDto<int>().Id.Should().Be(0);
    }

    [Fact]
    public void EntityDto_Guid_IdPadraoEhGuidVazio()
    {
        new EntityDto<Guid>().Id.Should().Be(Guid.Empty);
    }

    [Fact]
    public void EntityDto_String_IdPadraoEhNulo()
    {
        new EntityDto<string>().Id.Should().BeNull();
    }

    [Fact]
    public void EntityDto_Id_DevePermitirLeituraEEscrita()
    {
        var id = Guid.NewGuid();
        var dto = new EntityDto<Guid> { Id = id };

        dto.Id.Should().Be(id);
    }

    [Fact]
    public void EntityDto_AcessadoViaInterface_RefleteOMesmoId()
    {
        IEntityDto<int> dto = new EntityDto<int>();
        dto.Id = 42;

        ((EntityDto<int>)dto).Id.Should().Be(42);
    }

    [Fact]
    public void TiposDoPacote_EstaoNoNamespaceCodoutFrameworkApiClient()
    {
        // Observação: o pacote chama-se Codout.Framework.Api.Dto, mas os tipos do
        // shared project Codout.Framework.Dto.Shared são declarados no namespace
        // Codout.Framework.Api.Client (compartilhado com o pacote do client).
        typeof(EntityDto<>).Namespace.Should().Be("Codout.Framework.Api.Client");
        typeof(Client.Dto).Namespace.Should().Be("Codout.Framework.Api.Client");
        typeof(IDto).Namespace.Should().Be("Codout.Framework.Api.Client");
        typeof(IEntityDto<>).Namespace.Should().Be("Codout.Framework.Api.Client");
    }
}

public class DtoSerializationTests
{
    private static readonly JsonSerializerOptions Web = JsonSerializerOptions.Web;

    private class ClienteDto : EntityDto<Guid>
    {
        public string? Nome { get; set; }
        public int Pontos { get; set; }
    }

    [Fact]
    public void EntityDto_RoundtripJson_PreservaId()
    {
        var original = new EntityDto<int> { Id = 123 };

        var json = JsonSerializer.Serialize(original, Web);
        var restored = JsonSerializer.Deserialize<EntityDto<int>>(json, Web);

        restored!.Id.Should().Be(123);
    }

    [Fact]
    public void EntityDto_SerializadoComOpcoesWeb_UsaCamelCase()
    {
        var json = JsonSerializer.Serialize(new EntityDto<int> { Id = 7 }, Web);

        json.Should().Be("{\"id\":7}");
    }

    [Fact]
    public void EntityDto_Guid_RoundtripJson_PreservaId()
    {
        var id = Guid.NewGuid();
        var json = JsonSerializer.Serialize(new EntityDto<Guid> { Id = id }, Web);

        var restored = JsonSerializer.Deserialize<EntityDto<Guid>>(json, Web);

        restored!.Id.Should().Be(id);
    }

    [Fact]
    public void DtoDerivado_RoundtripJson_PreservaTodasAsPropriedades()
    {
        var original = new ClienteDto { Id = Guid.NewGuid(), Nome = "Maria", Pontos = 10 };

        var json = JsonSerializer.Serialize(original, Web);
        var restored = JsonSerializer.Deserialize<ClienteDto>(json, Web);

        restored.Should().BeEquivalentTo(original);
    }

    [Fact]
    public void DtoDerivado_DeserializaJsonCamelCase()
    {
        var json = "{\"id\":\"7e1f9a8a-9d24-4a6c-8e6e-2a76a3a1b001\",\"nome\":\"João\",\"pontos\":3}";

        var dto = JsonSerializer.Deserialize<ClienteDto>(json, Web);

        dto!.Id.Should().Be(Guid.Parse("7e1f9a8a-9d24-4a6c-8e6e-2a76a3a1b001"));
        dto.Nome.Should().Be("João");
        dto.Pontos.Should().Be(3);
    }

    [Fact]
    public void DtoDerivado_DeserializacaoDeJsonVazio_UsaDefaults()
    {
        var dto = JsonSerializer.Deserialize<ClienteDto>("{}", Web);

        dto!.Id.Should().Be(Guid.Empty);
        dto.Nome.Should().BeNull();
        dto.Pontos.Should().Be(0);
    }
}
