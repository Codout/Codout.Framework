using Codout.Framework.Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Codout.Framework.Application;

public static class RegisterServices
{
    public static IServiceCollection AddCrudAppServices(this IServiceCollection services)
    {
        return services
            .AddAutoMapper(typeof(MappingProfile))
            .AddScoped(typeof(ICrudAppService<,,>), typeof(CrudAppServiceBase<,,>));
    }
}