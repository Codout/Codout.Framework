using System;

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
    public Type TemplateRootType { get; set; }

    /// <summary>
    /// Configurações específicas do RazorLight
    /// </summary>
    public RazorLightOptions RazorLight { get; set; } = new();

    public void Validate()
    {
        if (TemplateRootType == null)
            throw new InvalidOperationException("TemplateRootType deve ser definido para localizar templates embarcados.");

        if (RazorLight != null)
        {
            if (string.IsNullOrWhiteSpace(RazorLight.DefaultNamespace))
                throw new InvalidOperationException("RazorLight.DefaultNamespace deve ser definido para localizar templates corretamente.");
        }
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
    public string DefaultNamespace { get; set; }

    /// <summary>
    /// Habilita o cache de templates
    /// </summary>
    public bool EnableTemplateCache { get; set; }
}