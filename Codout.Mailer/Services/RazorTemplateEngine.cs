using System.Threading.Tasks;
using Codout.Mailer.Interfaces;
using RazorLight;

namespace Codout.Mailer.Services;

public class RazorTemplateEngine(RazorLightEngine engine) : ITemplateEngine
{
    public async Task<string> RenderAsync<T>(string templateKey, T model)
    {
        return await engine.CompileRenderAsync(templateKey, model);
    }
}