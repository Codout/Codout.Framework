using System.Collections.Generic;

namespace Codout.Mailer.Models;

public class MailerResponse
{
    public bool Sent { get; set; }

    public IList<string> ErrorMessages { get; set; }
}