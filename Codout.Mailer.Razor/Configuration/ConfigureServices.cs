using System;
using Codout.Mailer.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;

namespace Codout.Mailer.Razor.Configuration;

public static class ConfigureServices
{
    /// <summary>
    /// Registra o template engine Razor nativo do ASP.NET Core para renderização de templates de e-mail.
    /// Deve ser chamado após <c>AddMailer()</c>.
    /// </summary>
    public static IServiceCollection AddMailerRazor(
        this IServiceCollection services,
        Action<RazorMailerOptions> configure)
    {
        var options = new RazorMailerOptions();
        configure(options);
        options.Validate();

        services.AddMvcCore()
            .AddRazorViewEngine()
            .AddRazorRuntimeCompilation(razorOptions =>
            {
                razorOptions.FileProviders.Add(
                    new EmbeddedFileProvider(options.TemplateAssembly, options.RootNamespace));
            });

        services.AddScoped<ITemplateEngine, RazorViewTemplateEngine>();

        return services;
    }
}


