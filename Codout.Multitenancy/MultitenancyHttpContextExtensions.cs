using Microsoft.AspNetCore.Http;

namespace Codout.Multitenancy
{
    /// <summary>
    /// Multitenant extensions for <see cref="HttpContext"/>.
    /// </summary>
    public static class MultitenancyHttpContextExtensions
    {
        private const string TenantContextKey = "softprime.TenantContext";

        public static void SetTenantContext(this HttpContext context, TenantContext tenantContext)
        {
            context.Items[TenantContextKey] = tenantContext;
        }

        public static TenantContext GetTenantContext(this HttpContext context)
        {
            if (context.Items.TryGetValue(TenantContextKey, out var tenantContext))
                return tenantContext as TenantContext;

            return null;
        }

        public static TTenant GetTenant<TTenant>(this HttpContext context) where TTenant : IAppTenant
        { 
            var tenantContext = GetTenantContext(context);
            return (TTenant) (tenantContext != null ? tenantContext.Tenant : default(TTenant));
        }
    }
}
