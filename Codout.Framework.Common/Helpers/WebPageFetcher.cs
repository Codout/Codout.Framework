using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Codout.Framework.Common.Helpers;

/// <summary>
/// Utilitários modernos para buscar conteúdo web
/// </summary>
public static class WebPageFetcher
{
    private static readonly HttpClient DefaultHttpClient = new()
    {
        Timeout = TimeSpan.FromSeconds(30)
    };

    /// <summary>
    /// Busca o conteúdo de uma página web de forma assíncrona
    /// </summary>
    /// <param name="url">A URL da página</param>
    /// <param name="cancellationToken">Token para cancelamento da operação</param>
    /// <returns>O conteúdo da página como string</returns>
    /// <exception cref="ArgumentException">Quando a URL é inválida</exception>
    /// <exception cref="HttpRequestException">Quando há erro na requisição HTTP</exception>
    /// <exception cref="TaskCanceledException">Quando a operação é cancelada ou timeout</exception>
    public static async Task<string> ReadWebPageAsync(string url, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(url, nameof(url));

        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
            throw new ArgumentException($"URL inválida: {url}", nameof(url));

        using var response = await DefaultHttpClient.GetAsync(uri, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Versão mais completa com configurações customizáveis
    /// </summary>
    /// <param name="url">A URL da página</param>
    /// <param name="options">Opções de configuração da requisição</param>
    /// <param name="cancellationToken">Token para cancelamento</param>
    /// <returns>O conteúdo da página</returns>
    public static async Task<string> ReadWebPageAsync(string url, WebPageOptions options, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(url, nameof(url));
        ArgumentNullException.ThrowIfNull(options, nameof(options));

        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
            throw new ArgumentException($"URL inválida: {url}", nameof(url));

        using var httpClient = CreateHttpClient(options);
        using var request = CreateHttpRequest(uri, options);

        try
        {
            using var response = await httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
            
            // Log se fornecido
            options.Logger?.LogInformation("Requisição para {Url} retornou status {StatusCode}", url, response.StatusCode);

            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            
            // Aplicar encoding específico se necessário
            if (options.ForceEncoding != null)
            {
                var bytes = Encoding.GetEncoding("ISO-8859-1").GetBytes(content);
                content = options.ForceEncoding.GetString(bytes);
            }

            return content;
        }
        catch (HttpRequestException ex)
        {
            options.Logger?.LogError(ex, "Erro ao buscar página {Url}", url);
            throw;
        }
        catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
        {
            options.Logger?.LogWarning("Timeout ao buscar página {Url} após {Timeout}ms", url, options.Timeout.TotalMilliseconds);
            throw;
        }
    }

    /// <summary>
    /// Versão síncrona para compatibilidade (não recomendada)
    /// </summary>
    /// <param name="url">A URL da página</param>
    /// <returns>O conteúdo da página</returns>
    [Obsolete("Use ReadWebPageAsync para melhor performance e evitar deadlocks")]
    public static string ReadWebPage(string url)
    {
        return ReadWebPageAsync(url).GetAwaiter().GetResult();
    }

    private static HttpClient CreateHttpClient(WebPageOptions options)
    {
        var client = new HttpClient();
        
        client.Timeout = options.Timeout;
        
        if (!string.IsNullOrEmpty(options.UserAgent))
            client.DefaultRequestHeaders.Add("User-Agent", options.UserAgent);

        foreach (var header in options.CustomHeaders)
            client.DefaultRequestHeaders.Add(header.Key, header.Value);

        return client;
    }

    private static HttpRequestMessage CreateHttpRequest(Uri uri, WebPageOptions options)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, uri);

        if (!string.IsNullOrEmpty(options.Accept))
            request.Headers.Accept.ParseAdd(options.Accept);

        if (!string.IsNullOrEmpty(options.AcceptLanguage))
            request.Headers.AcceptLanguage.ParseAdd(options.AcceptLanguage);

        return request;
    }
}

/// <summary>
/// Opções de configuração para requisições web
/// </summary>
public class WebPageOptions
{
    /// <summary>
    /// Timeout da requisição (padrão: 30 segundos)
    /// </summary>
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>
    /// User-Agent personalizado
    /// </summary>
    public string UserAgent { get; set; } = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36";

    /// <summary>
    /// Header Accept personalizado
    /// </summary>
    public string Accept { get; set; } = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8";

    /// <summary>
    /// Accept-Language header
    /// </summary>
    public string AcceptLanguage { get; set; } = "pt-BR,pt;q=0.9,en;q=0.8";

    /// <summary>
    /// Encoding forçado para o conteúdo
    /// </summary>
    public Encoding ForceEncoding { get; set; }

    /// <summary>
    /// Headers HTTP customizados
    /// </summary>
    public Dictionary<string, string> CustomHeaders { get; set; } = new();

    /// <summary>
    /// Logger para registrar operações
    /// </summary>
    public ILogger Logger { get; set; }

    /// <summary>
    /// Opções padrão otimizadas
    /// </summary>
    public static WebPageOptions Default => new();

    /// <summary>
    /// Opções para sites que requerem User-Agent específico
    /// </summary>
    public static WebPageOptions ForModernBrowser => new()
    {
        UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36",
        Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,*/*;q=0.8"
    };

    /// <summary>
    /// Opções para APIs que esperam JSON
    /// </summary>
    public static WebPageOptions ForApi => new()
    {
        Accept = "application/json, text/plain, */*",
        UserAgent = "ApiClient/1.0"
    };
}

/// <summary>
/// Exemplo de uso com injeção de dependência
/// </summary>
public class WebContentService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<WebContentService> _logger;

    public WebContentService(HttpClient httpClient, ILogger<WebContentService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    /// <summary>
    /// Método específico para buscar conteúdo usando HttpClient injetado
    /// </summary>
    public async Task<string> FetchContentAsync(string url, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(url, nameof(url));

        try
        {
            _logger.LogInformation("Buscando conteúdo de {Url}", url);
            
            using var response = await _httpClient.GetAsync(url, cancellationToken);
            response.EnsureSuccessStatusCode();
            
            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            
            _logger.LogInformation("Conteúdo obtido com sucesso. Tamanho: {Size} caracteres", content.Length);
            
            return content;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar conteúdo de {Url}", url);
            throw;
        }
    }
}