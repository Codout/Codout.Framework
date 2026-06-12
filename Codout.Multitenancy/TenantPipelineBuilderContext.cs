namespace Codout.Multitenancy;

public class TenantPipelineBuilderContext<TTenant> where TTenant : IAppTenant
{
    public TenantContext TenantContext { get; set; } = null!;
    public TTenant Tenant { get; set; } = default!;
}