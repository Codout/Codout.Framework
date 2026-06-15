# Codout.Image.Extensions

Extensões para imagens baseadas em [SixLabors.ImageSharp](https://www.nuget.org/packages/SixLabors.ImageSharp): desenho de retângulos e pontos sobre imagens (ex.: marcação de faces e landmarks detectados) e extração/recorte de regiões com redimensionamento.

## Instalação

```bash
dotnet add package Codout.Image.Extensions
```

## Uso

A classe `ImageExtensions` expõe métodos de extensão sobre `SixLabors.ImageSharp.Image`.

Desenhar retângulos (mutação da imagem original) e pontos (retorna uma cópia):

```csharp
using Codout.Image.Extensions;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;

using var image = Image.Load("foto.jpg");
var brush = Brushes.Solid(Color.Red);

// Muta a imagem, desenhando os retângulos (espessura automática se omitida)
image.DrawRectangles(brush, new[] { new Rectangle(50, 40, 120, 120) });

// Retorna uma cópia com os pontos desenhados
using var comPontos = image.DrawPoints(brush, new[] { new Point(80, 90), new Point(140, 95) });
comPontos.Save("foto-marcada.jpg");
```

Extrair uma região da imagem, limitando o tamanho da maior aresta:

```csharp
using Codout.Image.Extensions;
using SixLabors.ImageSharp;

using var image = Image.Load("foto.jpg");

// Recorte simples
using var recorte = image.Extract(new Rectangle(50, 40, 300, 300));

// Recorte com redução para no máximo 128px na maior aresta
using var thumb = image.Extract(new Rectangle(50, 40, 300, 300), extractedMaxEdgeSize: 128);
thumb.Save("thumb.jpg");
```

Há ainda `DrawRectanglesAndPoints` para desenhar retângulos (`RectangleF`) e pontos (`PointF`) de uma só vez, retornando uma cópia da imagem.

## Pacotes relacionados

- [Codout.Framework.Storage](https://www.nuget.org/packages/Codout.Framework.Storage) — abstração de storage do ecossistema Codout, útil para persistir as imagens processadas.
- [Codout.Framework.Storage.Azure](https://www.nuget.org/packages/Codout.Framework.Storage.Azure) — implementação para Azure Blob Storage.

---
Parte do [Codout.Framework](https://github.com/Codout/Codout.Framework) — licença MIT.
