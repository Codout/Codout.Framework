using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Codout.Mailer.Helpers;
using Codout.Mailer.Models;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace Codout.Mailer.SendGrid
{
    public class SendGridDispatcher : ICodoutMailerDispatcher
    {
        private readonly SendGridSettings _sendGridSettings;

        public SendGridDispatcher(SendGridSettings sendGridSettings)
        {
            _sendGridSettings = sendGridSettings;
        }

        public async Task<MailerResponse> Send(MailAddress from, MailAddress to, string subject, string htmlContent, string plainTextContent = null, System.Net.Mail.Attachment[] attachments = null)
        {
            var client = new SendGridClient(_sendGridSettings.ApiKey);

            var msg = MailHelper.CreateSingleEmail(
                new EmailAddress(from.Address, from.DisplayName),
                new EmailAddress(to.Address, to.DisplayName),
                subject,
                plainTextContent,
                htmlContent);

            if (attachments != null && attachments.Length > 0)
            {
                foreach (var attachment in attachments)
                {
                    var bytes = attachment.ContentStream.ReadFully();
                    var file = Convert.ToBase64String(bytes);
                    msg.AddAttachment(attachment.Name, file);
                }
            }

            try
            {
                var response = await client.SendEmailAsync(msg);

                return new MailerResponse
                {
                    Sent = response.StatusCode == HttpStatusCode.Accepted
                };
            }
            catch (Exception e)
            {
                return new MailerResponse
                {
                    Sent = false,
                    ErrorMessages = new List<string>(new[] { e.Message })
                };
            }
        }
    }
}
