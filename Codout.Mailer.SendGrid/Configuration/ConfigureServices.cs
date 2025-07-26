using System;
using Codout.Mailer.Configuration;
using Codout.Mailer.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Codout.Mailer.SendGrid.Configuration;

public static class ConfigureServices
{
    /// <summary>
    /// Extensão para configurar mailer com SendGrid
    /// </summary>
    public static IServiceCollection AddMailerWithSendGrid(this IServiceCollection services,
        IConfiguration configuration,
        Action<MailerOptions>? configureOptions = null)
    {
        return services
            .AddMailer(configuration, configureOptions)
            .AddScoped<IMailerDispatcher, SendGridDispatcher>();
    }
}