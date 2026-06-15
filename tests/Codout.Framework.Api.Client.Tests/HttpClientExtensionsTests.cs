using System.Net;
using Codout.Framework.Api.Client.Extensions;
using FluentAssertions;
using Xunit;

namespace Codout.Framework.Api.Client.Tests;

public class PersonDto : EntityDto<int>
{
    public string? Name { get; set; }

    public string? Notes { get; set; }
}

public class HttpClientExtensionsTests
{
    private readonly FakeHttpMessageHandler _handler = new();

    [Fact]
    public async Task GetAsync_DeveUsarVerboGetEDeserializarResposta()
    {
        _handler.Enqueue(HttpStatusCode.OK, "{\"id\":7,\"name\":\"Ana\"}");
        var client = _handler.CreateClient();

        var dto = await client.GetAsync<PersonDto>("people/7");

        _handler.Requests.Should().ContainSingle();
        _handler.Requests[0].Method.Should().Be(HttpMethod.Get);
        _handler.Requests[0].Uri!.ToString().Should().Be("https://api.example.test/people/7");
        dto.Id.Should().Be(7);
        dto.Name.Should().Be("Ana");
    }

    [Fact]
    public async Task PostAsync_DeveSerializarCorpoEmCamelCase()
    {
        _handler.Enqueue(HttpStatusCode.OK, "{\"id\":1,\"name\":\"Ana\"}");
        var client = _handler.CreateClient();

        var result = await client.PostAsync<PersonDto, PersonDto>("people", new PersonDto { Id = 1, Name = "Ana" });

        var request = _handler.Requests.Single();
        request.Method.Should().Be(HttpMethod.Post);
        request.ContentType.Should().Be("application/json");
        request.Body.Should().Be("{\"name\":\"Ana\",\"id\":1}");
        result.Id.Should().Be(1);
    }

    [Fact]
    public async Task PostAsync_NaoSerializaPropriedadesNulas()
    {
        _handler.Enqueue(HttpStatusCode.OK, "{}");
        var client = _handler.CreateClient();

        await client.PostAsync<PersonDto, PersonDto>("people", new PersonDto { Id = 2, Name = "Bia", Notes = null });

        _handler.Requests.Single().Body.Should().NotContain("notes");
    }

    [Fact]
    public async Task PostAsync_SemRetorno_DeveEnviarCorpo()
    {
        _handler.Enqueue(HttpStatusCode.NoContent);
        var client = _handler.CreateClient();

        await client.PostAsync("people", new PersonDto { Id = 3 });

        _handler.Requests.Single().Body.Should().Contain("\"id\":3");
    }

    [Fact]
    public async Task PostAsync_SemCorpo_DeveEnviarPostVazio()
    {
        _handler.Enqueue(HttpStatusCode.OK);
        var client = _handler.CreateClient();

        await client.PostAsync("people/refresh");

        var request = _handler.Requests.Single();
        request.Method.Should().Be(HttpMethod.Post);
        request.Body.Should().BeNull();
    }

    [Fact]
    public async Task PutAsync_DeveUsarVerboPutERetornarObjeto()
    {
        _handler.Enqueue(HttpStatusCode.OK, "{\"id\":4,\"name\":\"Caio\"}");
        var client = _handler.CreateClient();

        var dto = await client.PutAsync<PersonDto, PersonDto>("people/4", new PersonDto { Id = 4, Name = "Caio" });

        _handler.Requests.Single().Method.Should().Be(HttpMethod.Put);
        dto.Name.Should().Be("Caio");
    }

    [Fact]
    public async Task DeleteAsync_DeveUsarVerboDelete()
    {
        // Chamada estática proposital: `client.DeleteAsync(...)` resolveria para o
        // método de INSTÂNCIA HttpClient.DeleteAsync, não para a extensão.
        _handler.Enqueue(HttpStatusCode.OK);
        var client = _handler.CreateClient();

        await HttpClientExtensions.DeleteAsync(client, "people/5");

        var request = _handler.Requests.Single();
        request.Method.Should().Be(HttpMethod.Delete);
        request.Uri!.ToString().Should().EndWith("people/5");
    }

    [Fact]
    public async Task RespostaDeDeserializacaoCaseInsensitive_DeveFuncionar()
    {
        _handler.Enqueue(HttpStatusCode.OK, "{\"ID\":9,\"NAME\":\"Eva\"}");
        var client = _handler.CreateClient();

        var dto = await client.GetAsync<PersonDto>("people/9");

        dto.Id.Should().Be(9);
        dto.Name.Should().Be("Eva");
    }

    [Fact]
    public async Task RespostaDeErro_ComCorpoDeApiException_LancaExceptionGenericaComOCorpo()
    {
        // BUG?: GetExceptionAsync lança ApiClientException DENTRO do próprio try e o
        // catch genérico a engole, relançando `new Exception(corpo)`. Ou seja, o
        // ApiClientException (tipado) nunca chega ao chamador — sempre vem Exception
        // com o corpo bruto da resposta.
        var errorBody = "{\"statusCode\":500,\"message\":\"falhou\",\"errors\":[]}";
        _handler.Enqueue(HttpStatusCode.InternalServerError, errorBody);
        var client = _handler.CreateClient();

        var act = () => client.GetAsync<PersonDto>("people/1");

        var ex = await act.Should().ThrowAsync<Exception>();
        ex.Which.Should().NotBeOfType<ApiClientException>();
        ex.Which.Message.Should().Be(errorBody);
    }

    [Fact]
    public async Task RespostaDeErro_ComCorpoNaoJson_LancaExceptionComOCorpo()
    {
        _handler.Enqueue(HttpStatusCode.BadRequest, "erro interno em texto puro", "text/plain");
        var client = _handler.CreateClient();

        var act = () => HttpClientExtensions.DeleteAsync(client, "people/1");

        (await act.Should().ThrowAsync<Exception>())
            .WithMessage("erro interno em texto puro");
    }

    [Fact]
    public async Task RespostaDeErro_SemResposta_PropagaExcecaoOriginal()
    {
        using var handler = new ThrowingHandler();
        var client = new HttpClient(handler) { BaseAddress = new Uri("https://api.example.test/") };

        var act = () => client.GetAsync<PersonDto>("people/1");

        await act.Should().ThrowAsync<HttpRequestException>();
    }

    private sealed class ThrowingHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            throw new HttpRequestException("rede indisponível");
        }
    }
}

public class JsonExtensionsTests
{
    [Fact]
    public async Task ReadAsAsync_DeserializaCamelCaseECaseInsensitive()
    {
        var content = new StringContent("{\"id\":3,\"Name\":\"Zoe\"}");

        var dto = await content.ReadAsAsync<PersonDto>();

        dto.Id.Should().Be(3);
        dto.Name.Should().Be("Zoe");
    }

    [Fact]
    public async Task PostAsJsonAsync_DefineContentTypeApplicationJson()
    {
        var handler = new FakeHttpMessageHandler().Enqueue(System.Net.HttpStatusCode.OK);
        var client = handler.CreateClient();

        await client.PostAsJsonAsync("any", new PersonDto { Id = 1 });

        handler.Requests.Single().ContentType.Should().Be("application/json");
    }

    [Fact]
    public async Task PutAsJsonAsync_SerializaEmCamelCase()
    {
        var handler = new FakeHttpMessageHandler().Enqueue(System.Net.HttpStatusCode.OK);
        var client = handler.CreateClient();

        await client.PutAsJsonAsync("any", new PersonDto { Id = 2, Name = "Lia" });

        handler.Requests.Single().Body.Should().Be("{\"name\":\"Lia\",\"id\":2}");
    }
}
