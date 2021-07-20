using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Codout.Framework.Api.Client.Extensions
{
    public static class HttpClientExtensions
    {
        public static async Task<TDtoOutput> PostAsync<TDtoOutput, TDtoInput>(this HttpClient client, string uriService, TDtoInput obj)
        {
            return await CallClientAsync<TDtoOutput>(async () => await client.PostAsJsonAsync(uriService, obj));
        }

        public static async Task PostAsync<TDtoInput>(this HttpClient client, string uriService, TDtoInput obj)
        {
            await CallClientAsync(async () => await client.PostAsJsonAsync(uriService, obj));
        }

        public static async Task PostAsync(this HttpClient client, string uriService)
        {
            await CallClientAsync(async () => await client.PostAsync(uriService, null));
        }

        public static async Task<TDtoOutput> PutAsync<TDtoOutput, TDtoInput>(this HttpClient client, string uriService, TDtoInput obj)
        {
            return await CallClientAsync<TDtoOutput>(async () => await client.PutAsJsonAsync(uriService, obj));
        }

        public static async Task<TDtoOutput> GetAsync<TDtoOutput>(this HttpClient client, string uriService)
        {
            return await CallClientAsync<TDtoOutput>(async () => await client.GetAsync(uriService));
        }

        public static async Task DeleteAsync(this HttpClient client, string uriService)
        {
            await CallClientAsync(async () => await client.DeleteAsync(uriService));
        }

        private static async Task<TDto> CallClientAsync<TDto>(Func<Task<HttpResponseMessage>> client)
        {
            HttpResponseMessage response = null;

            try
            {
                response = await client();
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsAsync<TDto>();
            }
            catch (Exception ex)
            {
                throw response != null ? await response.GetExceptionAsync(ex) : ex;
            }
        }

        private static async Task CallClientAsync(Func<Task<HttpResponseMessage>> client)
        {
            HttpResponseMessage response = null;

            try
            {
                response = await client();
                response.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                throw response != null ? await response.GetExceptionAsync(ex) : ex;
            }
        }

        private static async Task<Exception> GetExceptionAsync(this HttpResponseMessage httpResponseMessage, Exception originalException)
        {
            if (httpResponseMessage == null) 
                return originalException;

            try
            {
                var apiException = await httpResponseMessage.Content.ReadAsAsync<ApiException>();
                throw new ApiClientException(apiException);
            }
            catch
            {
                throw new Exception(await httpResponseMessage.Content.ReadAsStringAsync());
            }
        }
    }
}
