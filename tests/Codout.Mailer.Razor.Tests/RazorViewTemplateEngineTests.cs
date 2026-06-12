using Codout.Mailer.Razor;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Codout.Mailer.Razor.Tests;

/// <summary>
/// Testes unitários do RazorViewTemplateEngine usando IRazorViewEngine fake —
/// cobrem a lógica de resolução de view e do pipeline de renderização sem
/// precisar de host ASP.NET Core nem compilação Razor real (esta é coberta
/// pelos testes de integração em RazorRenderingIntegrationTests).
/// </summary>
public class RazorViewTemplateEngineTests
{
    private readonly Mock<Microsoft.AspNetCore.Mvc.Razor.IRazorViewEngine> _viewEngine = new();
    private readonly Mock<ITempDataProvider> _tempDataProvider = new();
    private readonly IServiceProvider _serviceProvider = new ServiceCollection().BuildServiceProvider();

    public RazorViewTemplateEngineTests()
    {
        _tempDataProvider
            .Setup(p => p.LoadTempData(It.IsAny<Microsoft.AspNetCore.Http.HttpContext>()))
            .Returns(new Dictionary<string, object?>());
    }

    private RazorViewTemplateEngine CreateEngine() =>
        new(_viewEngine.Object, _tempDataProvider.Object, _serviceProvider);

    [Fact]
    public async Task RenderAsync_QuandoViewNaoEncontrada_LancaComLocaisPesquisados()
    {
        _viewEngine
            .Setup(e => e.GetView(null, "Inexistente", false))
            .Returns(ViewEngineResult.NotFound("Inexistente", ["/Views/Inexistente.cshtml"]));
        _viewEngine
            .Setup(e => e.FindView(It.IsAny<ActionContext>(), "Inexistente", false))
            .Returns(ViewEngineResult.NotFound("Inexistente", ["/Templates/Inexistente.cshtml"]));

        var acao = () => CreateEngine().RenderAsync("Inexistente", new WelcomeModel());

        (await acao.Should().ThrowAsync<InvalidOperationException>())
            .WithMessage("*'Inexistente'*")
            .WithMessage("*/Templates/Inexistente.cshtml*");
    }

    [Fact]
    public async Task RenderAsync_QuandoGetViewEncontra_DeveRenderizarEscrevendoNoWriter()
    {
        WelcomeModel? modelRecebido = null;

        var view = new Mock<IView>();
        view.Setup(v => v.RenderAsync(It.IsAny<ViewContext>()))
            .Returns<ViewContext>(async context =>
            {
                modelRecebido = context.ViewData.Model as WelcomeModel;
                await context.Writer.WriteAsync("<p>renderizado</p>");
            });

        _viewEngine
            .Setup(e => e.GetView(null, "/Templates/Welcome.cshtml", false))
            .Returns(ViewEngineResult.Found("/Templates/Welcome.cshtml", view.Object));

        var model = new WelcomeModel { Nome = "Maria" };

        var html = await CreateEngine().RenderAsync("/Templates/Welcome.cshtml", model);

        html.Should().Be("<p>renderizado</p>");
        modelRecebido.Should().BeSameAs(model);
        _viewEngine.Verify(e => e.FindView(It.IsAny<ActionContext>(), It.IsAny<string>(), It.IsAny<bool>()),
            Times.Never, "GetView resolveu, então FindView não deve ser consultado");
    }

    [Fact]
    public async Task RenderAsync_QuandoGetViewFalha_DeveTentarFindView()
    {
        var view = new Mock<IView>();
        view.Setup(v => v.RenderAsync(It.IsAny<ViewContext>()))
            .Returns<ViewContext>(context => context.Writer.WriteAsync("via FindView"));

        _viewEngine
            .Setup(e => e.GetView(null, "Welcome", false))
            .Returns(ViewEngineResult.NotFound("Welcome", []));
        _viewEngine
            .Setup(e => e.FindView(It.IsAny<ActionContext>(), "Welcome", false))
            .Returns(ViewEngineResult.Found("Welcome", view.Object));

        var html = await CreateEngine().RenderAsync("Welcome", new WelcomeModel());

        html.Should().Be("via FindView");
    }

    [Fact]
    public async Task RenderAsync_DevePropagarExcecaoDaView()
    {
        var view = new Mock<IView>();
        view.Setup(v => v.RenderAsync(It.IsAny<ViewContext>()))
            .ThrowsAsync(new InvalidOperationException("erro na view"));

        _viewEngine
            .Setup(e => e.GetView(null, "Quebrada", false))
            .Returns(ViewEngineResult.Found("Quebrada", view.Object));

        var acao = () => CreateEngine().RenderAsync("Quebrada", new WelcomeModel());

        (await acao.Should().ThrowAsync<InvalidOperationException>())
            .WithMessage("erro na view");
    }
}
