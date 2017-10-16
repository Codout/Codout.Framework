namespace Codout.Framework.DAL
{
    public interface ITenant 
    {
        string TenantKey { get; }

        string ConnectionString { get; }

        string AssemblyMappingName { get; }
    }
}
