namespace Codout.Security.Core;

public interface IPasswordHasher
{
    string HashPassword(string password);

    PasswordVerificationResult VerifyHashedPassword(
        string hashedPassword,
        string providedPassword);
}