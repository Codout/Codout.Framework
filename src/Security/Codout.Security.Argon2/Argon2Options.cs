namespace Codout.Security.Argon2;

public class Argon2Options
{
    /// <summary>
    /// opslimit represents a maximum amount of computations to perform.
    /// Raising this number will make the function require more CPU cycles to compute a key.
    /// When set along with MemLimit, overrides the Strength setting.
    /// </summary>
    public long? OpsLimit { get; set; }

    /// <summary>
    /// memlimit is the maximum amount of RAM that the function will use, in bytes.
    /// When set along with OpsLimit, overrides the Strength setting.
    /// </summary>
    public int? MemLimit { get; set; }
}
