using System;
using System.Reflection;

namespace Codout.Mailer.Razor.Configuration;

/// <summary>
/// Opções de configuração para o template engine Razor do ASP.NET Core
/// </summary>
public class RazorMailerOptions
{
    /// <summary>
    /// Assembly que contém os templates Razor embarcados como recursos
    /// </summary>
    public Assembly TemplateAssembly { get; set; }

    /// <summary>
    /// Namespace raiz dos templates embarcados no assembly
    /// </summary>
    public string RootNamespace { get; set; }

    /// <summary>
    /// Habilita o cache de templates compilados em memória
    /// </summary>
    public bool EnableCache { get; set; } = true;

    internal void Validate()
    {
        if (TemplateAssembly == null)
            throw new InvalidOperationException("TemplateAssembly deve ser definido para localizar templates embarcados.");

        if (string.IsNullOrWhiteSpace(RootNamespace))
            throw new InvalidOperationException("RootNamespace deve ser definido para localizar templates corretamente.");
    }
}
