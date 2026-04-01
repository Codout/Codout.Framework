using Codout.Security.Core;
using Microsoft.Extensions.Options;
using Sodium;

namespace Codout.Security.Argon2
{
    public class ArgonPasswordHash(
        IOptions<ImprovedPasswordHasherOptions>? optionsAccessor = null,
        IOptions<Argon2Options>? argon2OptionsAccessor = null) : IPasswordHasher
    {
        private readonly ImprovedPasswordHasherOptions _options = optionsAccessor?.Value ?? new ImprovedPasswordHasherOptions();
        private readonly Argon2Options _argon2Options = argon2OptionsAccessor?.Value ?? new Argon2Options();

        public string HashPassword(string password)
        {
            if (string.IsNullOrEmpty(password))
                throw new ArgumentNullException(nameof(password), $"{nameof(password)} should not be null");

            if (_argon2Options.OpsLimit.HasValue && _argon2Options.MemLimit.HasValue)
                return PasswordHash.ArgonHashString(password, _argon2Options.OpsLimit.Value, _argon2Options.MemLimit.Value).TrimEnd('\0');

            return _options.Strength switch
            {
                PasswordHasherStrength.Interactive => PasswordHash.ArgonHashString(password).TrimEnd('\0'),
                PasswordHasherStrength.Moderate => PasswordHash.ArgonHashString(password, PasswordHash.StrengthArgon.Moderate).TrimEnd('\0'),
                PasswordHasherStrength.Sensitive => PasswordHash.ArgonHashString(password, PasswordHash.StrengthArgon.Sensitive).TrimEnd('\0'),
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        public PasswordVerificationResult VerifyHashedPassword(string hashedPassword, string providedPassword)
        {
            if (string.IsNullOrEmpty(hashedPassword))
                throw new ArgumentNullException(nameof(hashedPassword), $"{nameof(hashedPassword)} should not be null");

            if (string.IsNullOrEmpty(providedPassword))
                throw new ArgumentNullException(nameof(providedPassword), $"{nameof(providedPassword)} should not be null");

            if (!PasswordHash.ArgonHashStringVerify(hashedPassword, providedPassword))
                return PasswordVerificationResult.Failed;

            return NeedsRehash(hashedPassword)
                ? PasswordVerificationResult.SuccessRehashNeeded
                : PasswordVerificationResult.Success;
        }

        private bool NeedsRehash(string hashedPassword)
        {
            // Parse Argon2 hash format: $argon2id$v=19$m=65536,t=3,p=1$salt$hash
            var parts = hashedPassword.Split('$');
            if (parts.Length < 4)
                return false;

            var paramPart = parts[3];
            var parameters = paramPart.Split(',');

            long storedMemKib = 0;
            long storedTimeCost = 0;

            foreach (var param in parameters)
            {
                if (param.StartsWith("m=") && long.TryParse(param.AsSpan(2), out var m))
                    storedMemKib = m;
                else if (param.StartsWith("t=") && long.TryParse(param.AsSpan(2), out var t))
                    storedTimeCost = t;
            }

            if (storedMemKib == 0 || storedTimeCost == 0)
                return false;

            var (expectedTimeCost, expectedMemKib) = GetExpectedParameters();
            return storedTimeCost < expectedTimeCost || storedMemKib < expectedMemKib;
        }

        private (long timeCost, long memKib) GetExpectedParameters()
        {
            if (_argon2Options.OpsLimit.HasValue && _argon2Options.MemLimit.HasValue)
                return (_argon2Options.OpsLimit.Value, _argon2Options.MemLimit.Value / 1024);

            return _options.Strength switch
            {
                PasswordHasherStrength.Interactive => (2, 65536),       // 64 MiB
                PasswordHasherStrength.Moderate => (3, 262144),         // 256 MiB
                PasswordHasherStrength.Sensitive => (4, 1048576),       // 1 GiB
                _ => (4, 1048576)
            };
        }
    }
}
