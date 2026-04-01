namespace Codout.Security.Core;

public enum PasswordHasherStrength
{
    /// <summary>For interactive sessions (fast: uses 16MB of RAM).</summary>
    Interactive,
    /// <summary>For normal use (moderate: uses 128MB of RAM).</summary>
    Moderate,
    /// <summary>For highly sensitive data (slow: uses more than 1GB of RAM).</summary>
    Sensitive
}