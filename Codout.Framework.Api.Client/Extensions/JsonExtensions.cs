using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Codout.Framework.Api.Client.Extensions;

/// <summary>
///     Classe de extensão para tratamento de pacotes Json
/// </summary>
public static class JsonExtensions
{
    private static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public static async Task<TModel> ReadAsAsync<TModel>(this HttpContent httpContent)
    {
        var data = await httpContent.ReadAsStringAsync();

        var items = JsonSerializer.Deserialize<TModel>(data, JsonSerializerOptions);

        return items;
    }

    extension(HttpClient client)
    {
        public async Task<HttpResponseMessage> PostAsJsonAsync<TModel>(string requestUrl, TModel model)
        {
            var json = JsonSerializer.Serialize(model, JsonSerializerOptions);
            var stringContent = new StringContent(json);
            stringContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            return await client.PostAsync(requestUrl, stringContent);
        }

        public async Task<HttpResponseMessage> PutAsJsonAsync<TModel>(string requestUrl, TModel model)
        {
            var json = JsonSerializer.Serialize(model, JsonSerializerOptions);
            var stringContent = new StringContent(json);
            stringContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            return await client.PutAsync(requestUrl, stringContent);
        }
    }
}