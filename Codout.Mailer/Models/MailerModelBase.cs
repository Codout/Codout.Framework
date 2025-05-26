using System.Net.Mail;

namespace Codout.Mailer.Models;

public class MailerModelBase
{
    public MailAddress To { get; set; }
}