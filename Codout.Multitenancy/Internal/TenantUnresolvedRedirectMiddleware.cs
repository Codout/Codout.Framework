using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Codout.Multitenancy.Internal;

public class TenantUnresolvedRedirectMiddleware<TTenant>(
    RequestDelegate next,
    string redirectLocation,
    bool permanentRedirect)
    where TTenant : IAppTenant
{
    public async Task Invoke(HttpContext context)
    {
        var tenantContext = context.GetTenantContext();

        if (tenantContext == null)
        {
            Redirect(context, redirectLocation);
            return;
        }

        await next(context);
    }

    private void Redirect(HttpContext context, string redirectLocation)
    {
        context.Response.Redirect(redirectLocation);
        context.Response.StatusCode =
            permanentRedirect ? StatusCodes.Status301MovedPermanently : StatusCodes.Status302Found;
    }
}