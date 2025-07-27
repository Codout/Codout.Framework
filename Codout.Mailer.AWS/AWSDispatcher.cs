using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Mail;
using System.Threading.Tasks;
using Amazon;
using Amazon.Runtime;
using Amazon.SimpleEmailV2;
using Amazon.SimpleEmailV2.Model;
using Codout.Mailer.AWS.Configuration;
using Codout.Mailer.Interfaces;
using Codout.Mailer.Models;
using Microsoft.Extensions.Options;
using MimeKit;
using ContentType = MimeKit.ContentType;

namespace Codout.Mailer.AWS;

public class AWSDispatcher(IOptions<AWSSettings> settings) : IMailerDispatcher
{
    private readonly AWSSettings _settings = settings.Value;

    public async Task<MailerResponse> Send(MailAddress from,
        MailAddress to,
        string subject,
        string htmlContent,
        string plainTextContent = null,
        System.Net.Mail.Attachment[] attachments = null)
    {
        try
        {
            var client = new AmazonSimpleEmailServiceV2Client(
                new BasicAWSCredentials(_settings.AccessKey, _settings.SecretKey),
                RegionEndpoint.GetBySystemName(_settings.RegionEndpoint));

            // Montar e-mail usando MimeKit (recomendado)
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(from.DisplayName, from.Address));
            message.To.Add(new MailboxAddress(to.DisplayName, to.Address));
            message.Subject = subject;

            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = htmlContent,
                TextBody = plainTextContent
            };

            if (attachments != null)
            {
                foreach (var attachment in attachments)
                {
                    using var memoryStream = new MemoryStream();
                    await attachment.ContentStream.CopyToAsync(memoryStream);
                    bodyBuilder.Attachments.Add(attachment.Name, memoryStream.ToArray(), ContentType.Parse(attachment.ContentType.MediaType));
                }
            }

            message.Body = bodyBuilder.ToMessageBody();

            using var ms = new MemoryStream();
            await message.WriteToAsync(ms);

            var sendRequest = new SendEmailRequest
            {
                FromEmailAddress = from.Address,
                Destination = new Destination
                {
                    ToAddresses = [to.Address]
                },
                Content = new EmailContent
                {
                    Raw = new RawMessage
                    {
                        Data = ms
                    }
                }
            };

            var response = await client.SendEmailAsync(sendRequest);

            return new MailerResponse
            {
                Sent = response.HttpStatusCode == System.Net.HttpStatusCode.Accepted,
                ErrorMessages = [response.MessageId]
            };
        }
        catch (Exception ex)
        {
            return new MailerResponse
            {
                Sent = false,
                ErrorMessages = new List<string> { ex.Message }
            };
        }
    }
}
