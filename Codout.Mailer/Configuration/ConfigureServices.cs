using System;
using Codout.Mailer.Interfaces;
using Codout.Mailer.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using RazorLight;

namespace Codout.Mailer.Configuration;

public static class ConfigureServices
{
    public static IServiceCollection AddMailer(this IServiceCollection services,
        IConfiguration configuration,
        Action<MailerOptions>? options = null)
    {
        // Configuração das settings principais
        services.Configure<MailerSettings>(configuration.GetSection(MailerSettings.SectionName));

        // Configuração das opções do mailer
        services.Configure<MailerOptions>(mailerOptions =>
        {
            configuration.GetSection("MailerOptions").Bind(mailerOptions);
            options?.Invoke(mailerOptions);
            mailerOptions.Validate();
        });

        // Registro do RazorLightEngine como singleton
        services.AddSingleton(provider =>
        {
            var mailerOptions = provider.GetRequiredService<IOptions<MailerOptions>>().Value;

            var builder = new RazorLightEngineBuilder();

            builder.UseEmbeddedResourcesProject(mailerOptions.TemplateRootType.Assembly, mailerOptions.RazorLight.DefaultNamespace);

            if (mailerOptions.RazorLight.EnableTemplateCache)
                builder.UseMemoryCachingProvider();

            return builder.Build();
        });

        // Registro dos serviços principais
        services.AddScoped<ITemplateEngine, RazorTemplateEngine>();

        // Health checks
        services.AddHealthChecks()
            .AddCheck<MailerHealthCheck>("mailer");

        return services;
    }
}