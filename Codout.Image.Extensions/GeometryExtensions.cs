﻿using System;
using SixLabors.ImageSharp;

namespace Codout.Image.Extensions;

public static class GeometryExtensions
{
    /// <summary>
    ///     Returns a square that contains the given rectangle in it's middle.
    /// </summary>
    /// <param name="rectangle">This rectangle's area should be in the center of the returned square.</param>
    /// <returns>A square shaped area.</returns>
    internal static Rectangle GetMinimumSupersetSquare(this Rectangle rectangle)
    {
        var center = Rectangle.Center(rectangle);
        var longerEdge = Math.Max(rectangle.Width, rectangle.Height);
        var halfLongerEdge = (longerEdge + 1) / 2; // +1 => Floor
        var minSuperSquare =
            new Rectangle(center.X - halfLongerEdge, center.Y - halfLongerEdge, longerEdge, longerEdge);
        return minSuperSquare;
    }

    internal static Size Scale(this Size size, double factor)
    {
        return new Size(
            (int)Math.Round(size.Width * factor),
            (int)Math.Round(size.Height * factor));
    }
}