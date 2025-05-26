using System;
using System.Threading.Tasks;
using Codout.Framework.Api.Client;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Codout.Framework.Api.Middleware;

public class ApiExceptionMiddleware(RequestDelegate next, ILogger<ApiExceptionMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext httpContext)
    {
        try
        {
            await next(httpContext);
        }
        catch (Exception ex)
        {
            logger.LogError(ex.Message, ex);
            await HandleExceptionAsync(httpContext, ex);
        }
    }

    private Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        return context.Response.WriteAsync(
            new ApiException(
                context.Response.StatusCode,
                exception.Message,
                new ApiErrorMessage(context.Response.StatusCode, exception.Message)
            ).ToString());
    }
}

public static class ApiExceptionMiddlewareExtensions
{
    public static void ConfigureExceptionMiddleware(this IApplicationBuilder app)
    {
        app.UseMiddleware<ApiExceptionMiddleware>();
    }
}