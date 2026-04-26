namespace Codout.Security.Scrypt;

public class ScryptOptions
{
    /// <summary>
    /// opslimit represents a maximum amount of computations to perform.
    /// When set along with MemLimit, overrides the Strength setting.
    /// </summary>
    public long? OpsLimit { get; set; }

    /// <summary>
    /// memlimit is the maximum amount of RAM that the function will use, in bytes.
    /// When set along with OpsLimit, overrides the Strength setting.
    /// </summary>
    public int? MemLimit { get; set; }
}
