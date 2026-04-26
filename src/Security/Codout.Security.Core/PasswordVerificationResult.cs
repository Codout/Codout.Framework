namespace Codout.Security.Core;

public enum PasswordVerificationResult
{
    Failed,
    Success,
    SuccessRehashNeeded
}