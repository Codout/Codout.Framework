namespace Codout.Mailer.AWS.Configuration;

public class AWSSettings
{
    public const string SectionName = "AWSSettings";

    public string RegionEndpoint { get; set; }

    public string AccessKey { get; set; }

    public string SecretKey { get; set; }
}