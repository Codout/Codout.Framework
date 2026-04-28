using Codout.Security.Core;
using Microsoft.Extensions.Options;

namespace Codout.Security.Bcrypt
{
    public class BcryptPasswordHash(IOptions<BcryptOptions>? bcryptOptionsAccessor = null) : IPasswordHasher
    {
        private readonly BcryptOptions _bcryptOptions = bcryptOptionsAccessor?.Value ?? new BcryptOptions();

        public string HashPassword(string password)
        {
            if (string.IsNullOrEmpty(password))
                throw new ArgumentNullException(nameof(password), $"{nameof(password)} should not be null");

            var salt = BCrypt.Net.BCrypt.GenerateSalt(_bcryptOptions.WorkFactor, GetSaltRevision());
            return BCrypt.Net.BCrypt.HashPassword(password, salt);
        }
       
        public PasswordVerificationResult VerifyHashedPassword(string hashedPassword, string providedPassword)
        {
            if (string.IsNullOrEmpty(hashedPassword))
                throw new ArgumentNullException(nameof(hashedPassword), $"{nameof(hashedPassword)} should not be null");

            if (string.IsNullOrEmpty(providedPassword))
                throw new ArgumentNullException(nameof(providedPassword), $"{nameof(providedPassword)} should not be null");

            if (!BCrypt.Net.BCrypt.Verify(providedPassword, hashedPassword))
                return PasswordVerificationResult.Failed;

            return BCrypt.Net.BCrypt.PasswordNeedsRehash(hashedPassword, _bcryptOptions.WorkFactor)
                ? PasswordVerificationResult.SuccessRehashNeeded
                : PasswordVerificationResult.Success;
        }

        private char GetSaltRevision() => _bcryptOptions.SaltRevision switch
        {
            BcryptSaltRevision.Revision2 => 'a',
            BcryptSaltRevision.Revision2A => 'a',
            BcryptSaltRevision.Revision2B => 'b',
            BcryptSaltRevision.Revision2X => 'x',
            BcryptSaltRevision.Revision2Y => 'y',
            _ => throw new ArgumentOutOfRangeException(nameof(_bcryptOptions.SaltRevision))
        };
    }
}
