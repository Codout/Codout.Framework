using Codout.Security.Core;
using Microsoft.Extensions.DependencyInjection;

namespace Codout.Security.Bcrypt;

public static class BcryptExtensions
{
    public static IServiceCollection UseBcrypt(this IPasswordHashBuilder builder, Action<BcryptOptions>? configure = null)
    {
        builder.Services.Configure<ImprovedPasswordHasherOptions>(options =>
        {
            options.Strength = builder.Options.Strength;
        });

        if (configure != null)
            builder.Services.Configure(configure);

        return builder.Services.AddScoped<IPasswordHasher, BcryptPasswordHash>();
    }
}