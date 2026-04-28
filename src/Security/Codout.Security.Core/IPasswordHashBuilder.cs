using Microsoft.Extensions.DependencyInjection;

namespace Codout.Security.Core;

public interface IPasswordHashBuilder
{
    IServiceCollection Services { get; }
    ImprovedPasswordHasherOptions Options { get; }

    IPasswordHashBuilder WithStrength(PasswordHasherStrength strength);
}