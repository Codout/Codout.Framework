using System.Collections.ObjectModel;

namespace Codout.Multitenancy;

public class MultitenancyOptions<TTenant> where TTenant : IAppTenant
{
    public Collection<TTenant> Tenants { get; set; }
}