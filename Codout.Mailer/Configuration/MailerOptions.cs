using System;
using System.ComponentModel.DataAnnotations;

namespace Codout.Mailer.Configuration;

/// <summary>
/// Opções de configuração para o sistema de mailer
/// </summary>
public class MailerOptions
{
    /// <summary>
    /// Tipo raiz para localização de templates embarcados
    /// Usado pelo RazorLight para encontrar templates em resources
    /// </summary>
    public Type? TemplateRootType { get; set; }

    /// <summary>
    /// Timeout para renderização de templates (em segundos)
    /// Padrão: 30 segundos
    /// </summary>
    [Range(1, 300)]
    public int TemplateRenderTimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// Timeout para envio de emails (em segundos)
    /// Padrão: 60 segundos
    /// </summary>
    [Range(1, 300)]
    public int EmailSendTimeoutSeconds { get; set; } = 60;

    /// <summary>
    /// Habilita cache de templates compilados
    /// Padrão: true
    /// </summary>
    public bool EnableTemplateCache { get; set; } = true;

    /// <summary>
    /// Tamanho máximo do cache de templates
    /// Padrão: 100 templates
    /// </summary>
    [Range(1, 1000)]
    public int TemplateCacheSize { get; set; } = 100;

    /// <summary>
    /// Tempo de vida do cache de templates (em minutos)
    /// Padrão: 60 minutos
    /// </summary>
    [Range(1, 1440)]
    public int TemplateCacheLifetimeMinutes { get; set; } = 60;

    /// <summary>
    /// Habilita rastreamento distribuído (OpenTelemetry)
    /// Padrão: true
    /// </summary>
    public bool EnableDistributedTracing { get; set; } = true;

    /// <summary>
    /// Habilita métricas personalizadas
    /// Padrão: true
    /// </summary>
    public bool EnableMetrics { get; set; } = true;

    /// <summary>
    /// Número máximo de tentativas de reenvio em caso de falha
    /// Padrão: 3
    /// </summary>
    [Range(0, 10)]
    public int MaxRetryAttempts { get; set; } = 3;

    /// <summary>
    /// Delay base para retry exponential backoff (em milissegundos)
    /// Padrão: 1000ms (1 segundo)
    /// </summary>
    [Range(100, 30000)]
    public int RetryBaseDelayMs { get; set; } = 1000;

    /// <summary>
    /// Modo de desenvolvimento - habilita logs verbosos e validações extras
    /// Padrão: false
    /// </summary>
    public bool DevelopmentMode { get; set; } = false;

    /// <summary>
    /// Prefixo para todas as activities de tracing
    /// Padrão: "Codout.Mailer"
    /// </summary>
    [Required]
    [StringLength(100, MinimumLength = 1)]
    public string TracingPrefix { get; set; } = "Codout.Mailer";

    /// <summary>
    /// Configurações específicas do RazorLight
    /// </summary>
    public RazorLightOptions RazorLight { get; set; } = new();

    /// <summary>
    /// Validação das opções
    /// </summary>
    public void Validate()
    {
        if (TemplateRenderTimeoutSeconds <= 0)
            throw new ArgumentException("TemplateRenderTimeoutSeconds deve ser maior que zero");
        
        if (EmailSendTimeoutSeconds <= 0)
            throw new ArgumentException("EmailSendTimeoutSeconds deve ser maior que zero");
        
        if (TemplateCacheSize <= 0)
            throw new ArgumentException("TemplateCacheSize deve ser maior que zero");
        
        if (string.IsNullOrWhiteSpace(TracingPrefix))
            throw new ArgumentException("TracingPrefix não pode ser vazio");
    }
}

/// <summary>
/// Configurações específicas do RazorLight Engine
/// </summary>
public class RazorLightOptions
{
    /// <summary>
    /// Namespace padrão para templates
    /// </summary>
    public string? DefaultNamespace { get; set; } = "Codout.Mailer.Templates";

    /// <summary>
    /// Habilita compilação em tempo de execução
    /// Padrão: true
    /// </summary>
    public bool EnableRuntimeCompilation { get; set; } = true;

    /// <summary>
    /// Habilita cache de assembly compilado
    /// Padrão: true
    /// </summary>
    public bool EnableAssemblyCache { get; set; } = true;

    /// <summary>
    /// Diretório para cache de assemblies (se vazio, usa temp)
    /// </summary>
    public string? CacheDirectory { get; set; }

    /// <summary>
    /// Habilita hot reload de templates em desenvolvimento
    /// Padrão: false
    /// </summary>
    public bool EnableHotReload { get; set; } = false;
}