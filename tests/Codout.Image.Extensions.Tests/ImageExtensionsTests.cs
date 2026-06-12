using Codout.Image.Extensions;
using FluentAssertions;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using Xunit;

namespace Codout.Image.Extensions.Tests;

public class ImageExtensionsTests
{
    private static Image<Rgba32> CreateBlackImage(int width = 100, int height = 100)
    {
        return new Image<Rgba32>(width, height, new Rgba32(0, 0, 0, 255));
    }

    private static int CountWhitePixels(Image<Rgba32> image)
    {
        var white = new Rgba32(255, 255, 255, 255);
        var count = 0;
        for (var y = 0; y < image.Height; y++)
        for (var x = 0; x < image.Width; x++)
            if (image[x, y] == white)
                count++;
        return count;
    }

    [Fact]
    public void Extract_RecortaAreaInformada()
    {
        using var source = CreateBlackImage();

        using var extracted = source.Extract(new Rectangle(10, 10, 30, 20));

        extracted.Width.Should().Be(30);
        extracted.Height.Should().Be(20);

        // A imagem original não é alterada (Clone)
        source.Width.Should().Be(100);
        source.Height.Should().Be(100);
    }

    [Fact]
    public void Extract_PreservaConteudoDaArea()
    {
        using var source = CreateBlackImage();
        source[15, 15] = new Rgba32(255, 0, 0, 255); // pixel vermelho dentro da área

        using var raw = source.Extract(new Rectangle(10, 10, 20, 20));
        using var extracted = raw.CloneAs<Rgba32>();

        extracted[5, 5].Should().Be(new Rgba32(255, 0, 0, 255));
    }

    [Fact]
    public void Extract_ComMaxEdge_ReduzMantendoProporcao()
    {
        using var source = CreateBlackImage(200, 200);

        using var extracted = source.Extract(new Rectangle(0, 0, 80, 40), extractedMaxEdgeSize: 40);

        extracted.Width.Should().Be(40);
        extracted.Height.Should().Be(20);
    }

    [Fact]
    public void Extract_ComMaxEdgeMaiorQueArea_NaoAmpliaPorPadrao()
    {
        using var source = CreateBlackImage(200, 200);

        using var extracted = source.Extract(new Rectangle(0, 0, 20, 10), extractedMaxEdgeSize: 40);

        extracted.Width.Should().Be(20);
        extracted.Height.Should().Be(10);
    }

    [Fact]
    public void Extract_ComScaleUp_AmpliaAteMaxEdge()
    {
        using var source = CreateBlackImage(200, 200);

        using var extracted = source.Extract(new Rectangle(0, 0, 20, 10),
            extractedMaxEdgeSize: 40, scaleUpToMaxEdgeSize: true);

        extracted.Width.Should().Be(40);
        extracted.Height.Should().Be(20);
    }

    [Fact]
    public void Extract_AreaForaDosLimites_EhRecortadaParaDentroDaImagem()
    {
        using var source = CreateBlackImage(100, 100);

        // Área parcialmente fora da imagem é intersectada com os limites
        using var extracted = source.Extract(new Rectangle(80, 80, 50, 50), extractedMaxEdgeSize: 100);

        extracted.Width.Should().Be(20);
        extracted.Height.Should().Be(20);
    }

    [Fact]
    public void DrawRectangles_MutaAImagemOriginal()
    {
        using var image = CreateBlackImage();
        CountWhitePixels(image).Should().Be(0);

        image.DrawRectangles(Brushes.Solid(Color.White),
            new[] { new Rectangle(20, 20, 40, 40) }, thickness: 3);

        image.Width.Should().Be(100);
        image.Height.Should().Be(100);
        CountWhitePixels(image).Should().BeGreaterThan(0);
    }

    [Fact]
    public void DrawPoints_RetornaCopiaSemAlterarOriginal()
    {
        using var image = CreateBlackImage();

        using var raw = image.DrawPoints(Brushes.Solid(Color.White), new[] { new Point(50, 50) });
        using var result = raw.CloneAs<Rgba32>();

        // Original permanece intacta
        CountWhitePixels(image).Should().Be(0);

        // A cópia tem o ponto desenhado nas proximidades de (50,50)
        result.Should().NotBeSameAs(image);
        CountWhitePixels(result).Should().BeGreaterThan(0);
        result[50, 50].Should().Be(new Rgba32(255, 255, 255, 255));
    }

    [Fact]
    public void DrawRectanglesAndPoints_DesenhaAmbosNaCopia()
    {
        using var image = CreateBlackImage();

        using var raw = image.DrawRectanglesAndPoints(Brushes.Solid(Color.White),
            new[] { new RectangleF(10, 10, 30, 30) },
            new[] { new PointF(70, 70) });
        using var result = raw.CloneAs<Rgba32>();

        CountWhitePixels(image).Should().Be(0);
        CountWhitePixels(result).Should().BeGreaterThan(0);

        // Ponto desenhado próximo de (70,70)
        result[70, 70].Should().Be(new Rgba32(255, 255, 255, 255));
    }
}
