using System.Collections.Generic;

namespace HexLib;

/// <summary>
/// Combines Orientation, Size, and Origin to translate between CubeCoordinates and Point2D pixels.
/// </summary>
public class Layout
{
    public Orientation Orientation { get; }
    public Point2D Size { get; }
    public Point2D Origin { get; }

    public Layout(Orientation orientation, Point2D size, Point2D origin)
    {
        Orientation = orientation;
        Size = size;
        Origin = origin;
    }

    /// <summary>
    /// Translates a logical cube coordinate to its center pixel position.
    /// </summary>
    public Point2D HexToPixel(CubeCoordinate h)
    {
        var m = Orientation;
        double x = (m.F0 * h.Q + m.F1 * h.R) * Size.X;
        double y = (m.F2 * h.Q + m.F3 * h.R) * Size.Y;
        return new Point2D(x + Origin.X, y + Origin.Y);
    }

    /// <summary>
    /// Translates a center pixel position to its corresponding fractional hex.
    /// Call <see cref="FractionalHex.Round"/> on the result to get a discrete coordinate.
    /// </summary>
    public FractionalHex PixelToHex(Point2D p)
    {
        var m = Orientation;
        Point2D pt = new Point2D((p.X - Origin.X) / Size.X, (p.Y - Origin.Y) / Size.Y);
        double q = m.B0 * pt.X + m.B1 * pt.Y;
        double r = m.B2 * pt.X + m.B3 * pt.Y;
        return new FractionalHex(q, r, -q - r);
    }

    /// <summary>
    /// Calculates the pixel offset for a single corner (0-5) relative to the center.
    /// </summary>
    public Point2D HexCornerOffset(int corner)
    {
        double angle = 2.0 * Math.PI * (Orientation.StartAngle + corner) / 6.0;
        return new Point2D(Size.X * Math.Cos(angle), Size.Y * Math.Sin(angle));
    }

    /// <summary>
    /// Returns the 6 vertices of a hex in clockwise order.
    /// </summary>
    public List<Point2D> PolygonCorners(CubeCoordinate h)
    {
        var corners = new List<Point2D>();
        var center = HexToPixel(h);
        for (int i = 0; i < 6; i++)
        {
            var offset = HexCornerOffset(i);
            corners.Add(new Point2D(center.X + offset.X, center.Y + offset.Y));
        }
        return corners;
    }

    /// <summary>
    /// Gets the midpoint of a specific hex edge (0-5).
    /// </summary>
    public Point2D HexEdgeMidpoint(CubeCoordinate h, int edge)
    {
        var corners = PolygonCorners(h);
        var p1 = corners[edge];
        var p2 = corners[(edge + 1) % 6];
        return new Point2D((p1.X + p2.X) / 2.0, (p1.Y + p2.Y) / 2.0);
    }
}
