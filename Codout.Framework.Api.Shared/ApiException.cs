using System.Text.Json;

namespace Codout.Framework.Api.Client;

#pragma warning disable CA1711 // Nome público existente preservado para não quebrar consumidores.
public class ApiException(int statusCode, string message, params ApiErrorMessage[] errors)
#pragma warning restore CA1711
{
    public int StatusCode { get; set; } = statusCode;

    public string Message { get; set; } = message;

    public ApiErrorMessage[] Errors { get; } = errors;

    public override string ToString()
    {
        return JsonSerializer.Serialize(this, JsonSerializerOptions.Web);
    }
}
