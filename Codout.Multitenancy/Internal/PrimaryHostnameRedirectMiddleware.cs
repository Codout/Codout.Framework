using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;

namespace Codout.Multitenancy.Internal;

public class PrimaryHostnameRedirectMiddleware<TTenant> where TTenant : IAppTenant
{
    private readonly RequestDelegate _next;
    private readonly bool _permanentRedirect;
    private readonly Func<TTenant, string> _primaryHostnameAccessor;

    public PrimaryHostnameRedirectMiddleware(
        RequestDelegate next,
        Func<TTenant, string> primaryHostnameAccessor,
        bool permanentRedirect)
    {
        _next = next;
        _primaryHostnameAccessor = primaryHostnameAccessor;
        _permanentRedirect = permanentRedirect;
    }

    public async Task Invoke(HttpContext context)
    {
        var tenantContext = context.GetTenantContext();

        if (tenantContext != null)
        {
            var primaryHostname = _primaryHostnameAccessor((TTenant)tenantContext.Tenant);

            if (!string.IsNullOrWhiteSpace(primaryHostname))
                if (!context.Request.Host.Value.Equals(primaryHostname, StringComparison.OrdinalIgnoreCase))
                {
                    Redirect(context, primaryHostname);
                    return;
                }
        }

        // otherwise continue processing
        await _next(context);
    }

    private void Redirect(HttpContext context, string primaryHostname)
    {
        var builder = new UriBuilder(context.Request.GetEncodedUrl());
        builder.Host = primaryHostname;

        context.Response.Redirect(builder.Uri.AbsoluteUri);
        context.Response.StatusCode =
            _permanentRedirect ? StatusCodes.Status301MovedPermanently : StatusCodes.Status302Found;
    }
}