using System.Diagnostics;

namespace Codout.Mailer.Diagnostics;

/// <summary>
/// ActivitySource para rastreamento distribuído do Codout.Mailer
/// Compatível com OpenTelemetry e .NET Diagnostics
/// </summary>
public static class MailerActivitySource
{
    /// <summary>
    /// Nome do ActivitySource para o sistema de mailer
    /// </summary>
    public const string ActivitySourceName = "Codout.Mailer";

    /// <summary>
    /// Versão do ActivitySource baseada na versão do assembly
    /// </summary>
    public static readonly string Version = typeof(MailerActivitySource).Assembly.GetName().Version?.ToString() ?? "1.0.0";

    /// <summary>
    /// Instância singleton do ActivitySource
    /// </summary>
    private static readonly ActivitySource _activitySource = new(ActivitySourceName, Version);

    /// <summary>
    /// Inicia uma nova Activity para rastreamento de operações do mailer
    /// </summary>
    /// <param name="operationName">Nome da operação (ex: "MailerService.Send", "TemplateEngine.Render")</param>
    /// <param name="kind">Tipo da activity (padrão: Internal)</param>
    /// <returns>Activity iniciada ou null se não houver listeners</returns>
    public static Activity? StartActivity(string operationName, ActivityKind kind = ActivityKind.Internal)
    {
        return _activitySource.StartActivity(operationName, kind);
    }

    /// <summary>
    /// Inicia uma Activity com tags customizadas
    /// </summary>
    /// <param name="operationName">Nome da operação</param>
    /// <param name="kind">Tipo da activity</param>
    /// <param name="tags">Tags a serem adicionadas à activity</param>
    /// <returns>Activity iniciada ou null se não houver listeners</returns>
    public static Activity? StartActivity(string operationName, ActivityKind kind, params (string Key, object? Value)[] tags)
    {
        var activity = _activitySource.StartActivity(operationName, kind);
        
        if (activity != null)
        {
            foreach (var (key, value) in tags)
            {
                activity.SetTag(key, value?.ToString());
            }
        }
        
        return activity;
    }

    /// <summary>
    /// Adiciona tags padrão para operações de email
    /// </summary>
    /// <param name="activity">Activity atual</param>
    /// <param name="templateKey">Chave do template</param>
    /// <param name="recipient">Destinatário do email</param>
    /// <param name="subject">Assunto do email</param>
    public static void AddEmailTags(this Activity? activity, string templateKey, string recipient, string subject)
    {
        activity?.SetTag("mailer.template_key", templateKey)
                 .SetTag("mailer.recipient", recipient)
                 .SetTag("mailer.subject", subject)
                 .SetTag("mailer.operation_type", "send_email");
    }

    /// <summary>
    /// Adiciona tag de erro à activity
    /// </summary>
    /// <param name="activity">Activity atual</param>
    /// <param name="exception">Exceção ocorrida</param>
    public static void SetError(this Activity? activity, System.Exception exception)
    {
        activity?.SetStatus(ActivityStatusCode.Error, exception.Message)
                 .SetTag("error.type", exception.GetType().Name)
                 .SetTag("error.message", exception.Message)
                 .SetTag("error.stack_trace", exception.StackTrace);
    }

    /// <summary>
    /// Adiciona tag de sucesso à activity
    /// </summary>
    /// <param name="activity">Activity atual</param>
    /// <param name="messageId">ID da mensagem enviada (se disponível)</param>
    public static void SetSuccess(this Activity? activity, string? messageId = null)
    {
        activity?.SetStatus(ActivityStatusCode.Ok)
                 .SetTag("mailer.status", "success");
        
        if (!string.IsNullOrEmpty(messageId))
        {
            activity?.SetTag("mailer.message_id", messageId);
        }
    }

    /// <summary>
    /// Dispõe o ActivitySource - deve ser chamado na finalização da aplicação
    /// </summary>
    public static void Dispose()
    {
        _activitySource.Dispose();
    }
}