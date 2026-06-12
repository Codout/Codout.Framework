using System.Net;
using System.Text.Json;
using Codout.DynamicLinq;
using FluentAssertions;
using Xunit;

namespace Codout.Framework.Api.Client.Tests;

public class ApiClientBaseTests
{
    [Fact]
    public void Construtor_DeveConfigurarBaseAddressAcceptETimeout()
    {
        var client = new RestApiClient<PersonDto, int>("people", "https://api.example.test/");

        client.UriService.Should().Be("people");
        client.Client.BaseAddress.Should().Be(new Uri("https://api.example.test/"));
        client.Client.DefaultRequestHeaders.Accept.Should()
            .ContainSingle(h => h.MediaType == "application/json");
        client.Client.Timeout.Should().Be(TimeSpan.FromMinutes(1));
        client.Client.DefaultRequestHeaders.Contains("ApiKey").Should().BeFalse();
    }

    [Fact]
    public void Construtor_ComApiKey_DeveAdicionarHeader()
    {
        var client = new RestApiClient<PersonDto, int>("people", "https://api.example.test/", "chave-secreta");

        client.Client.DefaultRequestHeaders.GetValues("ApiKey").Should().ContainSingle("chave-secreta");
    }

    [Fact]
    public void Construtor_ComBaseUrlInvalida_LancaUriFormatException()
    {
        var act = () => new RestApiClient<PersonDto, int>("people", "not a url");

        act.Should().Throw<UriFormatException>();
    }
}

public class RestApiClientTests
{
    private readonly FakeHttpMessageHandler _handler = new();

    private RestApiClient<PersonDto, int> CreateClient()
    {
        var client = new RestApiClient<PersonDto, int>("people", "https://api.example.test/");
        // ApiClientBase cria o HttpClient internamente, sem ponto de injeção;
        // substituímos o handler via reflection para evitar rede.
        _handler.InjectInto(client.Client);
        return client;
    }

    [Fact]
    public async Task GetAsync_DeveChamarGetNaRotaComId()
    {
        _handler.Enqueue(HttpStatusCode.OK, "{\"id\":7,\"name\":\"Ana\"}");
        var client = CreateClient();

        var dto = await client.GetAsync(7);

        var request = _handler.Requests.Single();
        request.Method.Should().Be(HttpMethod.Get);
        request.Uri!.ToString().Should().Be("https://api.example.test/people/7");
        dto.Id.Should().Be(7);
    }

    [Fact]
    public async Task PostAsync_DeveChamarPostNaRotaBase()
    {
        _handler.Enqueue(HttpStatusCode.OK, "{\"id\":1,\"name\":\"Ana\"}");
        var client = CreateClient();

        var dto = await client.PostAsync(new PersonDto { Id = 1, Name = "Ana" });

        var request = _handler.Requests.Single();
        request.Method.Should().Be(HttpMethod.Post);
        request.Uri!.ToString().Should().Be("https://api.example.test/people");
        request.Body.Should().Contain("\"name\":\"Ana\"");
        dto.Name.Should().Be("Ana");
    }

    [Fact]
    public async Task PutAsync_DeveChamarPutNaRotaComIdDoObjeto()
    {
        _handler.Enqueue(HttpStatusCode.OK, "{\"id\":4,\"name\":\"Caio\"}");
        var client = CreateClient();

        await client.PutAsync(new PersonDto { Id = 4, Name = "Caio" });

        var request = _handler.Requests.Single();
        request.Method.Should().Be(HttpMethod.Put);
        request.Uri!.ToString().Should().Be("https://api.example.test/people/4");
    }

    [Fact]
    public async Task DeleteAsync_DeveChamarDeleteNaRotaComId()
    {
        _handler.Enqueue(HttpStatusCode.OK);
        var client = CreateClient();

        await client.DeleteAsync(5);

        var request = _handler.Requests.Single();
        request.Method.Should().Be(HttpMethod.Delete);
        request.Uri!.ToString().Should().Be("https://api.example.test/people/5");
    }

