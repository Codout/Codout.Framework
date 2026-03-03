using System;
using System.IO;
using System.Threading.Tasks;
using Codout.Mailer.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;

namespace Codout.Mailer.Razor;

public class RazorViewTemplateEngine(
    IRazorViewEngine viewEngine,
    ITempDataProvider tempDataProvider,
    IServiceProvider serviceProvider) : ITemplateEngine
{
    public async Task<string> RenderAsync<T>(string templateKey, T model)
    {
        var httpContext = new DefaultHttpContext { RequestServices = serviceProvider };
        var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor());

        var viewResult = viewEngine.GetView(null, templateKey, false);

        if (!viewResult.Success)
            viewResult = viewEngine.FindView(actionContext, templateKey, false);

        if (!viewResult.Success)
        {
            var searchedLocations = string.Join(", ", viewResult.SearchedLocations ?? []);
            throw new InvalidOperationException(
                $"Template '{templateKey}' não encontrado. Locais pesquisados: {searchedLocations}");
        }

        await using var writer = new StringWriter();

        var viewData = new ViewDataDictionary<T>(
            new EmptyModelMetadataProvider(),
            new ModelStateDictionary())
        {
            Model = model
        };

        var tempData = new TempDataDictionary(httpContext, tempDataProvider);

        var viewContext = new ViewContext(
            actionContext,
            viewResult.View,
            viewData,
            tempData,
            writer,
            new HtmlHelperOptions());

        await viewResult.View.RenderAsync(viewContext);

        return writer.ToString();
    }
}
