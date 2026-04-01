using Codout.Security.Core;
using Microsoft.Extensions.Options;
using Sodium;

namespace Codout.Security.Scrypt
{
    public class ScryptPasswordHash(
        IOptions<ImprovedPasswordHasherOptions>? optionsAccessor = null,
        IOptions<ScryptOptions>? scryptOptionsAccessor = null) : IPasswordHasher
    {
        private readonly ImprovedPasswordHasherOptions _options = optionsAccessor?.Value ?? new ImprovedPasswordHasherOptions();
        private readonly ScryptOptions _scryptOptions = scryptOptionsAccessor?.Value ?? new ScryptOptions();

        public string HashPassword(string password)
        {
            if (string.IsNullOrEmpty(password))
                throw new ArgumentNullException(nameof(password), $"{nameof(password)} should not be null");
        
            if (_scryptOptions.OpsLimit.HasValue && _scryptOptions.MemLimit.HasValue)
                return PasswordHash.ScryptHashString(password, _scryptOptions.OpsLimit.Value, _scryptOptions.MemLimit.Value);

            return _options.Strength switch
            {
                PasswordHasherStrength.Interactive => PasswordHash.ScryptHashString(password),
                PasswordHasherStrength.Moderate => PasswordHash.ScryptHashString(password, PasswordHash.Strength.MediumSlow),
                PasswordHasherStrength.Sensitive => PasswordHash.ScryptHashString(password, PasswordHash.Strength.Sensitive),
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        public PasswordVerificationResult VerifyHashedPassword(string hashedPassword, string providedPassword)
        {
            if (string.IsNullOrEmpty(hashedPassword))
                throw new ArgumentNullException(nameof(hashedPassword), $"{nameof(hashedPassword)} should not be null");

            if (string.IsNullOrEmpty(providedPassword))
                throw new ArgumentNullException(nameof(providedPassword), $"{nameof(providedPassword)} should not be null");

            if (!PasswordHash.ScryptHashStringVerify(hashedPassword, providedPassword))
                return PasswordVerificationResult.Failed;

            return NeedsRehash(hashedPassword)
                ? PasswordVerificationResult.SuccessRehashNeeded
                : PasswordVerificationResult.Success;
        }

        private bool NeedsRehash(string hashedPassword)
        {
            // Scrypt hash format (libsodium/escrypt): $7$<N_log2_encoded><r_encoded><p_encoded><salt>$<hash>
            // The first char after "$7$" encodes N_log2 using itoa64
            if (!hashedPassword.StartsWith("$7$") || hashedPassword.Length < 4)
                return false;

            var storedNLog2 = DecodeItoa64(hashedPassword[3]);
            if (storedNLog2 < 0)
                return false;

            var expectedNLog2 = GetExpectedNLog2();
            return storedNLog2 < expectedNLog2;
        }

        private int GetExpectedNLog2()
        {
            if (_scryptOptions.OpsLimit.HasValue && _scryptOptions.MemLimit.HasValue)
            {
                // N ≈ memlimit / (r * 128), with default r=8 → N = memlimit / 1024
                var estimatedN = _scryptOptions.MemLimit.Value / 1024;
                return estimatedN > 0 ? (int)Math.Log2(estimatedN) : 14;
            }

            return _options.Strength switch
            {
                PasswordHasherStrength.Interactive => 14,  // N=2^14 (16 MiB)
                PasswordHasherStrength.Moderate => 17,     // N=2^17 (128 MiB, MediumSlow)
                PasswordHasherStrength.Sensitive => 20,    // N=2^20 (1 GiB)
                _ => 20
            };
        }

        /// <summary>
        /// Decodes a character from the itoa64 encoding used by libsodium's scrypt.
        /// Table: ./0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz
        /// </summary>
        private static int DecodeItoa64(char c) => c switch
        {
            '.' => 0,
            '/' => 1,
            >= '0' and <= '9' => c - '0' + 2,
            >= 'A' and <= 'Z' => c - 'A' + 12,
            >= 'a' and <= 'z' => c - 'a' + 38,
            _ => -1
        };
    }
}
