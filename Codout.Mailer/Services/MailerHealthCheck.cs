using System;
using System.Threading;
using System.Threading.Tasks;
using Codout.Mailer.Interfaces;
using Microsoft.Extensions.Diagnostics.HealthChecks;

public class MailerHealthCheck : IHealthCheck
{
    private readonly IMailerDispatcher _dispatcher;
    
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            // Implementar verificação de conectividade
            return HealthCheckResult.Healthy("Mailer service is healthy");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Mailer service is unhealthy", ex);
        }
    }
}