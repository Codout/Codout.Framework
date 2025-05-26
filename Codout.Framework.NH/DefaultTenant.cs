using Codout.Framework.DAL;

namespace Codout.Framework.NH;

public class DefaultTenant : ITenant
{
    public DefaultTenant(string assemblyMappingName, string tenantKey, string connectionString)
    {
        AssemblyMappingName = assemblyMappingName;
        ConnectionString = connectionString;
        TenantKey = tenantKey;
    }

    public string TenantKey { get; }

    public string ConnectionString { get; }

    public string AssemblyMappingName { get; }
}