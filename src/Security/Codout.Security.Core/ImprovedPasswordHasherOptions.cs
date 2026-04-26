namespace Codout.Security.Core;

public class ImprovedPasswordHasherOptions
{
    /// <summary>
    /// Password Strength. Used to configure the hashing algorithm's resource usage.
    /// </summary>
    public PasswordHasherStrength Strength { get; set; } = PasswordHasherStrength.Sensitive;
}