using System.Globalization;
using System.Text;
using ASL.Core;
using HexLib;

namespace ASLInputTool.Infrastructure;

/// <summary>
/// Static provider for hex geometric calculations and path generation.
/// </summary>
public static class HexGeometryProvider
{
    private const double InnerScale = 0.8;

    /// <summary>
    /// Generates the SVG path data and WPF points for a hex.
    /// </summary>
    /// <param name="layout">The current map layout.</param>
    /// <param name="location">The hex location.</param>
    /// <param name="wpfCorners">Output array for WPF-compatible points.</param>
    /// <returns>SVG path data string.</returns>
    public static string GenerateHexPath(Layout layout, CubeCoordinate location, out System.Windows.Point[] wpfCorners)
    {
        var corners = layout.PolygonCorners(location);
        var sb = new StringBuilder();
        wpfCorners = new System.Windows.Point[6];
        
        for (int i = 0; i < 6; i++)
        {
            var p = corners[i];
            wpfCorners[i] = new System.Windows.Point(p.X, p.Y);
            if (i == 0) sb.Append("M "); else sb.Append("L ");
            sb.AppendFormat(CultureInfo.InvariantCulture, "{0:F2},{1:F2} ", p.X, p.Y);
        }
        sb.Append("Z");
        return sb.ToString();
    }

    /// <summary>
    /// Generates the SVG path data for the inner hex (frame for elevation).
    /// </summary>
    /// <param name="centerX">Hex center X.</param>
    /// <param name="centerY">Hex center Y.</param>
    /// <param name="size">Hex size (circumradius).</param>
    /// <returns>SVG path data string.</returns>
    public static string GenerateInnerHexPath(double centerX, double centerY, double size)
    {
        var sb = new StringBuilder();
        // Flat-topped hexes start with a corner at 0 degrees (due East).
        for (int i = 0; i < 6; i++)
        {
            double angleDeg = 60 * i;
            double angleRad = Math.PI / 180 * angleDeg;
            
            double px = centerX + (size * InnerScale) * Math.Cos(angleRad);
            double py = centerY + (size * InnerScale) * Math.Sin(angleRad);
            
            if (i == 0) sb.Append("M "); else sb.Append("L ");
            sb.AppendFormat(CultureInfo.InvariantCulture, "{0:F2},{1:F2} ", px, py);
        }
        sb.Append("Z");
        return sb.ToString();
    }
}
