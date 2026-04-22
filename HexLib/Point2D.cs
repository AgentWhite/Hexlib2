namespace HexLib;

/// <summary>
/// A simple, framework-independent 2D point using doubles.
/// Used for geometric calculations before converting to UI-specific types.
/// </summary>
public struct Point2D
{
    /// <summary>Gets the X double coordinate.</summary>
    public double X { get; }
    /// <summary>Gets the Y double coordinate.</summary>
    public double Y { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Point2D"/> struct.
    /// </summary>
    /// <param name="x">The X coordinate.</param>
    /// <param name="y">The Y coordinate.</param>
    public Point2D(double x, double y)
    {
        X = x;
        Y = y;
    }
}
