using Codout.Security.Core;
using Microsoft.Extensions.DependencyInjection;

namespace Codout.Security.Argon2;

public static class Argon2Extensions
{
    public static IServiceCollection UseArgon2(this IPasswordHashBuilder builder, Action<Argon2Options>? configure = null)
    {
        builder.Services.Configure<ImprovedPasswordHasherOptions>(options =>
        {
            options.Strength = builder.Options.Strength;
        });

        if (configure != null)
            builder.Services.Configure(configure);

        return builder.Services.AddScoped<IPasswordHasher, ArgonPasswordHash>();
    }
}