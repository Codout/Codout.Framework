using Microsoft.Extensions.DependencyInjection;

namespace Codout.Security.Core
{
    public static class PasswordHasherServiceExtensions
    {
        public static IPasswordHashBuilder UseCustomHashPasswordBuilder(this IServiceCollection services)
        {
            return new PasswordHasherBuilder(services);
        }

        public static IPasswordHashBuilder UpgradePasswordSecurity(this IServiceCollection services)
        {
            return services.UseCustomHashPasswordBuilder();
        }

    }
}
