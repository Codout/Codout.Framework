namespace Codout.Security.Bcrypt;

public class BcryptOptions
{
    /// <summary>
    /// The log2 of the number of rounds of hashing to apply on BCrypt.
    /// The work factor therefore increases as 2**workFactor.
    /// Valid range: 4-31.
    /// </summary>
    public int WorkFactor { get; set; } = 12;

    /// <summary>
    /// The salt revision to use for BCrypt hashing.
    /// </summary>
    public BcryptSaltRevision SaltRevision { get; set; } = BcryptSaltRevision.Revision2B;
}
