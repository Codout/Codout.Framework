using Codout.Multitenancy.Internal;
using Microsoft.AspNetCore.Builder;

namespace Codout.Multitenancy
{
    public static class MultitenancyApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseMultitenancy(this IApplicationBuilder app)
        {
            return app.UseMiddleware<TenantResolutionMiddleware>();
        }
    }
}
