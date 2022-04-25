using System.Net.Mail;
using System.Threading.Tasks;
using Codout.Mailer.Models;

namespace Codout.Mailer
{
    public interface ICodoutMailer
    {
        Task<MailerResponse> Send<T>(string templateKey, T model, string subject, Attachment[] attachments = null) where T : MailerModelBase;
    }
}
