namespace Codout.Mailer.AWS.Configuration;

public class AWSSettings
{
    public const string SectionName = "AWSSettings";

    public string RegionEndpoint { get; set; } = null!;

    public string AccessKey { get; set; } = null!;

    public string SecretKey { get; set; } = null!;
}