    [Fact]
    public async Task GetAllAsync_DevePostarORequestNaRotaGetAll()
    {
        _handler.Enqueue(HttpStatusCode.OK, "{\"total\":2,\"data\":[]}");
        var client = CreateClient();

        var result = await client.GetAllAsync(new DataSourceRequest { Take = 10, Skip = 0 });

        var request = _handler.Requests.Single();
        request.Method.Should().Be(HttpMethod.Post);
        request.Uri!.ToString().Should().Be("https://api.example.test/people/get-all");
        request.Body.Should().Contain("\"take\":10");
        result.Total.Should().Be(2);
    }

    [Fact]
    public async Task DeleteAsync_ComRespostaDeErro_NaoLancaExcecao()
    {
        // BUG?: dentro de RestApiClient, `Client.DeleteAsync(...)` resolve para o método
        // de INSTÂNCIA HttpClient.DeleteAsync (métodos de instância têm precedência sobre
        // extensões), então a extensão com EnsureSuccessStatusCode nunca é chamada e
        // qualquer erro HTTP do DELETE é silenciosamente ignorado.
        _handler.Enqueue(HttpStatusCode.InternalServerError, "falhou", "text/plain");
        var client = CreateClient();

        var act = () => client.DeleteAsync(5);

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task GetAsync_ComRespostaDeErro_LancaExcecaoComCorpo()
    {
        _handler.Enqueue(HttpStatusCode.NotFound, "nao encontrado", "text/plain");
        var client = CreateClient();

        var act = () => client.GetAsync(99);

        (await act.Should().ThrowAsync<Exception>()).WithMessage("nao encontrado");
    }
}

public class ApiExceptionTests
{
    [Fact]
    public void ApiException_NaoEhUmaException()
    {
        // Observação de caracterização: ApiException é um POCO (não herda de
        // System.Exception), portanto não pode ser lançada diretamente.
        typeof(Exception).IsAssignableFrom(typeof(ApiException)).Should().BeFalse();
    }

    [Fact]
    public void ApiException_DeveGuardarStatusMensagemEErros()
    {
        var error = new ApiErrorMessage(400, "campo obrigatório");
        var exception = new ApiException(400, "requisição inválida", error);

        exception.StatusCode.Should().Be(400);
        exception.Message.Should().Be("requisição inválida");
        exception.Errors.Should().ContainSingle().Which.Should().BeSameAs(error);
    }

    [Fact]
    public void ApiException_ToString_SerializaEmJsonCamelCase()
    {
        var exception = new ApiException(500, "falhou", new ApiErrorMessage(500, "detalhe"));

        var json = exception.ToString();
        using var document = JsonDocument.Parse(json);

        document.RootElement.GetProperty("statusCode").GetInt32().Should().Be(500);
        document.RootElement.GetProperty("message").GetString().Should().Be("falhou");
        document.RootElement.GetProperty("errors")[0].GetProperty("errorMessage").GetString()
            .Should().Be("detalhe");
    }

    [Fact]
    public void ApiErrorMessage_GuardaCodigoEMensagem()
    {
        var error = new ApiErrorMessage(404, "não achei");

        error.ErrorCode.Should().Be(404);
        error.ErrorMessage.Should().Be("não achei");
    }

    [Fact]
    public void ApiClientException_ExpoeApiExceptionEMensagem()
    {
        var apiException = new ApiException(500, "erro interno");

        var clientException = new ApiClientException(apiException);

        clientException.Should().BeAssignableTo<Exception>();
        clientException.Message.Should().Be("erro interno");
        clientException.ApiException.Should().BeSameAs(apiException);
    }

    [Fact]
    public void EntityDtoBase_ExpoeIdTipado()
    {
        var dto = new PersonDto { Id = 10 };

        ((IEntityDto<int>)dto).Id.Should().Be(10);
    }
}
