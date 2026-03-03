namespace Codout.Framework.Data.Auditing;

/// <summary>
/// Defines the contract for providing the current user's identifier
/// </summary>
public interface ICurrentUserProvider
{
    /// <summary>
    /// Gets the identifier of the current user
    /// </summary>
    /// <returns>The current user's identifier, or null if not authenticated</returns>
    string? GetCurrentUserId();
}
