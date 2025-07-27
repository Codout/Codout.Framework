namespace Codout.Mailer.SendGrid.Configuration;

public class SendGridSettings
{
    public const string SectionName = "SendGridSettings";

    public string ApiKey { get; set; }

    public bool StandBox { get; set; }
}