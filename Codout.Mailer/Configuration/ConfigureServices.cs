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
        Action<MailerOptions>? configureOptions = null)
    {
        // Configuração das settings principais
        services.Configure<MailerSettings>(configuration.GetSection(MailerSettings.SectionName));
        
        // Configuração das opções do mailer
        services.Configure<MailerOptions>(options =>
        {
            configuration.GetSection("MailerOptions").Bind(options);
            configureOptions?.Invoke(options);
            options.Validate(); // Valida as configurações
        });
        
        // Registro do RazorLightEngine como singleton
        services.AddSingleton(provider =>
        {
            var options = provider.GetRequiredService<IOptions<MailerOptions>>().Value;
            
            var builder = new RazorLightEngineBuilder();
            
            // Configuração baseada nas opções
            if (options.TemplateRootType != null)
            {
                builder.UseEmbeddedResourcesProject(options.TemplateRootType);
            }
            
            if (options.EnableTemplateCache)
            {
                builder.UseMemoryCachingProvider();
            }
            
            // Configurações específicas do RazorLight
            if (!string.IsNullOrEmpty(options.RazorLight.DefaultNamespace))
            {
                builder.SetOperatingAssembly(options.TemplateRootType?.Assembly ?? typeof(MailerOptions).Assembly);
            }
            
            return builder.Build();
        });
        
        // Registro dos serviços principais
        services.AddScoped<ITemplateEngine, RazorTemplateEngine>();
        services.AddScoped<IMailerService, MailerService>();
        
        // Health checks
        services.AddHealthChecks()
            .AddCheck<MailerHealthCheck>("mailer");
        
        return services;
    }
}