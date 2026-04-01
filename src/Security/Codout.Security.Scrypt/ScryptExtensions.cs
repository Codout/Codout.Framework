using Codout.Security.Core;
using Microsoft.Extensions.DependencyInjection;

namespace Codout.Security.Scrypt;

public static class ScryptExtensions
{
    public static IServiceCollection UseScrypt(this IPasswordHashBuilder builder, Action<ScryptOptions>? configure = null)
    {
        builder.Services.Configure<ImprovedPasswordHasherOptions>(options =>
        {
            options.Strength = builder.Options.Strength;
        });

        if (configure != null)
            builder.Services.Configure(configure);

        return builder.Services.AddScoped<IPasswordHasher, ScryptPasswordHash>();
    }
}