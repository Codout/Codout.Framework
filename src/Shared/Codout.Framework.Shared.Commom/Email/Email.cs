using System;
using System.Net.Mail;
using System.Text;

namespace Codout.Framework.Shared.Commom.Email
{
    /// <summary>
    /// Classe de envio de Emails
    /// </summary>
    public class Email
    {
        /// <summary>
        /// Envia Email
        /// Obs.: Verifique se existe configuração da conta de envio de Emails no arquivo .config
        /// </summary>
        /// <param name="emailTo">Email de destino</param>
        /// <param name="subject">Assunto</param>
        /// <param name="message">Mensagem do Email</param>
        public void Send(string subject, string message, string emailTo)
        {
            if (string.IsNullOrWhiteSpace(emailTo))
                throw new Exception("Email must not be empty");

            var from = string.IsNullOrWhiteSpace(Configuration.Email.EmailConfiguration.DisplayName)
                ? new MailAddress(Configuration.Email.EmailConfiguration.EmailFrom)
                : new MailAddress(Configuration.Email.EmailConfiguration.EmailFrom,
                    Configuration.Email.EmailConfiguration.DisplayName);

            var to = new MailAddress(emailTo.Trim());

            var mailMessage = new MailMessage(from, to)
            {
                Body = message,
                BodyEncoding = Encoding.UTF8,
                Subject = subject,
                SubjectEncoding = Encoding.UTF8,
                IsBodyHtml = Configuration.Email.EmailConfiguration.IsBodyHtml
            };

            var client = new SmtpClient(Configuration.Email.EmailConfiguration.Smtp, Configuration.Email.EmailConfiguration.Port)
            {
                UseDefaultCredentials = Configuration.Email.EmailConfiguration.DefaultCredentials,
                EnableSsl = Configuration.Email.EmailConfiguration.EnableSsl,
                Credentials = new System.Net.NetworkCredential(Configuration.Email.EmailConfiguration.UserName, Configuration.Email.EmailConfiguration.Password)
            };

            client.Send(mailMessage);
        }
    }
}
