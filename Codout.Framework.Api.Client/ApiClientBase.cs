using System;
using System.Net.Http;
using System.Net.Http.Headers;

namespace Codout.Framework.Api.Client;

public abstract class ApiClientBase
{
    protected ApiClientBase(string uriService, string baseUrl)
    {
        UriService = uriService;
        Client = new HttpClient { BaseAddress = new Uri(baseUrl) };
        Client.DefaultRequestHeaders.Accept.Clear();
        Client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        Client.Timeout = new TimeSpan(0, 0, 1, 0);
    }

    protected ApiClientBase(string uriService, string baseUrl, string apiKey)
    {
        UriService = uriService;
        Client = new HttpClient { BaseAddress = new Uri(baseUrl) };
        Client.DefaultRequestHeaders.Accept.Clear();
        Client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        Client.DefaultRequestHeaders.Add("ApiKey", apiKey);
        Client.Timeout = new TimeSpan(0, 0, 1, 0);
    }

    public string UriService { get; }

    public HttpClient Client { get; }
}