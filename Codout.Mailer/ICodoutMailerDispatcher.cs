using System.Net.Mail;
using System.Threading.Tasks;
using Codout.Mailer.Models;

namespace Codout.Mailer
{
    public interface ICodoutMailerDispatcher
    {
        Task<MailerResponse> Send(MailAddress from, MailAddress to, string subject, string htmlContent, string plainTextContent = null, Attachment[] attachments = null);
    }
}
