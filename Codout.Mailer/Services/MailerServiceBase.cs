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

public abstract class MailerServiceBase(
    IOptions<MailerSettings> mailerSettings,
    IMailerDispatcher dispatcher,
    ITemplateEngine templateEngine,
    ILogger<MailerServiceBase> logger)
    : IMailerService
{
    private readonly MailerSettings _mailerSettings = mailerSettings.Value;

    public virtual async Task<MailerResponse> Send<T>(string templateKey, T model, string subject, Attachment[] attachments = null) where T : MailerModelBase
    {
        using var activity = MailerActivitySource.StartActivity("MailerService.Send");
        logger.LogInformation("Sending email with template {TemplateKey} to {Recipient}", templateKey, model.To.Address);
        
        try
        {
            var htmlContent = await templateEngine.RenderAsync(templateKey, model);
            var plainTextContent = HtmlUtilities.ConvertToPlainText(htmlContent);
            return await dispatcher.Send(new MailAddress(_mailerSettings.DefaultFromEmail, _mailerSettings.DefaultFromName), model.To,
                subject, htmlContent, plainTextContent, attachments);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send email with template {TemplateKey}", templateKey);
            return new MailerResponse { Sent = false, ErrorMessages = [ex.Message] };
        }
    }
}

