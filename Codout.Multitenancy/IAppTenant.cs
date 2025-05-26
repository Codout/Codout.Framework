namespace Codout.Multitenancy;

public interface IAppTenant
{
    string TenantKey { get; set; }
    DataBaseType DataBaseType { get; set; }
    string ConnectionString { get; set; }
}