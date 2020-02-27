using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Codout.Multitenancy.Internal
{
	public class TenantResolutionMiddleware
    {
        private readonly RequestDelegate _next;

        public TenantResolutionMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context, ITenantResolver tenantResolver)
        {
            var tenantContext = await tenantResolver.ResolveAsync(context);

            if (tenantContext != null)
                context.SetTenantContext(tenantContext);

            await _next.Invoke(context);
        }
    }
}
