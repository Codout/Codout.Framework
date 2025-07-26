using System;
using Codout.Mailer.Configuration;
using Codout.Mailer.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Codout.Mailer.AWS.Configuration;

public static class ConfigureServices
{
    /// <summary>
    /// Extensão para configurar mailer com AWS SES
    /// </summary>
    public static IServiceCollection AddMailerWithAws(this IServiceCollection services,
        IConfiguration configuration,
        Action<MailerOptions>? configureOptions = null)
    {
        return services
            .AddMailer(configuration, configureOptions)
            .AddScoped<IMailerDispatcher, AWSDispatcher>();
    }
}