using System.Threading.Tasks;

namespace Codout.Mailer.Interfaces;

public interface ITemplateEngine
{
    Task<string> RenderAsync<T>(string templateKey, T model);
}