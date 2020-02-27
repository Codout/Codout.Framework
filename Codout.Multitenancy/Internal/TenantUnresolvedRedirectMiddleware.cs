using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Codout.Multitenancy.Internal
{
    public class TenantUnresolvedRedirectMiddleware<TTenant> where TTenant : IAppTenant
    {
        private readonly string _redirectLocation;
        private readonly bool _permanentRedirect;
        private readonly RequestDelegate _next;

        public TenantUnresolvedRedirectMiddleware(
            RequestDelegate next,
            string redirectLocation,
            bool permanentRedirect)
        {
            _next = next;
            _redirectLocation = redirectLocation;
            _permanentRedirect = permanentRedirect;
        }

        public async Task Invoke(HttpContext context)
        {
            var tenantContext = context.GetTenantContext();

            if (tenantContext == null)
            {
                Redirect(context, _redirectLocation);
                return;
            }

            await _next(context);
        }

        private void Redirect(HttpContext context, string redirectLocation)
        {
            context.Response.Redirect(redirectLocation);
            context.Response.StatusCode = _permanentRedirect ? StatusCodes.Status301MovedPermanently : StatusCodes.Status302Found;
        }
    }
}
