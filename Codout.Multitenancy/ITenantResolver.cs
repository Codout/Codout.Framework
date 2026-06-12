using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Codout.Multitenancy;

public interface ITenantResolver
{
    // Contexto nullable-oblivious: implementações históricas tanto devolvem
    // TenantContext quanto null; manter o membro fora do contexto anotado
    // preserva a compatibilidade nas duas direções.
#nullable disable
    Task<TenantContext> ResolveAsync(HttpContext context);
#nullable restore
}
