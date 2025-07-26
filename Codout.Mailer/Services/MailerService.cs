using System;
using System.Net.Mail;
using System.Threading.Tasks;
using Codout.Mailer.Configuration;
using Codout.Mailer.Diagnostics;
using Codout.Mailer.Helpers;
using Codout.Mailer.Interfaces;
using Codout.Mailer.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Codout.Mailer.Services;

public class MailerService : IMailerService
{
    private readonly IMailerDispatcher _dispatcher;
    private readonly ITemplateEngine _templateEngine;
    private readonly MailerSettings _mailerSettings;
    private readonly ILogger<MailerService> _logger;

    protected MailerService(IOptions<MailerSettings> mailerSettings, IMailerDispatcher dispatcher, ITemplateEngine templateEngine, ILogger<MailerService> logger)
    {
        _mailerSettings = mailerSettings.Value;
        _dispatcher = dispatcher;
        _templateEngine = templateEngine;
        _logger = logger;
    }


    public virtual async Task<MailerResponse> Send<T>(string templateKey, T model, string subject, Attachment[] attachments = null) where T : MailerModelBase
    {
        using var activity = MailerActivitySource.StartActivity("MailerService.Send");
        _logger.LogInformation("Sending email with template {TemplateKey} to {Recipient}", templateKey, model.To.Address);
        
        try
        {
            var htmlContent = await _templateEngine.RenderAsync(templateKey, model);
            var plainTextContent = HtmlUtilities.ConvertToPlainText(htmlContent);
            return await _dispatcher.Send(new MailAddress(_mailerSettings.DefaultFromEmail, _mailerSettings.DefaultFromName), model.To,
                subject, htmlContent, plainTextContent, attachments);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email with template {TemplateKey}", templateKey);
            return new MailerResponse { Sent = false, ErrorMessages = [ex.Message] };
        }
    }
}

