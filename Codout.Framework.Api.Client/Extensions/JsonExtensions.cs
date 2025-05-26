using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Codout.Framework.Api.Client.Extensions;

/// <summary>
///     Classe de extensão para tratamento de pacotes Json
/// </summary>
public static class JsonExtensions
{
    public static async Task<TModel> ReadAsAsync<TModel>(this HttpContent httpContent)
    {
        var data = await httpContent.ReadAsStringAsync();

        var items = JsonConvert.DeserializeObject<TModel>(data, new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore
        });

        return items;
    }

    public static async Task<HttpResponseMessage> PostAsJsonAsync<TModel>(this HttpClient client, string requestUrl,
        TModel model)
    {
        var json = JsonConvert.SerializeObject(model);
        var stringContent = new StringContent(json);
        stringContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        return await client.PostAsync(requestUrl, stringContent);
    }

    public static async Task<HttpResponseMessage> PutAsJsonAsync<TModel>(this HttpClient client, string requestUrl,
        TModel model)
    {
        var json = JsonConvert.SerializeObject(model);
        var stringContent = new StringContent(json);
        stringContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        return await client.PutAsync(requestUrl, stringContent);
    }
}