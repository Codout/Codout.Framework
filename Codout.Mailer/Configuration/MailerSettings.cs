namespace Codout.Mailer.Configuration;

public class MailerSettings
{
    public const string SectionName = "MailerSettings";

    public string DefaultFromName { get; set; }

    public string DefaultFromEmail { get; set; }
}