using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Amazon;
using Amazon.Runtime;
using Amazon.SimpleEmailV2;
using Amazon.SimpleEmailV2.Model;
using Codout.Mailer.Models;
using Attachment = System.Net.Mail.Attachment;

namespace Codout.Mailer.AWS
{
    public class AWSDispatcher : ICodoutMailerDispatcher
    {
        private readonly AWSSettings _settings;

        public AWSDispatcher(AWSSettings settings)
        {
            _settings = settings;
        }

        public async Task<MailerResponse> Send(MailAddress @from, 
            MailAddress to, 
            string subject, 
            string htmlContent, 
            string plainTextContent = null,
            Attachment[] attachments = null)
        {
            try
            {
                var client = new AmazonSimpleEmailServiceV2Client(new BasicAWSCredentials(_settings.AccessKey, _settings.SecretKey), RegionEndpoint.GetBySystemName(_settings.RegionEndpoint));

                var response = await client.SendEmailAsync(new SendEmailRequest
                {
                    Destination = new Destination
                    {
                        ToAddresses = new List<string> { to.Address }
                    },
                    Content = new EmailContent
                    {
                        Simple = new Message
                        {
                            Body = new Body
                            {
                                Html = new Content
                                {
                                    Charset = "UTF-8",
                                    Data = htmlContent,
                                },
                                Text = new Content
                                {
                                    Charset = "UTF-8",
                                    Data = plainTextContent,
                                }
                            },
                            Subject = new Content
                            {
                                Charset = "UTF-8",
                                Data = subject
                            }
                        }
                    },
                    ReplyToAddresses = new List<string> { from.Address },
                    FromEmailAddress = from.Address
                });

                var messageId = response.MessageId;

                return new MailerResponse
                {
                    Sent = response.HttpStatusCode == HttpStatusCode.Accepted
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
