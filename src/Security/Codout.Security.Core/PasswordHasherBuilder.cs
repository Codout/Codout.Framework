using Microsoft.Extensions.DependencyInjection;

namespace Codout.Security.Core;

public class PasswordHasherBuilder(IServiceCollection services) : IPasswordHashBuilder
{
    public ImprovedPasswordHasherOptions Options { get; } = new();

    public IServiceCollection Services { get; } = services ?? throw new ArgumentNullException(nameof(services));

    /// <summary>
    /// Password Strength. Used to configure the hashing algorithm's resource usage.
    /// </summary>
    public IPasswordHashBuilder WithStrength(PasswordHasherStrength strength)
    {
        Options.Strength = strength;
        return this;
    }
}