using System.Net;
using System.Reflection;
using System.Text;

namespace Codout.Framework.Api.Client.Tests;

/// <summary>
///     Handler fake que captura as requisições e devolve respostas pré-configuradas,
///     sem tocar a rede.
/// </summary>
public class FakeHttpMessageHandler : HttpMessageHandler
{
    private readonly Queue<Func<HttpRequestMessage, HttpResponseMessage>> _responses = new();

    public List<CapturedRequest> Requests { get; } = [];

    public record CapturedRequest(HttpMethod Method, Uri? Uri, string? Body, string? ContentType);

    public FakeHttpMessageHandler Enqueue(HttpStatusCode statusCode, string? body = null,
        string contentType = "application/json")
    {
        _responses.Enqueue(_ =>
        {
            var response = new HttpResponseMessage(statusCode);
            if (body != null)
                response.Content = new StringContent(body, Encoding.UTF8, contentType);
            return response;
        });
        return this;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        string? body = null;
        if (request.Content != null)
            body = await request.Content.ReadAsStringAsync(cancellationToken);

        Requests.Add(new CapturedRequest(
            request.Method,
            request.RequestUri,
            body,
            request.Content?.Headers.ContentType?.MediaType));

        if (_responses.Count == 0)
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{}", Encoding.UTF8, "application/json")
            };

        return _responses.Dequeue()(request);
    }

    public HttpClient CreateClient(string baseUrl = "https://api.example.test/")
    {
        return new HttpClient(this) { BaseAddress = new Uri(baseUrl) };
    }

    /// <summary>
    ///     Substitui via reflection o handler interno do HttpClient criado por
    ///     ApiClientBase (que não permite injeção de handler).
    /// </summary>
    public void InjectInto(HttpClient client)
    {
        var field = typeof(HttpMessageInvoker)
            .GetField("_handler", BindingFlags.Instance | BindingFlags.NonPublic);

        if (field == null)
            throw new InvalidOperationException(
                "Campo privado _handler não encontrado em HttpMessageInvoker — ajuste o teste para esta versão do runtime.");

        field.SetValue(client, this);
    }
}
