using System;
using Codout.Mailer.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

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

        // Health checks
        services.AddHealthChecks()
            .AddCheck<MailerHealthCheck>("mailer");

        return services;
    }
}