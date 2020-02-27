namespace Codout.Multitenancy
{
    public class TenantPipelineBuilderContext<TTenant> where TTenant : IAppTenant
    {
        public TenantContext TenantContext { get; set; }
        public TTenant Tenant { get; set; }
    }
}
