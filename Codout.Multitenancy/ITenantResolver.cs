using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Codout.Multitenancy
{
    public interface ITenantResolver
    {
        Task<TenantContext> ResolveAsync(HttpContext context);
    }
}