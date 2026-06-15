using Codout.Mailer.Models;

namespace Codout.Mailer.Razor.Tests;

public class WelcomeModel : MailerModelBase
{
    public string Nome { get; set; } = string.Empty;
}
