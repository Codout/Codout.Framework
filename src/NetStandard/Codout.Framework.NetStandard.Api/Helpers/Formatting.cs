﻿using Newtonsoft.Json;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Codout.Framework.NetStandard.Api.Helpers
{

    /// <summary>
    /// Classe de extensão para tratamento de pacotes Json
    /// </summary>
    public static class Formatting
    {
        public static async Task<TModel> ReadAsAsync<TModel>(this HttpContent httpContent)
        {
            var data = await httpContent.ReadAsStringAsync();
            var items = JsonConvert.DeserializeObject<TModel>(data);
            return items;
        }

        public static async Task<HttpResponseMessage> PostAsJsonAsync<TModel>(this HttpClient client, string requestUrl, TModel model)
        {
            var json = JsonConvert.SerializeObject(model);
            var stringContent = new StringContent(json, Encoding.UTF8, "application/json");
            return await client.PostAsync(requestUrl, stringContent);
        }

        public static async Task<HttpResponseMessage> PutAsJsonAsync<TModel>(this HttpClient client, string requestUrl, TModel model)
        {
            var json = JsonConvert.SerializeObject(model);
            var stringContent = new StringContent(json, Encoding.UTF8, "application/json");
            return await client.PutAsync(requestUrl, stringContent);
        }
    }
}
