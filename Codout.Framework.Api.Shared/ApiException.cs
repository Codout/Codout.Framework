using System.Text.Json;

namespace Codout.Framework.Api.Client;

public class ApiException(int statusCode, string message, params ApiErrorMessage[] errors)
{
    public int StatusCode { get; set; } = statusCode;

    public string Message { get; set; } = message;

    public ApiErrorMessage[] Errors { get; } = errors;

    public override string ToString()
    {
        return JsonSerializer.Serialize(this, JsonSerializerOptions.Web);
    }
}